using System;
using System.Linq;

namespace bluetooth
{
   public static class BluetoothUtils
   {
      public static Guid CreateGuidFromAdoptedKey( this Int16 adoptedKey )
      {
         //                0000 xxxx -  0000 -  1000 -  80    00 -  00    80    5f    9b    34    fb
         return new Guid( adoptedKey, 0x0000, 0x1000, 0x80, 0x00, 0x00, 0x80, 0x5f, 0x9b, 0x34, 0xfb );
      }

      public static Guid CreateGuidFromAdoptedKey( this String adoptedKey )
      {
         //                       0000       xxxx       -0000-1000-8000-00805f9b34fb
         return Guid.ParseExact( "0000" + adoptedKey + "-0000-1000-8000-00805f9b34fb", "d" );
      }

      public static Guid MacToGuid( String address )
      {
         // TODO: Need to verify that this works
         var deviceGuid = new Byte[16];
         var macWithoutColons = address.Replace( ":", "" );
         var macBytes =
            Enumerable.Range( 0, macWithoutColons.Length )
                      .Where( x => x % 2 == 0 )
                      .Select( x => Convert.ToByte( macWithoutColons.Substring( x, 2 ), 16 ) )
                      .ToArray();
         macBytes.CopyTo( deviceGuid, 10 );
         return new Guid( deviceGuid );
      }
   }
}