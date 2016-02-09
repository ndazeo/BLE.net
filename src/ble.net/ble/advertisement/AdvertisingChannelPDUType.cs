namespace bluetooth.ble.advertisement
{
   public enum AdvertisingChannelPDUType
   {
      /// <summary>
      /// ADV_IND
      /// </summary>
      AdvertisementConnectableUndirected = 0,
      /// <summary>
      /// ADV_NONCONN_IND
      /// </summary>
      AdvertisementUnconnectableUndirected = 2,
      /// <summary>
      /// ADV_SCAN_IDN
      /// </summary>
      AdvertisementScannableUndirected = 6,
      /// <summary>
      /// ADV_DIRECT_IND
      /// </summary>
      AdvertisementConnectableDirect = 1,

      /// <summary>
      /// SCAN_REQ
      /// </summary>
      ScanRequest = 3,
      /// <summary>
      /// SCAN_RSP
      /// </summary>
      ScanResponse = 4,

      /// <summary>
      /// CONNECT_REQ
      /// </summary>
      ConnectionRequest = 5
   }
}