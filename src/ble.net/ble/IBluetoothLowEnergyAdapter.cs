using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using bluetooth.ble.gatt;

namespace bluetooth.ble
{
   /// <summary>
   /// This is the entry point for all Bluetooth Low Energy functionality. Use a <see cref="IBluetoothLowEnergyAdapter" /> to
   /// scan for BLE advertisements and connect to found devices.
   /// </summary>
   public interface IBluetoothLowEnergyAdapter : IDisposable
   {
      event Action<IBluetoothLowEnergyPeripheral> AdvertisementDiscovered;

      event Action ScanStarted;

      // TODO: Need more detail than a byte array here; everything else is typed, if we use a byte array it should be at a lower level
      //Task<Byte[]> SendScanRequest( IBluetoothLowEnergyPeripheral peripheral );

      event Action<ScanStopReason> ScanStopped;

      IEnumerable<IBluetoothLowEnergyPeripheral> DiscoveredDevices { get; }

      Boolean IsScanningForAdvertisements { get; }

      /// <summary>
      /// Once you have discovered a device's advertisement by listening to <see cref="AdvertisementDiscovered" />, call this to
      /// connect to that device's GATT server.
      /// </summary>
      /// <param name="device"></param>
      /// <returns></returns>
      Task<IGattServerConnection> ConnectToDevice( IBluetoothLowEnergyPeripheral device );

      void StartScanningForAdvertisements( TimeSpan timeout );

      void StartScanningForAdvertisements( Guid deviceId, TimeSpan timeout );

      void StopScanningForAdvertisements();
   }
}