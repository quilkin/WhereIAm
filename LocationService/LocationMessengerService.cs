using System;
using Android.App;
using Android.OS;
using Android.Content;
using Android.Util;
using Java.Interop;

namespace StockService
{
    [Service]
    [IntentFilter(new String[]{"com.xamarin.LocationMessengerService"})]
    public class LocationMessengerService : Service
    {
        Messenger LocationMessenger;

        public LocationMessengerService ()
        {
            LocationMessenger = new Messenger (new LocationHandler ());
        }

        public override IBinder OnBind (Intent intent)
        {
            Log.Debug ("StockMessengerService", "client bound to service");

            return LocationMessenger.Binder;
        }

        class LocationHandler : Handler
        {
            public override void HandleMessage (Message msg)
            {
                Log.Debug ("LocationMessengerService", msg.What.ToString ());

                string text = msg.Data.GetString ("InputText");

                Log.Debug ("LocationMessengerService", "InputText = " + text);
            }
        }
    }
}


