using System;
using nexus.core;

namespace bluetooth.ble.gatt
{
   public abstract class BaseGattDescriptor<TNative> // : IGattDescriptor
   {
      protected readonly TNative nativeDescriptor;

      protected BaseGattDescriptor( TNative nativeDescriptor, Guid id )
      {
         this.nativeDescriptor = nativeDescriptor;
         Id = id;
      }

      public Guid Id { get; private set; }

      public override Boolean Equals( Object obj )
      {
         var ch = obj as IGattDescriptor;
         return ch != null && ch.Id == Id;
      }

      public override Int32 GetHashCode()
      {
         return Id.GetHashCode();
      }

      public override String ToString()
      {
         return "{{Gatt Descriptor {0}}}".F( Id );
      }
   }
}