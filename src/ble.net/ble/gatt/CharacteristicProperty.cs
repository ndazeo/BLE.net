using System;

namespace bluetooth.ble.gatt
{
   [Flags]
   public enum CharacteristicProperty
   {
      Broadcast = 1,
      Read = 2,
      WriteNoResponse = 4,
      Write = 8,
      Notify = 16,
      Indicate = 32,
      SignedWrite = 64,
      ExtendedProperties = 128
   }

   [Flags]
   public enum CharacteristicExtendedProperty
   {
      NotifyEncryptionRequired = 256,
      IndicateEncryptionRequired = 512
   }
}