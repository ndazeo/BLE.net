# Bluetooth Low Energy PCL [![Build status](https://img.shields.io/appveyor/ci/nexussays/ble-net.svg?style=flat-square)](https://ci.appveyor.com/project/nexussays/ble-net) [![NuGet](https://img.shields.io/nuget/v/ble.net.svg?style=flat-square)](https://www.nuget.org/packages/ble.net) [![Apache 2.0 License](https://img.shields.io/badge/license-Apache%202.0-blue.svg?style=flat-square)](http://www.apache.org/licenses/LICENSE-2.0)

Cross-platform (Android & iOS) Bluetooth Low Energy library for .NET/Xamarin

### Getting Started

There are packages available for Android, and iOS (as well as the shared PCL if you'd like to implement another platform)

```powershell
Install-Package ble.net-android
```

```powershell
Install-Package ble.net-ios
```

You'll need to create an instance of `IBluetoothLowEnergyAdapter` by instantiating `new BluetoothLowEnergyAdapter()` for the respective platform at initialization. Then use that `IBluetoothLowEnergyAdapter` throughout your code as needed to scan for and connect to peripherals, etc.
