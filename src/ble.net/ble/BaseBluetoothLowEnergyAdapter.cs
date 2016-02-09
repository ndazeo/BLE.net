using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bluetooth.ble.gatt;
using nexus.core.logging;

namespace bluetooth.ble
{
   public abstract class BaseBluetoothLowEnergyAdapter<TPeripheral, TServerConnection> // : IBluetoothLowEnergyAdapter
      where TServerConnection : class, IGattServerConnection
      where TPeripheral : class, IBluetoothLowEnergyPeripheral
   {
      protected readonly IList<TServerConnection> connectedDevices;
      protected readonly IList<TPeripheral> discoveredDevices;

      protected abstract void ConnectToDeviceInternal( TaskCompletionSource<IGattServerConnection> tcs,
                                                       TPeripheral device );

      protected abstract void StartScanInternal( Guid serviceUuid );

      protected abstract void StopScanInternal();

      protected BaseBluetoothLowEnergyAdapter()
      {
         discoveredDevices = new List<TPeripheral>();
         connectedDevices = new List<TServerConnection>();
      }

      public event Action<IBluetoothLowEnergyPeripheral> AdvertisementDiscovered;

      public event Action ScanStarted;

      public event Action<ScanStopReason> ScanStopped;

      public IEnumerable<IBluetoothLowEnergyPeripheral> DiscoveredDevices
      {
         get { return discoveredDevices; }
      }

      public Boolean IsScanningForAdvertisements { get; private set; }

      public Task<IGattServerConnection> ConnectToDevice( IBluetoothLowEnergyPeripheral device )
      {
         if(device == null)
         {
            throw new ArgumentNullException( nameof( device ) );
         }

         var tcs = new TaskCompletionSource<IGattServerConnection>();
         var castDevice = (TPeripheral)device;
         var connected = connectedDevices.FirstOrDefault( c => c.Id == device.Id );
         if(discoveredDevices.Contains( castDevice ))
         {
            if(connected == null)
            {
               Log.Debug(
                  "BaseBluetoothLowEnergyAdapter.ConnectToDevice. peripheral has been discovered but is not currently connected" );
               StopScanningForAdvertisements();
               ConnectToDeviceInternal( tcs, castDevice );
            }
            else
            {
               Log.Debug( "BaseBluetoothLowEnergyAdapter.ConnectToDevice. peripheral is already connected" );
               tcs.TrySetResult( connected );
            }
         }
         else
         {
            tcs.TrySetException( new Exception( "Peripheral is not among those discovered" ) );
         }
         return tcs.Task;
      }

      public virtual void Dispose()
      {
         Log.Debug( "Disposing IBluetoothLowEnergyAdapter" );
         foreach(var conn in connectedDevices)
         {
            conn.Dispose();
         }
         connectedDevices.Clear();
      }

      public void StartScanningForAdvertisements( TimeSpan timeout )
      {
         StartScanningForAdvertisements( Guid.Empty, timeout );
      }

      public async void StartScanningForAdvertisements( Guid deviceId, TimeSpan timeout )
      {
         if(IsScanningForAdvertisements)
         {
            return;
         }

         Log.Debug( "initiating device scan" );

         IsScanningForAdvertisements = true;
         StartScanInternal( deviceId );
         OnScanStarted();

         await Task.Delay( timeout ).ConfigureAwait( false );
         if(!IsScanningForAdvertisements)
         {
            //Log.Debug( "device scan already stopped when timeout elapsed" );
            return;
         }

         Log.Debug( "device scan timeout elapsed" );
         IsScanningForAdvertisements = false;
         StopScanInternal();
         OnScanStopped( ScanStopReason.Timeout );
      }

      public void StopScanningForAdvertisements()
      {
         if(!IsScanningForAdvertisements)
         {
            return;
         }
         Log.Debug( "stopping device scan" );
         IsScanningForAdvertisements = false;
         StopScanInternal();
         OnScanStopped( ScanStopReason.StopRequested );
      }

      protected void OnAdvertisementDiscovered( IBluetoothLowEnergyPeripheral device )
      {
         var func = AdvertisementDiscovered;
         if(func != null)
         {
            try
            {
               func( device );
            }
            catch(Exception ex)
            {
               Log.Warn( ex );
            }
         }
      }

      protected void TrackDiscoveredDevice( TPeripheral device )
      {
         discoveredDevices.Add( device );
         OnAdvertisementDiscovered( device );
      }

      private void OnScanStarted()
      {
         var func = ScanStarted;
         if(func != null)
         {
            try
            {
               func();
            }
            catch(Exception ex)
            {
               Log.Warn( ex );
            }
         }
      }

      private void OnScanStopped( ScanStopReason reason )
      {
         var func = ScanStopped;
         if(func != null)
         {
            try
            {
               func( reason );
            }
            catch(Exception ex)
            {
               Log.Warn( ex );
            }
         }
      }
   }
}