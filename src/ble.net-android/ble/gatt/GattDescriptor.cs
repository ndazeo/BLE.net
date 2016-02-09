using System;
using System.Threading.Tasks;
using Android.Bluetooth;

namespace bluetooth.ble.gatt
{
   public class GattDescriptor
      : BaseGattDescriptor<BluetoothGattDescriptor>,
        IGattDescriptor
   {
      private readonly IBluetoothGattConnection m_connection;

      public GattDescriptor( IBluetoothGattConnection connection, BluetoothGattDescriptor nativeDescriptor )
         : base( nativeDescriptor, nativeDescriptor.Uuid.ToGuid() )
      {
         m_connection = connection;
         Value = nativeDescriptor.GetValue();
      }

      public BluetoothGattDescriptor NativeDescriptor
      {
         get { return nativeDescriptor; }
      }

      public Byte[] Value { get; private set; }

      public Task<Byte[]> Read()
      {
         return m_connection.ReadDescriptor( this ).ContinueWith(
            task =>
            {
               Value = nativeDescriptor.GetValue();
               return Value;
            } );
      }

      public async Task Write( Byte[] value )
      {
         await m_connection.WriteDescriptor( this, value );
         Value = value;
      }
   }
}