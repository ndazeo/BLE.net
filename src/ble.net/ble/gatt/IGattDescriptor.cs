using System;
using System.Threading.Tasks;

namespace bluetooth.ble.gatt
{
   public interface IGattDescriptor : IGattAttribute
   {
      Byte[] Value { get; }

      Task<Byte[]> Read();

      Task Write( Byte[] data );
   }
}