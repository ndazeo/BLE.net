using System;
using CoreBluetooth;
using Foundation;

namespace bluetooth
{
   public static class iOSUtils
   {
      public static Guid ToGuid( this CBUUID uuid )
      {
         var id = uuid.ToString();
         return id.Length == 4 ? id.CreateGuidFromAdoptedKey() : Guid.ParseExact( id, "d" );
      }

      public static Guid ToGuid( this NSUuid uuid )
      {
         var id = uuid.AsString();
         return id.Length == 4 ? id.CreateGuidFromAdoptedKey() : Guid.ParseExact( id, "d" );
      }
   }
}