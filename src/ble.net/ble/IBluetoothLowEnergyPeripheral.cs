using System;

namespace bluetooth.ble
{
   /// <summary>
   /// A structure to contain the advertising and other information broadcast by devices and discovered when
   /// <see cref="IBluetoothLowEnergyAdapter" /> is scanning.
   /// </summary>
   public interface IBluetoothLowEnergyPeripheral : IBluetoothIdentifier
      // TODO: is the identifier really a GUID? it should be the MAC address of the device, no?
   {
      Byte[] Advertisement { get; }

      String Name { get; }

      /// <summary>
      /// Received Signal Strenth Indicator in decibels
      /// </summary>
      Int32 Rssi { get; }
   }
}