using System;
using System.Text;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;

namespace bluetooth.ble.gatt
{
   public class GattDescriptor
      : BaseGattDescriptor<CBDescriptor>,
        IGattDescriptor
   {
      private readonly CBPeripheral m_connection;

      public GattDescriptor( CBPeripheral connection, CBDescriptor nativeDescriptor )
         : base( nativeDescriptor, nativeDescriptor.UUID.ToGuid() )
      {
         m_connection = connection;
         // TODO: Figure out what type Value actually is
         //Value = nativeDescriptor.Value.ToString().GetUtf8Bytes();
      }

      public Byte[] Value { get; private set; }

      public Task<Byte[]> Read()
      {
         m_connection.ReadValue( nativeDescriptor );
         Value = Encoding.UTF8.GetBytes( nativeDescriptor.Value.ToString() );
         return Task.FromResult( Value );
      }

      public Task Write( Byte[] value )
      {
         m_connection.WriteValue( NSData.FromArray( value ), nativeDescriptor );
         Value = value;
         return Task.FromResult( 0 );
      }
   }
}