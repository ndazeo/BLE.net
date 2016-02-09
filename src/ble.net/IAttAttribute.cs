using System;

namespace bluetooth
{
   public interface IAttAttribute
   {
      Guid Id { get; }

      Byte[] Value { get; }
   }
}