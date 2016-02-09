using System;
using System.Threading.Tasks;
using CoreBluetooth;
using Foundation;
using nexus.core.logging;
using nexus.core.serialization.binary;

namespace bluetooth.ble.gatt
{
   public class GattCharacteristic : BaseGattCharacteristic<CBCharacteristic>
   {
      private readonly CBPeripheral m_connection;
      private Byte[] m_value;

      public GattCharacteristic( CBPeripheral connection, CBCharacteristic nativeCharacteristic )
         : base( nativeCharacteristic, nativeCharacteristic.UUID.ToGuid() )
      {
         m_connection = connection;
         m_connection.DiscoveredDescriptor += connection_DiscoveredDescriptor;
         m_connection.UpdatedCharacterteristicValue += connection_UpdatedCharacterteristicValue;
         //m_connection.DiscoverDescriptors(nativeCharacteristic);
         Properties = (CharacteristicProperty)(Int32)nativeCharacteristic.Properties;
      }

      public override CharacteristicProperty Properties { get; }

      public override Byte[] Value
      {
         get { return m_value; }
      }

      public override void Dispose()
      {
         m_connection.DiscoveredDescriptor -= connection_DiscoveredDescriptor;
         m_connection.UpdatedCharacterteristicValue -= connection_UpdatedCharacterteristicValue;
      }

      protected override void ReadInternal( TaskCompletionSource<Byte[]> tcs )
      {
         EventHandler<CBCharacteristicEventArgs> onUpdate = null;
         onUpdate = ( sender, e ) =>
         {
            if(e.Characteristic.UUID.ToGuid() == Id)
            {
               Log.Debug( "ReadInternal. onUpdate." );
               m_connection.UpdatedCharacterteristicValue -= onUpdate;
               m_value = nativeCharacteristic.Value.ToArray();
               tcs.SetResult( m_value );
            }
         };
         m_connection.UpdatedCharacterteristicValue += onUpdate;
         Log.Debug( "ReadInternal" );
         m_connection.ReadValue( nativeCharacteristic );
      }

      protected override void RegisterForNotification()
      {
         m_connection.SetNotifyValue( true, nativeCharacteristic );
         Log.Debug( "Notifications now ON for {0}", Id );
      }

      protected override void UnregisterForNotification()
      {
         m_connection.SetNotifyValue( false, nativeCharacteristic );
         Log.Debug( "Notifications now OFF for {0}", Id );
      }

      protected override void WriteInternal( TaskCompletionSource<Object> tcs, Byte[] value )
      {
         var type = (Properties & CharacteristicProperty.WriteNoResponse) != 0
            ? CBCharacteristicWriteType.WithoutResponse
            : CBCharacteristicWriteType.WithResponse;
         if(true || type == CBCharacteristicWriteType.WithResponse)
         {
            EventHandler<CBCharacteristicEventArgs> onWrite = null;
            onWrite = ( sender, e ) =>
            {
               Log.Debug(
                  "GattCharacteristic write callback, id={0} device={1} match={2}",
                  Id,
                  nativeCharacteristic,
                  e.Characteristic.UUID.ToGuid() == Id );
               if(e.Characteristic.UUID.ToGuid() == Id)
               {
                  m_connection.WroteCharacteristicValue -= onWrite;
                  m_connection.UpdatedCharacterteristicValue -= onWrite;
                  if(e.Error == null)
                  {
                     if(nativeCharacteristic != null && nativeCharacteristic.Value != null)
                     {
                        m_value = nativeCharacteristic.Value.ToArray();
                     }
                     else
                     {
                        m_value = new Byte[0];
                     }
                     Log.Debug( "GattCharacteristic setting result for tcs" );
                     tcs.SetResult( m_value );
                  }
                  else
                  {
                     Log.Debug( "GattCharacteristic setting exception for tcs" );
                     tcs.SetException( new Exception( e.Error + "" ) );
                  }
               }
            };
            m_connection.WroteCharacteristicValue += onWrite;
            m_connection.UpdatedCharacterteristicValue += onWrite;
         }
         Log.Debug( "GattCharacteristic write, value={0} write-type={1}", value.EncodeToBase16(), type );
         m_connection.WriteValue(
            NSData.FromArray( value ),
            nativeCharacteristic,
            CBCharacteristicWriteType.WithResponse ); //type);
         if(type == CBCharacteristicWriteType.WithoutResponse)
         {
            //tcs.SetResult( true );
         }
      }

      private void connection_DiscoveredDescriptor( Object sender, CBCharacteristicEventArgs e )
      {
         if(nativeCharacteristic.Descriptors != null)
         {
            Log.Debug( "connection_DiscoveredDescriptor" );
            foreach(var desc in nativeCharacteristic.Descriptors)
            {
               descriptors.Add( new GattDescriptor( m_connection, desc ) );
            }
         }
      }

      private void connection_UpdatedCharacterteristicValue( Object sender, CBCharacteristicEventArgs e )
      {
         //Log.Debug( "connection_UpdatedCharacterteristicValue. NOTIFYING old_val={0} new_val={1}", m_value, e.Characteristic.Value.ToArray() );
         m_value = e.Characteristic.Value.ToArray();
         OnNotifyChanged();
      }
   }
}