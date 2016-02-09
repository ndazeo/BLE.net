using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nexus.core;
using nexus.core.logging;

namespace bluetooth.ble.gatt
{
   /// <summary>
   /// Abstract class with shared code for internal usage only. Saves on redundant code that is shared between platform BLE
   /// implementations
   /// </summary>
   /// <typeparam name="TNative">The concrete type for a BLE characteristic in the native library.</typeparam>
   public abstract class BaseGattCharacteristic<TNative> : IGattCharacteristic
   {
      protected readonly HashSet<IGattDescriptor> descriptors;
      protected readonly TNative nativeCharacteristic;

      private Boolean m_notificationsActive;
      private Action<IGattCharacteristic> m_notifyChange;

      public abstract CharacteristicProperty Properties { get; }

      public abstract Byte[] Value { get; }

      public abstract void Dispose();

      protected abstract void ReadInternal( TaskCompletionSource<Byte[]> tcs );

      protected abstract void RegisterForNotification();

      protected abstract void UnregisterForNotification();

      protected abstract void WriteInternal( TaskCompletionSource<Object> tcs, Byte[] value );

      protected BaseGattCharacteristic( TNative nativeCharacteristic, Guid id )
      {
         this.nativeCharacteristic = nativeCharacteristic;
         Id = id;
         descriptors = new HashSet<IGattDescriptor>();
      }

      public event Action<IGattCharacteristic> NotifyChange
      {
         add
         {
            if(this.CanNotify())
            {
               m_notifyChange += value;
               if(!m_notificationsActive)
               {
                  //Log.Debug( "First notification handler added to characteristic, registering for notifications" );
                  m_notificationsActive = true;
                  RegisterForNotification();
               }
            }
            else
            {
               throw new InvalidOperationException(
                  "BLE GATT characteristic {0} does not support {1}".F( Id, CharacteristicProperty.Notify ) );
            }
         }
         remove
         {
            if(this.CanNotify())
            {
               m_notifyChange -= value;
               if(m_notifyChange.GetInvocationList().Length == 0 && m_notificationsActive)
               {
                  //Log.Debug( "Last notification handler removed from characteristic, unregistering for notifications" );
                  UnregisterForNotification();
                  m_notificationsActive = false;
               }
            }
            else
            {
               throw new InvalidOperationException(
                  "BLE GATT characteristic {0} does not support {1}".F( Id, CharacteristicProperty.Notify ) );
            }
         }
      }

      public IEnumerable<IGattDescriptor> Descriptors
      {
         get { return descriptors; }
      }

      public Guid Id { get; }

      public override Boolean Equals( Object obj )
      {
         var ch = obj as IGattCharacteristic;
         return ch != null && ch.Id == Id;
      }

      public IGattDescriptor GetDescriptor( Guid guid )
      {
         return descriptors.FirstOrDefault( x => x.Id == guid );
      }

      public override Int32 GetHashCode()
      {
         return Id.GetHashCode();
      }

      public Task<Byte[]> Read()
      {
         var tcs = new TaskCompletionSource<Byte[]>();
         if(!this.CanRead())
         {
            tcs.SetException(
               new InvalidOperationException(
                  "BLE GATT characteristic {0} does not support {1}".F( Id, CharacteristicProperty.Read ) ) );
         }
         else
         {
            ReadInternal( tcs );
         }
         return tcs.Task;
      }

      public override String ToString()
      {
         return "{{Gatt Characteristic {0}}}".F( Id );
      }

      public Task Write( Byte[] data )
      {
         var tcs = new TaskCompletionSource<Object>();
         if(!this.CanWrite())
         {
            tcs.SetException(
               new InvalidOperationException(
                  "BLE GATT characteristic {0} does not support {1}".F( Id, CharacteristicProperty.Write ) ) );
         }
         else
         {
            WriteInternal( tcs, data );
         }
         return tcs.Task;
      }

      protected void OnNotifyChanged()
      {
         var func = m_notifyChange;
         if(func != null)
         {
            try
            {
               func( this );
            }
            catch(Exception ex)
            {
               Log.Error( ex );
            }
         }
      }
   }
}