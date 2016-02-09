using System;
using CoreBluetooth;

namespace bluetooth.ble
{
   public class BluetoothLowEnergyPeripheral
      : BaseBluetoothLowEnergyPeripheral<CBPeripheral>,
        IBluetoothLowEnergyPeripheral
   {
      public BluetoothLowEnergyPeripheral( CBPeripheral peripheral, Byte[] advertisement, Int32 rssi )
         : base( peripheral.Identifier.ToGuid(), peripheral.Name, peripheral, advertisement, rssi )
      {
      }

      internal void SetAdvertisement( Byte[] advertisement )
      {
         Advertisement = advertisement;
      }

      internal void SetRssi( Int32 rssi )
      {
         Rssi = rssi;
      }
   }
}