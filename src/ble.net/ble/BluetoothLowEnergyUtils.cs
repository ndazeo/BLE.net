using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using bluetooth.ble.gatt;
using nexus.core;
using nexus.core.logging;

namespace bluetooth.ble
{
   public static class BluetoothLowEnergyUtils
   {
      public static readonly Guid NotifyDescriptorGuid = BluetoothUtils.CreateGuidFromAdoptedKey( 0x2902 );

      public static Boolean CanIndicate( this IGattCharacteristic characteristic )
      {
         return (characteristic.Properties & CharacteristicProperty.Indicate) != 0;
      }

      public static Boolean CanNotify( this IGattCharacteristic characteristic )
      {
         return (characteristic.Properties & CharacteristicProperty.Notify) != 0;
      }

      public static Boolean CanRead( this IGattCharacteristic characteristic )
      {
         return (characteristic.Properties & CharacteristicProperty.Read) != 0;
      }

      public static Boolean CanWrite( this IGattCharacteristic characteristic )
      {
         return (characteristic.Properties & (CharacteristicProperty.Write | CharacteristicProperty.WriteNoResponse)) !=
                0;
      }

      public static Int32 GetAdoptedKey( this Guid guid )
      {
         // TODO: need to see if this actually works, think I have to get the high bits
         return guid.ToByteArray().ToInt32();
      }

      /// <summary>
      /// This method is used by <see cref="BaseGattServerConnection{TNativeDevice}" /> and exposed via
      /// <see cref="IGattServerConnection" /> so there is no need to keep it public
      /// </summary>
      public static async Task<IGattService> GetService( this IGattServerConnection server, Guid serviceId,
                                                         CancellationToken cancel )
      {
         var service = server.DiscoveredServices.FirstOrDefault( x => x.Id == serviceId );
         if(service != null)
         {
            Log.Debug( "BluetoothLowEnergyUtils.GetService {0}. service has already been discovered.", serviceId );
            return await Task.FromResult( service ).ConfigureAwait( false );
         }

         var tcs = new TaskCompletionSource<IGattService>();
         CancellationTokenRegistration cts;
         Action<IGattServerConnection> serverOnServicesDiscovered = connection =>
         {
            var s = connection.DiscoveredServices.FirstOrDefault( x => x.Id == serviceId );
            if(s != null)
            {
               Log.Debug( "BluetoothLowEnergyUtils.GetService {0}. found service, setting result.", serviceId );
               tcs.SetResult( s );
            }
            else
            {
               Log.Debug( "BluetoothLowEnergyUtils.GetService {0}. service could still not be found.", serviceId );
            }
         };

         try
         {
            Log.Debug( "BluetoothLowEnergyUtils.GetService {0}, setup", serviceId );
            cts = cancel.Register( () => tcs.TrySetResult( null ), false );
            server.ServicesDiscovered += serverOnServicesDiscovered;
            server.DiscoverAllServices();
            //Log.Debug( "BluetoothLowEnergyUtils.GetService {0}, await return task...", serviceId );
            return await tcs.Task.ConfigureAwait( false );
         }
         finally
         {
            cts.Dispose();
            server.ServicesDiscovered -= serverOnServicesDiscovered;
            Log.Debug( "BluetoothLowEnergyUtils.GetService {0}, finally, disposed.", serviceId );
         }
      }

      public static Task<IBluetoothLowEnergyPeripheral> ScanForDevice( this IBluetoothLowEnergyAdapter ble,
                                                                       Func<IBluetoothLowEnergyPeripheral, Boolean>
                                                                          isCorrect, TimeSpan timeout )
      {
         return ScanForDevice( ble, isCorrect, timeout, default(CancellationToken) );
      }

      public static async Task<IBluetoothLowEnergyPeripheral> ScanForDevice( this IBluetoothLowEnergyAdapter ble,
                                                                             Func
                                                                                <IBluetoothLowEnergyPeripheral, Boolean>
                                                                                isCorrect, TimeSpan timeout,
                                                                             CancellationToken cancel )
      {
         var tcs = new TaskCompletionSource<IBluetoothLowEnergyPeripheral>();
         CancellationTokenRegistration cancellationTokenRegistration;
         var scanAlreadyUnderway = ble.IsScanningForAdvertisements;

         Action<IBluetoothLowEnergyPeripheral> discoveredDevice = peripheral =>
         {
            if(isCorrect( peripheral ))
            {
               Log.Debug( "ScanForDevice. discovered correct device, name={0} id={1}", peripheral.Name, peripheral.Id );
               tcs.TrySetResult( peripheral );
            }
         };

         Action<ScanStopReason> scanStopped = reason =>
         {
            Log.Debug( "ScanForDevice. scanStopped." );
            if(scanAlreadyUnderway)
            {
               scanAlreadyUnderway = false;
               // TODO: Figure out why scanning by service UUID isn't working
               ble.StartScanningForAdvertisements( /*service,*/ timeout );
            }
            else
            {
               tcs.TrySetCanceled();
            }
         };

         try
         {
            cancellationTokenRegistration =
               CancellationTokenSource.CreateLinkedTokenSource( cancel, new CancellationTokenSource( timeout ).Token )
                                      .Token.Register(
                                         () =>
                                         {
                                            Log.Debug( "ScanForDevice. timeout, cancelling task" );
                                            tcs.TrySetCanceled();
                                         },
                                         false );
            ble.ScanStopped += scanStopped;
            ble.AdvertisementDiscovered += discoveredDevice;

            if(!scanAlreadyUnderway)
            {
               Log.Debug( "ScanForDevice. starting scan." );
               // TODO: Figure out why scanning by service UUID isn't working
               ble.StartScanningForAdvertisements( /*service,*/ timeout );
            }
            return await tcs.Task.ConfigureAwait( false );
         }
         finally
         {
            ble.AdvertisementDiscovered -= discoveredDevice;
            ble.ScanStopped -= scanStopped;
            ble.StopScanningForAdvertisements();
            cancellationTokenRegistration.Dispose();
            Log.Debug( "ScanForDevice. finally, disposed." );
         }
      }

      public static Task<IBluetoothLowEnergyPeripheral> ScanForDeviceWithService( this IBluetoothLowEnergyAdapter ble,
                                                                                  Guid service, TimeSpan timeout )
      {
         return ScanForDevice( ble, peripheral => peripheral.Id == service, timeout );
      }
   }
}