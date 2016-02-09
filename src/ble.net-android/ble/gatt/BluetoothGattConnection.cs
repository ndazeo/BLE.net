using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;
using nexus.core;
using nexus.core.logging;

namespace bluetooth.ble.gatt
{
   /// <summary>
   /// A wrapper around <see cref="BluetoothGatt" /> (which is sealed and doesn't implement an interface), and
   /// <see cref="BluetoothGattCallback" /> which is another of Android's idiotic API decisions. This is the anti-corruption
   /// layer to interface with the Android API. Our bluetooth code begins <see cref="GattServerConnection" /> which does not
   /// expose read/write methods and doesn't have all these public callbacks and methods.
   /// </summary>
   internal sealed class BluetoothGattConnection
      : BluetoothGattCallback,
        IBluetoothGattConnection
   {
      private readonly Queue<Action> m_actionQueue;
      private readonly Dictionary<Guid, TaskCompletionSource<GattStatus>> m_pendingTasks;
      private Boolean m_actionInProgress;
      private BluetoothGatt m_gatt;

      public BluetoothGattConnection()
      {
         m_actionQueue = new Queue<Action>();
         m_pendingTasks = new Dictionary<Guid, TaskCompletionSource<GattStatus>>();
      }

      public event Action<BluetoothDevice, ProfileState> ConnectionStateChanged;

      public event Action<BluetoothGattCharacteristic> NotificationValueChanged;

      public event Action ServicesDiscovered;

      public IEnumerable<BluetoothGattService> Services
      {
         get { return m_gatt.Services; }
      }

      public void Connect( BluetoothDevice device, Context context )
      {
         m_gatt = device.ConnectGatt( context, false, this );
      }

      public Task DisableNotifications( GattCharacteristic characteristic )
      {
         return ToggleNotifications( characteristic, false );
      }

      public void Disconnect()
      {
         m_gatt.Disconnect();
      }

      public Boolean DiscoverServices()
      {
         return m_gatt.DiscoverServices();
      }

      public new void Dispose()
      {
         Disconnect();
         m_gatt.Dispose();
      }

      public Task EnableNotifications( GattCharacteristic characteristic )
      {
         return ToggleNotifications( characteristic, true );
      }

      public override void OnCharacteristicChanged( BluetoothGatt gatt, BluetoothGattCharacteristic characteristic )
      {
         Log.Debug( "OnCharacteristicChanged {0}", characteristic.Uuid );
         var func = NotificationValueChanged;
         if(func != null)
         {
            func( characteristic );
         }
      }

      public override void OnCharacteristicRead( BluetoothGatt gatt, BluetoothGattCharacteristic characteristic,
                                                 GattStatus status )
      {
         ActionComplete( characteristic.Uuid.ToGuid(), status, "reading from characteristic" );
      }

      public override void OnCharacteristicWrite( BluetoothGatt gatt, BluetoothGattCharacteristic characteristic,
                                                  GattStatus status )
      {
         ActionComplete( characteristic.Uuid.ToGuid(), status, "writing to characteristic" );
      }

      public override void OnConnectionStateChange( BluetoothGatt gatt, GattStatus status, ProfileState newState )
      {
         var disconnectFunc = ConnectionStateChanged;
         if(disconnectFunc != null)
         {
            disconnectFunc( gatt.Device, newState );
         }
      }

      public override void OnDescriptorRead( BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status )
      {
         ActionComplete( descriptor.Uuid.ToGuid(), status, "reading from descriptor" );
      }

      public override void OnDescriptorWrite( BluetoothGatt gatt, BluetoothGattDescriptor descriptor, GattStatus status )
      {
         ActionComplete( descriptor.Uuid.ToGuid(), status, "writing to descriptor" );
      }

      public override void OnReadRemoteRssi( BluetoothGatt gatt, Int32 rssi, GattStatus status )
      {
         Log.Debug( "OnReadRemoteRssi {0}", rssi );
      }

      public override void OnReliableWriteCompleted( BluetoothGatt gatt, GattStatus status )
      {
         Log.Debug( "OnReliableWriteCompleted {0}", status );
      }

      public override void OnServicesDiscovered( BluetoothGatt gatt, GattStatus status )
      {
         var func = ServicesDiscovered;
         if(func != null)
         {
            func();
         }
      }

      public Task<GattStatus> ReadCharacteristic( GattCharacteristic characteristic )
      {
         var tcs = new TaskCompletionSource<GattStatus>();
         EnqueueAction(
            () =>
            {
               m_pendingTasks.Add( characteristic.Id, tcs );
               if(!m_gatt.ReadCharacteristic( characteristic.NativeCharacteristic ))
               {
                  m_pendingTasks.Remove( characteristic.Id );
                  tcs.SetException( new Exception( "Error reading from characteristic {0}".F( characteristic.Id ) ) );
               }
            } );
         return tcs.Task;
      }

      public Task<GattStatus> ReadDescriptor( GattDescriptor descriptor )
      {
         var tcs = new TaskCompletionSource<GattStatus>();
         EnqueueAction(
            () =>
            {
               m_pendingTasks.Add( descriptor.Id, tcs );
               if(!m_gatt.ReadDescriptor( descriptor.NativeDescriptor ))
               {
                  m_pendingTasks.Remove( descriptor.Id );
                  tcs.SetException( new Exception( "Error reading from descriptor {0}".F( descriptor.Id ) ) );
               }
            } );
         return tcs.Task;
      }

      public void Reconnect()
      {
         // TODO: If disconnecting, then wait until state changes and then reconnect
         //return State != ProfileState.Disconnected || m_gatt.Connect();
      }

      public Task<GattStatus> WriteCharacteristic( GattCharacteristic characteristic, Byte[] value )
      {
         Log.Debug( "BluetoothGattConnection.  enqueuing action, type=write-characteristic" );
         var tcs = new TaskCompletionSource<GattStatus>();
         EnqueueAction(
            () =>
            {
               Log.Debug( "BluetoothGattConnection.  performing action, type=write-characteristic" );
               m_pendingTasks.Add( characteristic.Id, tcs );
               characteristic.NativeCharacteristic.SetValue( value );
               if(!m_gatt.WriteCharacteristic( characteristic.NativeCharacteristic ))
               {
                  m_pendingTasks.Remove( characteristic.Id );
                  Log.Warn( "failed writing {0} bytes to {1}", value.Length, characteristic.Id );
                  tcs.SetException( new Exception( "Error writing to characteristic {0}".F( characteristic.Id ) ) );
               }
               else
               {
                  Log.Debug( "BluetoothGattConnection. writing {0} bytes to {1}", value.Length, characteristic.Id );
               }
            } );
         return tcs.Task;
      }

      public Task<GattStatus> WriteDescriptor( GattDescriptor descriptor, Byte[] value )
      {
         Log.Debug( "BluetoothGattConnection. enqueuing action, type=write-descriptor" );
         var tcs = new TaskCompletionSource<GattStatus>();
         EnqueueAction(
            () =>
            {
               Log.Debug( "BluetoothGattConnection.  performing action, type=write-descriptor" );
               m_pendingTasks.Add( descriptor.Id, tcs );
               descriptor.NativeDescriptor.SetValue( value );
               if(!m_gatt.WriteDescriptor( descriptor.NativeDescriptor ))
               {
                  m_pendingTasks.Remove( descriptor.Id );
                  tcs.SetException( new Exception( "Error writing to descriptor {0}".F( descriptor.Id ) ) );
               }
               else
               {
                  Log.Debug( "BluetoothGattConnection. writing {0} bytes to {1}", value.Length, descriptor.Id );
               }
            } );
         return tcs.Task;
      }

      private void ActionComplete( Guid guid, GattStatus status, String type )
      {
         Log.Debug( "BluetoothGattConnection. completing queued action, guid={0}", guid );
         var tcs = m_pendingTasks.Get( guid );
         if(tcs != null)
         {
            m_pendingTasks.Remove( guid );
            if(status == GattStatus.Success)
            {
               tcs.SetResult( status );
            }
            else
            {
               tcs.SetException( new Exception( "Error {1} {2} {0}".F( guid, status, type ) ) );
            }
         }

         m_actionInProgress = false;
         if(m_actionQueue.Count > 0)
         {
            m_actionInProgress = true;
            m_actionQueue.Dequeue()();
         }
      }

      private void EnqueueAction( Action action )
      {
         if(m_actionInProgress)
         {
            Log.Debug( "BluetoothGattConnection. action in progress, queuing." );
            m_actionQueue.Enqueue( action );
         }
         else
         {
            Log.Debug( "BluetoothGattConnection. action in progress, queuing." );
            m_actionInProgress = true;
            action();
         }
      }

      private async Task ToggleNotifications( GattCharacteristic characteristic, Boolean enable )
      {
         Log.Debug(
            "BluetoothGattConnection. toggle notifications {1} for {0}...",
            characteristic.Id,
            enable ? "on" : "off" );
         var notifyDescriptor =
            characteristic.Descriptors.FirstOrDefault(
               d => d.Id.Equals( BluetoothLowEnergyUtils.NotifyDescriptorGuid ) );
         if(notifyDescriptor == null)
         {
            Log.Debug( "BluetoothGattConnection. no notify descriptor on this characteristic" );
         }
         else if(m_gatt.SetCharacteristicNotification( characteristic.NativeCharacteristic, enable ))
         {
            try
            {
               await
                  notifyDescriptor.Write(
                     enable
                        ? BluetoothGattDescriptor.EnableNotificationValue.ToArray()
                        : BluetoothGattDescriptor.DisableNotificationValue.ToArray() );
               // SetCharacteristicNotification() doesn't do the lock and busy check that all the other methods do,
               // this glorious oversight by the Android API team lets us have a moment of respite from their other horrible decisions.
               Log.Debug( "BluetoothGattConnection. turning notifications on for {0}", characteristic.Id );
            }
            catch(Exception ex)
            {
               Log.Debug( "BluetoothGattConnection. failed to write notification descriptor. {0}", ex.Message );
            }
         }
         else
         {
            Log.Debug( "BluetoothGattConnection. failed to set notification on characteristic" );
         }
      }
   }
}