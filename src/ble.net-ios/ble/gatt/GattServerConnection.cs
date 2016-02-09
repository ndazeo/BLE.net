using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;
using nexus.core;
using nexus.core.logging;

namespace bluetooth.ble.gatt
{
   public class GattServerConnection
      : BaseGattServerConnection<CBPeripheral>,
        IGattServerConnection
   {
      protected readonly IList<GattService> m_discoveredServices;

      public GattServerConnection( BluetoothLowEnergyPeripheral device )
         : base( device )
      {
         m_discoveredServices = new List<GattService>();
         ChangeNativeDevice( device.NativeDevice );
      }

      public override IEnumerable<IGattService> DiscoveredServices
      {
         get { return m_discoveredServices; }
      }

      public Boolean IsDisposed { get; private set; }

      public override ConnectionState State
      {
         get
         {
            switch(device.NativeDevice.State)
            {
               case CBPeripheralState.Connected:
                  return ConnectionState.Connected;
               case CBPeripheralState.Connecting:
                  return ConnectionState.Connecting;
               case CBPeripheralState.Disconnected:
               default:
                  return ConnectionState.Disconnected;
            }
         }
      }

      public override void DiscoverAllServices()
      {
         if(device.NativeDevice.Services != null && device.NativeDevice.Services.Length > 0)
         {
            Log.Debug(
               "Current services: {0}",
               String.Join( ",", device.NativeDevice.Services.Select( x => x.UUID.ToString() ) ) );
         }
         device.NativeDevice.DiscoverServices();
         Log.Debug( "iOS.GattServerConnection.DiscoverAllServices" );
      }

      public override void Dispose()
      {
         if(IsDisposed)
         {
            return;
         }

         IsDisposed = true;
         Log.Debug( "iOS.GattServerConnection.Dispose" );
         foreach(var s in m_discoveredServices)
         {
            s.Dispose();
         }
         m_discoveredServices.Clear();
         device.NativeDevice.DiscoveredService -= connection_ServicesDiscovered;
         device.NativeDevice.DiscoveredIncludedService -= connection_IncludedServiceDiscovered;
         device.NativeDevice.Dispose();
      }

      internal void ChangeNativeDevice( CBPeripheral newNativeDevice )
      {
         if(device.NativeDevice != null)
         {
            device.NativeDevice.DiscoveredService -= connection_ServicesDiscovered;
            device.NativeDevice.DiscoveredIncludedService -= connection_IncludedServiceDiscovered;

            //var native = base.device.NativeDevice;
            //native.DiscoveredDescriptor += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.DiscoveredDescriptor" );
            //native.DiscoveredIncludedService += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.DiscoveredIncludedService" );
            //native.DiscoveredService += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.DiscoveredService" );
            //native.InvalidatedService += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.InvalidatedService" );
            //native.ModifiedServices += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.ModifiedServices" );
            //native.RssiRead += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.RssiRead" );
            //native.RssiUpdated += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.RssiUpdated" );
            //native.UpdatedCharacterteristicValue += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.UpdatedCharacterteristicValue {0}", args );
            //native.UpdatedName += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.UpdatedName" );
            //native.UpdatedNotificationState += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.UpdatedNotificationState {0}", args );
            //native.UpdatedValue += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.UpdatedValue {0}", args );
            //native.WroteCharacteristicValue += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.WroteCharacteristicValue {0}", args );
            //native.WroteDescriptorValue += ( sender, args ) => Log.Debug( "iOS.GattServerConnection.WroteDescriptorValue {0}", args );
         }
         SetNativeDevice( newNativeDevice );
         device.NativeDevice.DiscoveredService += connection_ServicesDiscovered;
         device.NativeDevice.DiscoveredIncludedService += connection_IncludedServiceDiscovered;
      }

      private void connection_IncludedServiceDiscovered( Object sender, CBServiceEventArgs e )
      {
         Log.Debug( "iOS.GattServerConnection discovered-service={0}", e.Service.UUID );
      }

      private async void connection_ServicesDiscovered( Object sender, NSErrorEventArgs e )
      {
         if(device.NativeDevice.Services != null && device.NativeDevice.Services.Length > 0)
         {
            Log.Debug(
               "Current services: {0}",
               String.Join( ",", device.NativeDevice.Services.Select( x => x.UUID.ToString() ) ) );
         }
         Log.Debug( "iOS.GattServerConnection.connection_ServicesDiscovered {0}", m_discoveredServices.Count );
         m_discoveredServices.AddAll(
            device.NativeDevice.Services.Where(
               nativeService => m_discoveredServices.All( s => s.Id != nativeService.UUID.ToGuid() ) )
                  .Select(
                     nativeService =>
                     {
                        device.NativeDevice.DiscoverCharacteristics( nativeService );
                        return new GattService( device.NativeDevice, nativeService );
                     } ) );
         // I am at the end of my wits dealing with this horrid API. I don't fucking care.
         Log.Debug( "sleeping..." );
         await Task.Delay( 1000 );
         Log.Debug( "done sleeping, triggering OnServicesDiscovered()" );
         OnServicesDiscovered();
      }
   }
}