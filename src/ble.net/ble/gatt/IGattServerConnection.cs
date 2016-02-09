using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace bluetooth.ble.gatt
{
   /// <summary>
   /// An active connection to a <see cref="IBluetoothLowEnergyPeripheral" />. The GATT Server connection is established when
   /// a Central device successfully establishes a client/server connection to a Peripheral device.
   /// </summary>
   public interface IGattServerConnection
      : IBluetoothLowEnergyPeripheral,
        IDisposable
   {
      event Action<IGattServerConnection> ServicesDiscovered;

      IEnumerable<IGattService> DiscoveredServices { get; }

      ConnectionState State { get; }

      /// <summary>
      /// If you don't know what services are available on a device, you can scan for all of them and then iterate through
      /// <see cref="DiscoveredServices" />.
      /// </summary>
      void DiscoverAllServices();

      /// <summary>
      /// Use when you are looking to retrieve a particular service and don't need to perform discovery.
      /// </summary>
      /// <param name="serviceId"></param>
      /// <returns></returns>
      Task<IGattService> GetService( Guid serviceId );
   }
}