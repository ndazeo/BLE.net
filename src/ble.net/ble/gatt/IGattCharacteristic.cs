using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bluetooth.ble.gatt
{
   public interface IGattCharacteristic
      : IGattAttribute,
        IDisposable
   {
      event Action<IGattCharacteristic> NotifyChange;

      IEnumerable<IGattDescriptor> Descriptors { get; }

      CharacteristicProperty Properties { get; }

      Byte[] Value { get; }

      IGattDescriptor GetDescriptor( Guid guid );

      Task<Byte[]> Read();

      Task Write( Byte[] data );
   }
}