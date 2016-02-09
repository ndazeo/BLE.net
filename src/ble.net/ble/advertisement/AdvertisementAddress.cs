using System;
using System.Diagnostics.Contracts;

namespace bluetooth.ble.advertisement
{
   /// <summary>
   /// AdvA
   /// </summary>
   public class AdvertisementAddress
   {
      public AdvertisementAddress( Byte[] advertisementAddress, Boolean isRandomAddress )
      {
         Contract.Requires( advertisementAddress != null );
         Contract.Requires( advertisementAddress.Length == 6 );

         Value = advertisementAddress;
         IsRandomAddress = isRandomAddress;
      }

      public Boolean IsRandomAddress { get; private set; }

      // TODO: convert to MAC
      public Byte[] Value { get; private set; }

      [ContractInvariantMethod]
      private void InvariantMethod()
      {
         Contract.Invariant( Value.Length == 6 );
      }
   }
}