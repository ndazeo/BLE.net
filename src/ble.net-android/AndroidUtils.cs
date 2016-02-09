using System;
using Java.Util;

namespace bluetooth
{
   public static class AndroidUtils
   {
      public static Guid ToGuid( this UUID uuid )
      {
         var id = uuid.ToString();
         return id.Length == 4 ? id.CreateGuidFromAdoptedKey() : Guid.ParseExact( id, "d" );
      }
   }
}