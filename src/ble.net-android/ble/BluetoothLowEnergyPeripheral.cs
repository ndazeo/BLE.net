using System;
using Android.Bluetooth;

namespace bluetooth.ble
{
   public class BluetoothLowEnergyPeripheral
      : BaseBluetoothLowEnergyPeripheral<BluetoothDevice>,
        IBluetoothLowEnergyPeripheral
   {
      public BluetoothLowEnergyPeripheral( BluetoothDevice nativeDevice, Byte[] advertisement, Int32 rssi )
         : base( BluetoothUtils.MacToGuid( nativeDevice.Address ), nativeDevice.Name, nativeDevice, advertisement, rssi
            )
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