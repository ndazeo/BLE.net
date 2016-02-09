using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bluetooth.ble.gatt;
using CoreBluetooth;
using CoreFoundation;
using Foundation;
using nexus.core;
using nexus.core.logging;

namespace bluetooth.ble
{
   public class BluetoothLowEnergyAdapter
      : BaseBluetoothLowEnergyAdapter<BluetoothLowEnergyPeripheral, GattServerConnection>,
        IBluetoothLowEnergyAdapter
   {
      protected readonly IDictionary<Guid, TaskCompletionSource<IGattServerConnection>> m_pendingConnections;
      private readonly CBCentralManager m_bluetooth;
      private Guid? m_startScanOnPoweredOn;

      public BluetoothLowEnergyAdapter()
      {
         m_pendingConnections = new Dictionary<Guid, TaskCompletionSource<IGattServerConnection>>();
         m_bluetooth = new CBCentralManager( DispatchQueue.CurrentQueue );
         m_bluetooth.DiscoveredPeripheral += bluetooth_AdvertisementDiscovered;
         m_bluetooth.UpdatedState += bluetooth_UpdatedState;
         m_bluetooth.FailedToConnectPeripheral += bluetooth_FailedToConnectPeripheral;
         m_bluetooth.DisconnectedPeripheral += bluetooth_DisconnectedPeripheral;
         m_bluetooth.ConnectedPeripheral += bluetooth_ConnectedPeripheral;

         m_bluetooth.RetrievedConnectedPeripherals += ( sender, args ) => Log.Info( "RetrievedConnectedPeripherals" );
         m_bluetooth.RetrievedPeripherals += ( sender, args ) => Log.Info( "RetrievedPeripherals" );
         m_bluetooth.WillRestoreState += ( sender, args ) => Log.Info( "WillRestoreState" );
      }

      public override void Dispose()
      {
         Log.Debug( "Disposing iOS BluetoothLowEnergyAdapter" );
         m_bluetooth.DiscoveredPeripheral -= bluetooth_AdvertisementDiscovered;
         m_bluetooth.UpdatedState -= bluetooth_UpdatedState;
         m_bluetooth.FailedToConnectPeripheral -= bluetooth_FailedToConnectPeripheral;
         m_bluetooth.DisconnectedPeripheral -= bluetooth_DisconnectedPeripheral;
         m_bluetooth.ConnectedPeripheral -= bluetooth_ConnectedPeripheral;
         m_bluetooth.Dispose();
         base.Dispose();
      }

      protected override void ConnectToDeviceInternal( TaskCompletionSource<IGattServerConnection> tcs,
                                                       BluetoothLowEnergyPeripheral device )
      {
         m_pendingConnections.Add( device.Id, tcs );
         m_bluetooth.ConnectPeripheral(
            device.NativeDevice,
            new PeripheralConnectionOptions {NotifyOnDisconnection = true, NotifyOnConnection = true} );
      }

      protected override void StartScanInternal( Guid serviceUuid )
      {
         if(m_bluetooth.State != CBCentralManagerState.PoweredOn)
         {
            Log.Info( "Turning on bluetooth" );
            m_startScanOnPoweredOn = serviceUuid;
         }
         else if(IsScanningForAdvertisements)
            //ensure we're scanning in case this is being called from the poweredon callback
         {
            if(serviceUuid != Guid.Empty)
            {
               Log.Trace( "Initiating advertisement scan for {0}...", serviceUuid );
               m_bluetooth.ScanForPeripherals( CBUUID.FromString( serviceUuid.ToString() ) );
            }
            else
            {
               Log.Trace( "Initiating advertisement scan" );
               m_bluetooth.ScanForPeripherals( new CBUUID[] {} );
            }
         }
      }

      protected override void StopScanInternal()
      {
         m_bluetooth.StopScan();
      }

      private void bluetooth_AdvertisementDiscovered( Object sender, CBDiscoveredPeripheralEventArgs e )
      {
         var nativeDevice = e.Peripheral;
         // check if device exists so we don't add duplicates
         if(discoveredDevices.All( d => !nativeDevice.Identifier.Equals( d.NativeDevice.Identifier ) ))
         {
            Log.Debug(
               "scan found new device: {0} {1} {2}",
               nativeDevice.Name,
               nativeDevice.Identifier,
               e.AdvertisementData );
            //kCBAdvDataIsConnectable 
            //kCBAdvDataManufacturerData
            //kCBAdvDataServiceData
            //kCBAdvDataServiceUUIDs
            //kCBAdvDataChannel = 38;
            //kCBAdvDataLocalName = "JhBC_ZDHTMDRNg";
            //kCBAdvDataTxPowerLevel = 0;
            var iConnectable =
               ((NSNumber)e.AdvertisementData.ObjectForKey( new NSString( "kCBAdvDataIsConnectable" ) )).Int32Value == 1;
            var kCBAdvDataManufacturerData =
               e.AdvertisementData.ObjectForKey( new NSString( "kCBAdvDataManufacturerData" ) );
            var kCBAdvDataServiceData =
               (NSDictionary)e.AdvertisementData.ObjectForKey( new NSString( "kCBAdvDataServiceData" ) );
            var advertisedServices =
               (NSArray)e.AdvertisementData.ObjectForKey( new NSString( "kCBAdvDataServiceUUIDs" ) );
            //Log.Debug( "Device " + (iConnectable ? "is" : "is not") + " open for connections" );
            if(kCBAdvDataServiceData != null)
            {
               //Log.Info( "kCBAdvDataServiceData {0}", kCBAdvDataServiceData );
               //.ObjectForKey( new NSString( "Device Information" ) ) );
            }
            if(kCBAdvDataManufacturerData != null)
            {
               //Log.Info( "kCBAdvDataManufacturerData {0}", kCBAdvDataManufacturerData );
            }
            if(advertisedServices != null)
            {
               var services = NSArray.FromArray<CBUUID>( advertisedServices ).Select( x => x.ToGuid() ).ToList();
               //Log.Info( "service guids: {0}", String.Join( ", ", services ) );
            }
            TrackDiscoveredDevice(
               new BluetoothLowEnergyPeripheral(
                  nativeDevice,
                  null,
                  e.RSSI.Int32Value
                  // TODO: handle advertisement data object
/*e.AdvertisementData*/ ) );
         }
         else
         {
            var peripheral = discoveredDevices.First( d => d.NativeDevice.Equals( nativeDevice ) );
            Log.Debug(
               "scan found existing device: name={0} uuid(n)={1} uuid(x)={2}",
               nativeDevice.Name,
               nativeDevice.Identifier,
               peripheral.Id );
            peripheral.SetRssi( e.RSSI.Int32Value );
            //peripheral.SetAdvertisement( e.AdvertisementData );
            OnAdvertisementDiscovered( peripheral );
         }
      }

      private void bluetooth_ConnectedPeripheral( Object sender, CBPeripheralEventArgs e )
      {
         Log.Debug( "bluetooth_ConnectedPeripheral. connected to {0}", e.Peripheral.Name );

         var guid = e.Peripheral.Identifier.ToGuid();
         var connection = connectedDevices.FirstOrDefault( c => c.Id == guid );
         if(connection != null)
         {
            Log.Debug( "bluetooth_ConnectedPeripheral. Found existing connection" );
            // TODO: Dispose of existing object graph and use new object
            //connection.ChangeNativeDevice( e.Peripheral );
         }
         else
         {
            var peripheral = discoveredDevices.First( d => d.Id == guid );
            Log.Debug(
               "bluetooth_ConnectedPeripheral. creating new connection instance to name={0} guid={1}",
               peripheral.Name,
               peripheral.Id );
            connection = new GattServerConnection( peripheral );
            connectedDevices.Add( connection );
         }

         var tcs = m_pendingConnections.Get( guid );
         if(tcs != null)
         {
            m_pendingConnections.Remove( guid );
            tcs.SetResult( connection );
         }
      }

      private void bluetooth_DisconnectedPeripheral( Object sender, CBPeripheralErrorEventArgs e )
      {
         var guid = e.Peripheral.Identifier.ToGuid();
         Log.Debug( "bluetooth_DisconnectedPeripheral name={0} guid={1}", e.Peripheral.Name, guid );
         var connection = connectedDevices.FirstOrDefault( d => d.Id == guid );
         if(connection == null || connection.IsDisposed)
         {
            if(connection != null)
            {
               connectedDevices.Remove( connection );
            }
            var peripheral = discoveredDevices.FirstOrDefault( d => d.Id == guid );
            if(peripheral != null)
            {
               //OnDeviceDisconnected( peripheral );
            }
            m_bluetooth.CancelPeripheralConnection( e.Peripheral );
         }
         else
         {
            // Try to reconnect
            Log.Info(
               "bluetooth_DisconnectedPeripheral. TODO: Try to reconnect name={0} guid={1}",
               connection.Name,
               connection.Id );
            m_bluetooth.ConnectPeripheral(
               discoveredDevices.First( d => d.Id == guid ).NativeDevice,
               new PeripheralConnectionOptions {NotifyOnDisconnection = true, NotifyOnConnection = true} );
         }
      }

      private void bluetooth_FailedToConnectPeripheral( Object sender, CBPeripheralErrorEventArgs e )
      {
         Log.Warn( "bluetooth_FailedToConnectPeripheral" );
         var device =
            discoveredDevices.FirstOrDefault( x => Equals( x.NativeDevice.Identifier, e.Peripheral.Identifier ) );
         if(device != null)
         {
            // TODO: Handle this somehow
            // DeviceFailedToConnect( device );
         }
      }

      private void bluetooth_UpdatedState( Object sender, EventArgs e )
      {
         Log.Debug( "bluetooth connection state changed to " + m_bluetooth.State );
         if(m_bluetooth.State == CBCentralManagerState.PoweredOn && m_startScanOnPoweredOn != null)
         {
            StartScanInternal( m_startScanOnPoweredOn.Value );
            m_startScanOnPoweredOn = null;
         }
      }
   }
}