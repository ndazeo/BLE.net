using System;
using System.Runtime.InteropServices;
using nexus.core;

namespace bluetooth.ble.advertisement
{
   [StructLayout( LayoutKind.Sequential, Pack = 1 )]
   public struct AdvertisingChannelPDU
   {
      public UInt16 header;
      public Byte[] payload;

      public AdvertisingChannelPDUType Type
      {
         get { return ((AdvertisingChannelPDUType)(header >> 12)); }
      }

      public Byte PayloadLength
      {
         get { return (Byte)((header >> 2) & 63); }
      }

      public Boolean TxAdd
      {
         get { return ((header >> 9) & 1) == 1; }
      }

      public Boolean RxAdd
      {
         get { return ((header >> 8) & 1) == 1; }
      }

      public static AdvertisingChannelPDU ExtractFrom( BluetoothLinkLayerPacket packet )
      {
         if(packet.accessAddress == BleAdvertisingUtils.AdvertisingPacketAccessAddress &&
            packet.preamble == BleAdvertisingUtils.AdvertisingPacketPreamble)
         {
            return new AdvertisingChannelPDU
            {
               header = (UInt16)packet.Pdu.Slice( 0, 2 ).ToInt16(),
               payload = packet.Pdu.Slice( 3 )
            };
         }
         return default(AdvertisingChannelPDU);
      }
   }
}