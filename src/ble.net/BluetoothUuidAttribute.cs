using System;
using System.Reflection;
using nexus.core;

namespace bluetooth
{
   [AttributeUsage( AttributeTargets.Class )]
   public class BluetoothUuidAttribute
      : Attribute,
        IBluetoothIdentifier
   {
      public BluetoothUuidAttribute( Int16 adoptedKey, String name )
      {
         Id = adoptedKey.CreateGuidFromAdoptedKey();
         Name = name;
      }

      public BluetoothUuidAttribute( Guid uuid, String name = null )
      {
         Id = uuid;
         Name = name.IsNullOrEmpty() ? "Unknown" : name;
      }

      public BluetoothUuidAttribute( String uuid, String name = null )
         : this( Parse<Guid>.OrDefault( uuid ), name )
      {
      }

      public Guid Id { get; }

      public String Name { get; }

      public static BluetoothUuidAttribute GetBluetoothUuid<T>()
      {
         return typeof(T).GetTypeInfo().GetCustomAttribute<BluetoothUuidAttribute>();
      }

      public static implicit operator Guid( BluetoothUuidAttribute value )
      {
         return value != null ? value.Id : Guid.Empty;
      }
   }
}