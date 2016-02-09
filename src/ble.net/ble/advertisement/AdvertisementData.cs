using System;
using System.Diagnostics.Contracts;
using nexus.core;

namespace bluetooth.ble.advertisement
{
   /// <summary>
   /// AdvData
   /// </summary>
   public class AdvertisementData
   {
      public AdvertisementData( Byte[] value )
      {
         Contract.Requires( value != null );
         Contract.Requires( value.Length < 31 );
         // TODO: This is actually a sequence of length/type/data triples
         Value = value;
      }

      public Byte[] Value { get; private set; }

      [ContractInvariantMethod]
      private void InvariantMethod()
      {
         Contract.Invariant( Value.Length < 31 );
      }

      public static AdvertisementData ExtractFrom( AdvertisingChannelPDU pdu )
      {
         var ad = new AdvertisementAddress( pdu.payload.Slice( 0, 6 ), pdu.TxAdd );
         if(pdu.Type == AdvertisingChannelPDUType.AdvertisementConnectableUndirected && pdu.payload.Length >= 6)
         {
            return new AdvertisementData( pdu.payload.Length > 6 ? pdu.payload.Slice( 7 ) : new Byte[] {} );
         }
         return null;
      }
   }
}