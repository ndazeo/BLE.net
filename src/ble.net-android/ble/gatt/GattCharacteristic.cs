using System;
using System.Threading.Tasks;
using Android.Bluetooth;
using nexus.core;
using nexus.core.logging;

namespace bluetooth.ble.gatt
{
   public class GattCharacteristic
      : BaseGattCharacteristic<BluetoothGattCharacteristic>,
        IGattCharacteristic
   {
      private readonly IBluetoothGattConnection m_connection;
      private Byte[] m_value;

      public GattCharacteristic( IBluetoothGattConnection connection, BluetoothGattCharacteristic nativeObject )
         : base( nativeObject, nativeObject.Uuid.ToGuid() )
      {
         m_connection = connection;
         m_connection.NotificationValueChanged += connection_NotificationValueChanged;
         m_value = nativeCharacteristic.GetValue();
         foreach(var item in nativeCharacteristic.Descriptors)
         {
            descriptors.Add( new GattDescriptor( connection, item ) );
         }
      }

      public override CharacteristicProperty Properties
      {
         get { return (CharacteristicProperty)(Int32)nativeCharacteristic.Properties; }
      }

      public override Byte[] Value
      {
         get { return m_value; }
      }

      internal BluetoothGattCharacteristic NativeCharacteristic
      {
         get { return nativeCharacteristic; }
      }

      public override void Dispose()
      {
         m_connection.NotificationValueChanged -= connection_NotificationValueChanged;
      }

      protected override void ReadInternal( TaskCompletionSource<Byte[]> tcs )
      {
         m_connection.ReadCharacteristic( this ).ContinueWith(
            task =>
            {
               if(task.Result == GattStatus.Success)
               {
                  m_value = nativeCharacteristic.GetValue();
                  tcs.SetResult( m_value );
               }
               else
               {
                  tcs.SetException( new Exception( "Error {0} reading from characteristic {1}".F( task.Result, Id ) ) );
               }
            } );
      }

      protected override async void RegisterForNotification()
      {
         await m_connection.EnableNotifications( this );
      }

      protected override async void UnregisterForNotification()
      {
         await m_connection.DisableNotifications( this );
      }

      protected override void WriteInternal( TaskCompletionSource<Object> tcs, Byte[] value )
      {
         m_connection.WriteCharacteristic( this, value ).ContinueWith(
            task =>
            {
               Log.Debug( "GattCharacteristic. write completed for {0} {1}", Id, task.Result );
               if(task.Result == GattStatus.Success)
               {
                  m_value = value;
                  tcs.SetResult( m_value );
               }
               else
               {
                  tcs.SetException( new Exception( "Error {0} writing to characteristic {1}".F( task.Result, Id ) ) );
               }
            } );
      }

      private void connection_NotificationValueChanged( BluetoothGattCharacteristic characteristic )
      {
         if(characteristic.Uuid.ToGuid().Equals( Id ))
         {
            Log.Debug(
               "connection_NotificationValueChanged. NOTIFYING old_val={0} new_val={1}",
               m_value,
               characteristic.GetValue() );
            m_value = characteristic.GetValue();
            OnNotifyChanged();
         }
      }
   }
}