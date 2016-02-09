using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using bluetooth.ble.gatt;
using nexus.core.logging;

namespace bluetooth.ble
{
   public class BluetoothLowEnergyAdapter
      : BaseBluetoothLowEnergyAdapter<BluetoothLowEnergyPeripheral, GattServerConnection>,
        IBluetoothLowEnergyAdapter
   {
      private readonly BluetoothLowEnergyScanner m_scanner;

      public BluetoothLowEnergyAdapter( BluetoothAdapter adapter )
      {
         m_scanner = new BluetoothLowEnergyScanner( adapter );
         m_scanner.AdvertisementDiscovered += bluetooth_AdvertisementDiscovered;
      }

      public override void Dispose()
      {
         m_scanner.AdvertisementDiscovered -= bluetooth_AdvertisementDiscovered;
         m_scanner.Dispose();
         foreach(var server in connectedDevices)
         {
            server.Disconnected -= server_Disconnected;
         }
         base.Dispose();
      }

      protected override void ConnectToDeviceInternal( TaskCompletionSource<IGattServerConnection> tcs,
                                                       BluetoothLowEnergyPeripheral device )
      {
         var server = GattServerConnection.Connect( device, Application.Context, tcs.SetResult );
         server.Disconnected += server_Disconnected;
         connectedDevices.Add( server );
      }

      protected override void StartScanInternal( Guid serviceUuid )
      {
         m_scanner.StartScan();
      }

      protected override void StopScanInternal()
      {
         m_scanner.StopScan();
      }

      private void bluetooth_AdvertisementDiscovered( BluetoothDevice nativeDevice, Int32 rssi, Byte[] advertisement )
      {
         var address = nativeDevice.Address;
         if(discoveredDevices.All( d => d.NativeDevice.Address != address ))
         {
            Log.Debug( "scan found new device: " + address );
            TrackDiscoveredDevice( new BluetoothLowEnergyPeripheral( nativeDevice, advertisement, rssi ) );
         }
         else
         {
            Log.Debug( "scan found existing device: " + address );
            OnAdvertisementDiscovered( discoveredDevices.First( d => d.NativeDevice.Address == address ) );
         }
      }

      private void server_Disconnected( GattServerConnection connection )
      {
         if(connection.IsDisposed)
         {
            connectedDevices.Remove( connection );
         }
      }
   }
}