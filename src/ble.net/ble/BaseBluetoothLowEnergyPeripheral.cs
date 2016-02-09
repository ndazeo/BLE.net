using System;
using System.Diagnostics.Contracts;
using nexus.core;

namespace bluetooth.ble
{
   public abstract class BaseBluetoothLowEnergyPeripheral<TNativeDevice> // : IBluetoothLowEnergyPeripheral
      where TNativeDevice : class
   {
      protected BaseBluetoothLowEnergyPeripheral( Guid id, String name, TNativeDevice nativeDevice, Byte[] advertisement,
                                                  Int32 rssi )
      {
         Contract.Requires( nativeDevice != null );
         NativeDevice = nativeDevice;
         Id = id;
         Name = name;
         Advertisement = advertisement;
         Rssi = rssi;
      }

      public Byte[] Advertisement { get; protected set; }

      public Guid Id { get; private set; }

      public String Name { get; private set; }

      public TNativeDevice NativeDevice { get; private set; }

      public Int32 Rssi { get; protected set; }

      public void SetNativeDevice( TNativeDevice nativeDevice )
      {
         Contract.Ensures( NativeDevice != null );
         if(nativeDevice != null && !nativeDevice.Equals( NativeDevice ))
         {
            NativeDevice = nativeDevice;
         }
      }

      public override String ToString()
      {
         return Name.IsNullOrWhiteSpace() ? Id.ToString() : "{0} {1}".F( Name, Id );
      }
   }
}