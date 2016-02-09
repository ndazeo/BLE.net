using System;
using System.Collections.Generic;
using System.Linq;
using CoreBluetooth;
using nexus.core;
using nexus.core.logging;

namespace bluetooth.ble.gatt
{
   public class GattService
      : IGattService,
        IDisposable
   {
      private readonly HashSet<GattCharacteristic> m_characteristics;
      private readonly CBPeripheral m_connection;
      private readonly CBService m_nativeService;

      public GattService( CBPeripheral connection, CBService nativeService )
      {
         m_nativeService = nativeService;
         Id = m_nativeService.UUID.ToGuid();
         m_connection = connection;
         m_characteristics = new HashSet<GattCharacteristic>();
         m_connection.DiscoveredCharacteristic += connection_DiscoveredCharacteristic;
      }

      public IEnumerable<IGattCharacteristic> Characteristics
      {
         get { return m_characteristics; }
      }

      public Guid Id { get; private set; }

      public Boolean IsPrimary
      {
         get { return m_nativeService.Primary; }
      }

      public void Dispose()
      {
         foreach(var ch in m_characteristics)
         {
            ch.Dispose();
         }
         m_characteristics.Clear(); // not needed for disposal but nice to not loop twice if dispose is called again
         m_connection.DiscoveredCharacteristic -= connection_DiscoveredCharacteristic;
      }

      public override Boolean Equals( Object obj )
      {
         var ch = obj as IGattService;
         return ch != null && ch.Id == Id;
      }

      public IGattCharacteristic GetCharacteristic( Guid guid )
      {
         return Characteristics.FirstOrDefault( item => item.Id.Equals( guid ) );
      }

      public override Int32 GetHashCode()
      {
         return Id.GetHashCode();
      }

      public override String ToString()
      {
         return "{{Gatt Service {0}}}".F( Id );
      }

      private void connection_DiscoveredCharacteristic( Object sender, CBServiceEventArgs e )
      {
         var chars = m_nativeService.Characteristics;
         if(e.Service.UUID.ToGuid() == Id && chars != null && chars.Length > 0)
         {
            Log.Debug( "GattService discovered characteristic={0}", Id );
            m_characteristics.AddAll(
               chars.Where( cbCharacteristic => m_characteristics.All( ch => ch.Id != cbCharacteristic.UUID.ToGuid() ) )
                    .Select( cbCharacteristic => new GattCharacteristic( m_connection, cbCharacteristic ) ) );
         }
      }
   }
}