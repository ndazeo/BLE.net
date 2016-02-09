using System;
using System.Runtime.InteropServices;
using nexus.core;

namespace bluetooth
{
   [StructLayout( LayoutKind.Sequential, Pack = 1 )]
   public struct BluetoothLinkLayerPacket
   {
      public Byte preamble;
      public UInt32 accessAddress;
      public Byte[] pduAndCrc;

      public Byte[] Pdu
      {
         get
         {
            if(pduAndCrc != null && pduAndCrc.Length > 3)
            {
               return pduAndCrc.Slice( 0, pduAndCrc.Length - 3 );
            }
            return new Byte[0];
         }
      }

      public Byte[] Crc
      {
         get
         {
            if(pduAndCrc != null && pduAndCrc.Length > 3)
            {
               return pduAndCrc.Slice( pduAndCrc.Length - 4 );
            }
            return new Byte[3];
         }
      }
   }
}