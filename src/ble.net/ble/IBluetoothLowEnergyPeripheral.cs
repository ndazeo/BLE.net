using System;

namespace bluetooth.ble
{
   /// <summary>
   /// A structure to contain the advertising and other information broadcast by devices and discovered when
   /// <see cref="IBluetoothLowEnergyAdapter" /> is scanning.
   /// </summary>
   public interface IBluetoothLowEnergyPeripheral : IBluetoothIdentifier
   {
      Byte[] Advertisement { get; }

      /// <summary>
      /// The Id is just <see cref="Address"/> in Guid form
      /// </summary>
      new Guid Id { get; }

      String Name { get; }

      /// <summary>
      /// MAC address of this device
      /// </summary>
      //Byte[] Address { get; }

      /// <summary>
      /// Received Signal Strenth Indicator in decibels
      /// </summary>
      Int32 Rssi { get; }
   }
}