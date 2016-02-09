using System;

namespace bluetooth.ble.advertisement
{
   internal static class BleAdvertisingUtils
   {
      public const UInt32 AdvertisingPacketAccessAddress = 0x8E89BED6;
      public const Byte AdvertisingPacketPreamble = 0xAA;

      public static AdvertisingChannelPDU ExtractAdvertisingChannelPDU( this BluetoothLinkLayerPacket packet )
      {
         return AdvertisingChannelPDU.ExtractFrom( packet );
      }

      public static AdvertisementData ExtractConnectableUndirectedAdvertisementPayload( this AdvertisingChannelPDU pdu )
      {
         return AdvertisementData.ExtractFrom( pdu );
      }
   }
}