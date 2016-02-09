using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using Android.Content;
using nexus.core;
using nexus.core.logging;

namespace bluetooth.ble.gatt
{
   public class GattServerConnection
      : BaseGattServerConnection<BluetoothDevice>,
        IGattServerConnection
   {
      private readonly IBluetoothGattConnection m_connection;
      private readonly IList<GattService> m_discoveredServices;
      private readonly List<Action<GattServerConnection>> m_onConnectedActions;
      private ConnectionState m_connectionState;

      private GattServerConnection( BluetoothLowEnergyPeripheral device )
         : base( device )
      {
         m_discoveredServices = new List<GattService>();
         m_onConnectedActions = new List<Action<GattServerConnection>>();
         m_connectionState = ConnectionState.Disconnected;
         m_connection = new BluetoothGattConnection();
         m_connection.ServicesDiscovered += connection_ServicesDiscovered;
         m_connection.ConnectionStateChanged += connection_ConnectionStateChanged;
      }

      public event Action<GattServerConnection> Disconnected;

      public override IEnumerable<IGattService> DiscoveredServices
      {
         get { return m_discoveredServices; }
      }

      public Boolean IsDisposed { get; private set; }

      public override ConnectionState State
      {
         get { return m_connectionState; }
      }

      public override void DiscoverAllServices()
      {
         Log.Debug( "DiscoverAllServices" );
         m_connection.DiscoverServices();
      }

      public override void Dispose()
      {
         if(IsDisposed)
         {
            return;
         }

         IsDisposed = true;
         Log.Debug( "Dispose" );
         foreach(var s in m_discoveredServices)
         {
            s.Dispose();
         }
         m_discoveredServices.Clear();
         m_connection.ServicesDiscovered -= connection_ServicesDiscovered;
         m_connection.ConnectionStateChanged -= connection_ConnectionStateChanged;
         m_connection.Dispose();
      }

      public void OnceConnected( Action<GattServerConnection> action )
      {
         if(m_connectionState == ConnectionState.Connected)
         {
            action( this );
         }
         else
         {
            m_onConnectedActions.Add( action );
         }
      }

      private void connection_ConnectionStateChanged( BluetoothDevice bluetoothDevice, ProfileState profileState )
      {
         switch(profileState)
         {
            case ProfileState.Connected:
               m_connectionState = ConnectionState.Connected;
               foreach(var action in m_onConnectedActions)
               {
                  action( this );
               }
               m_onConnectedActions.Clear();
               break;
            case ProfileState.Connecting:
               m_connectionState = ConnectionState.Connecting;
               break;
            case ProfileState.Disconnecting:
            case ProfileState.Disconnected:
               m_connectionState = ConnectionState.Disconnected;
               var func = Disconnected;
               if(func != null)
               {
                  func( this );
               }
               break;
         }
      }

      private void connection_ServicesDiscovered()
      {
         m_discoveredServices.AddAll(
            m_connection.Services.Where(
               nativeService => m_discoveredServices.All( s => s.Id != nativeService.Uuid.ToGuid() ) )
                        .Select( nativeService => new GattService( m_connection, nativeService ) ) );
         OnServicesDiscovered();
      }

      public static GattServerConnection Connect( BluetoothLowEnergyPeripheral device, Context context,
                                                  Action<GattServerConnection> connectionAction = null )
      {
         var result = new GattServerConnection( device );
         result.m_connection.Connect( device.NativeDevice, context );
         if(connectionAction != null)
         {
            result.OnceConnected( connectionAction );
         }
         return result;
      }
   }
}