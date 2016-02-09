using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using nexus.core;
using nexus.core.logging;

namespace bluetooth.ble.gatt
{
   public abstract class BaseGattServerConnection<TNativeDevice> : IGattServerConnection
      where TNativeDevice : class
   {
      protected readonly BaseBluetoothLowEnergyPeripheral<TNativeDevice> device;

      public abstract IEnumerable<IGattService> DiscoveredServices { get; }

      public abstract ConnectionState State { get; }

      public abstract void DiscoverAllServices();

      public abstract void Dispose();

      protected BaseGattServerConnection( BaseBluetoothLowEnergyPeripheral<TNativeDevice> device )
      {
         this.device = device;
      }

      public event Action<IGattServerConnection> ServicesDiscovered;

      public Byte[] Advertisement => device.Advertisement;

      public Guid Id => device.Id;

      public String Name => device.Name;

      public Int32 Rssi => device.Rssi;

      public override Boolean Equals( Object obj )
      {
         var ch = obj as IGattServerConnection;
         return ch != null && ch.Id == Id;
      }

      public override Int32 GetHashCode()
      {
         return Id.GetHashCode();
      }

      public Task<IGattService> GetService( Guid serviceId )
      {
         return this.GetService( serviceId, new CancellationTokenSource( TimeSpan.FromSeconds( 5 ) ).Token );
      }

      public override String ToString()
      {
         return Name.IsNullOrWhiteSpace() ? Id.ToString() : "{0} {1}".F( Name, Id );
      }

      protected void OnServicesDiscovered()
      {
         var func = ServicesDiscovered;
         if(func != null)
         {
            try
            {
               func( this );
            }
            catch(Exception ex)
            {
               Log.Info( ex );
            }
         }
      }

      protected void SetNativeDevice( TNativeDevice newNativeDevice )
      {
         device.SetNativeDevice( newNativeDevice );
      }
   }
}