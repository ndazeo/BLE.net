using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Content;

namespace bluetooth.ble.gatt
{
   /// <summary>
   /// Interface for a wrapper around <see cref="BluetoothGatt" /> (which is sealed and doesn't implement an interface), and
   /// <see cref="BluetoothGattCallback" />.
   /// </summary>
   public interface IBluetoothGattConnection : IDisposable
   {
      event Action<BluetoothDevice, ProfileState> ConnectionStateChanged;

      event Action<BluetoothGattCharacteristic> NotificationValueChanged;

      event Action ServicesDiscovered;

      //Boolean IsConnected { get; }

      IEnumerable<BluetoothGattService> Services { get; }

      //ProfileState State { get; }

      void Connect( BluetoothDevice device, Context context );

      Task DisableNotifications( GattCharacteristic gattCharacteristic );

      void Disconnect();

      Boolean DiscoverServices();

      Task EnableNotifications( GattCharacteristic characteristic );

      Task<GattStatus> ReadCharacteristic( GattCharacteristic characteristic );

      Task<GattStatus> ReadDescriptor( GattDescriptor descriptor );

      void Reconnect();

      Task<GattStatus> WriteCharacteristic( GattCharacteristic characteristic, Byte[] value );

      Task<GattStatus> WriteDescriptor( GattDescriptor descriptor, Byte[] value );
   }
}