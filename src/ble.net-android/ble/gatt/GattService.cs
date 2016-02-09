using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using nexus.core;

namespace bluetooth.ble.gatt
{
   public class GattService : IGattService
   {
      private readonly IBluetoothGattConnection m_connection;
      private readonly BluetoothGattService m_nativeService;
      private HashSet<GattCharacteristic> m_characteristics;

      public GattService( IBluetoothGattConnection connection, BluetoothGattService nativeService )
      {
         m_nativeService = nativeService;
         m_connection = connection;
         Id = nativeService.Uuid.ToGuid();
      }

      public IEnumerable<IGattCharacteristic> Characteristics
      {
         get
         {
            // if it hasn't been populated yet, populate it
            if(m_characteristics == null)
            {
               m_characteristics = new HashSet<GattCharacteristic>();
               foreach(var item in m_nativeService.Characteristics)
               {
                  m_characteristics.Add( new GattCharacteristic( m_connection, item ) );
               }
            }
            return m_characteristics;
         }
      }

      public Guid Id { get; private set; }

      public Boolean IsPrimary
      {
         get { return m_nativeService.Type == GattServiceType.Primary; }
      }

      public void Dispose()
      {
         foreach(var ch in m_characteristics)
         {
            ch.Dispose();
         }
         m_characteristics.Clear(); // not needed for disposal but nice to not loop twice if dispose is called again
      }

      public override Boolean Equals( Object obj )
      {
         var ch = obj as IGattService;
         return ch != null && ch.Id == Id;
      }

      public IGattCharacteristic GetCharacteristic( Guid guid )
      {
         return Characteristics.FirstOrDefault( c => c.Id.Equals( guid ) );
      }

      public override Int32 GetHashCode()
      {
         return Id.GetHashCode();
      }

      public override String ToString()
      {
         return "{{Gatt Service {0}}}".F( Id );
      }
   }
}