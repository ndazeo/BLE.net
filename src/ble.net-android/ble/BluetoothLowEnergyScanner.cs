using System;
using System.Collections.Generic;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Object = Java.Lang.Object;

namespace bluetooth.ble
{
   /// <summary>
   /// A wrapper around the BLE interface to <see cref="BluetoothAdapter" /> and the idiotic
   /// <see cref="BluetoothAdapter.ILeScanCallback" /> callback interface.
   /// </summary>
   public sealed class BluetoothLowEnergyScanner
      : Object,
        BluetoothAdapter.ILeScanCallback
   {
      private readonly BluetoothAdapter m_adapter;

      public BluetoothLowEnergyScanner( BluetoothAdapter adapter )
      {
         m_adapter = adapter;
      }

      public event Action<BluetoothDevice, Int32, Byte[]> AdvertisementDiscovered = delegate { };

      public void OnLeScan( BluetoothDevice device, Int32 rssi, Byte[] scanRecord )
      {
         AdvertisementDiscovered( device, rssi, scanRecord );
      }

      public void StartScan()
      {
         m_adapter.StartLeScan( this );
      }

      public void StopScan()
      {
         m_adapter.StopLeScan( this );
      }

      protected override void Dispose( Boolean disposing )
      {
         base.Dispose( disposing );
         m_adapter.Dispose();
      }
   }

   internal sealed class BluetoothLowEnergyScannerApi21 : ScanCallback
   {
      private readonly BluetoothLeScanner m_scanner;

      public BluetoothLowEnergyScannerApi21( BluetoothLeScanner scanner )
      {
         m_scanner = scanner;
      }

      public override void OnBatchScanResults( IList<ScanResult> results )
      {
         base.OnBatchScanResults( results );
      }

      public override void OnScanFailed( ScanFailure errorCode )
      {
         base.OnScanFailed( errorCode );
      }

      public override void OnScanResult( ScanCallbackType callbackType, ScanResult result )
      {
         base.OnScanResult( callbackType, result );
      }

      public void StartScan()
      {
         m_scanner.StartScan( this );
      }

      public void StopScan()
      {
         m_scanner.StopScan( this );
      }
   }
}