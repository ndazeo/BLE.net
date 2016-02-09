using System;
using System.Collections.Generic;

namespace bluetooth.ble.gatt
{
   public interface IGattService
      : IGattAttribute,
        IDisposable
   {
      IEnumerable<IGattCharacteristic> Characteristics { get; }

      Boolean IsPrimary { get; }

      IGattCharacteristic GetCharacteristic( Guid guid );
   }
}