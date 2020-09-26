using System;
using Android.App;
using Android.Util;
using Android.Content;
using Android.OS;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;

namespace LocationService
{
    public class Location
    {
        public double Latitude;
        public double Longitude;
        public DateTime time;
        public bool same;
        public static int owner;
        public static string username;

    }

    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted } ,  Categories = new[] { "android.intent.category.HOME" })]
    partial class BootReceiver : BroadcastReceiver
    {

        public override void OnReceive(Context context, Intent intent)
        {

            if ((intent.Action != null) &&
                        (intent.Action ==
                         Android.Content.Intent.ActionBootCompleted))
            {         // Start the service or activity
                context.ApplicationContext.StartService(new Intent(context, typeof(LocationService)));
            }
     

        }
    }

    [Service]
	[IntentFilter(new String[]{"com.xamarin.LocationService"})]
	public class LocationService : Service
	{
		//LocationServiceBinder binder;
        Notification.Builder builder;
        NotificationManager notificationManager;
        Notification notification;
        int notificationID = 0;
        //Android.Media.MediaPlayer _player;
        Plugin.Geolocator.Abstractions.IGeolocator locator;
        //Plugin.TextToSpeech.Abstractions.ITextToSpeech TTS;
        int positionCount = 0;
   
        Location lastLocation;
        List<Location> savedLocations;
        const int maxLocations = 200;
        Random rand = new Random();

        HttpClient client;

        public object CrossGeolocator { get; private set; }

        public override StartCommandResult OnStartCommand (Android.Content.Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Debug ("LocationService", "LocationService started");
			StartService();
			return StartCommandResult.Sticky;
		}

		void StartService ()
		{
            // Set up an intent so that tapping the notifications returns to this app:
            //  Intent intent = new Intent(this, typeof(MainApplication));

            // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
            //const int pendingIntentId = 0;
            //PendingIntent pendingIntent =
            //    PendingIntent.GetActivity(this, pendingIntentId, intent, PendingIntentFlags.OneShot);
            ISharedPreferences prefs = Android.Preferences.PreferenceManager.GetDefaultSharedPreferences(this);
            try
            {
                Location.owner = prefs.GetInt("owner", 0);
                Location.username = prefs.GetString("user", "unknown");
            }
            catch
            {
                Location.owner = 0;
            }

            builder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.icon)//.setTicker(text).setWhen(time)
                .SetContentTitle("Location Service is running")
                .SetContentText("Location Service is running");
                //.SetContentIntent(pendingIntent);
            notification = builder.Build();
            notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
            notificationID = (int)NotificationFlags.ForegroundService;
            builder.SetAutoCancel(true);
            StartForeground (notificationID, notification);

            savedLocations = new List<Location>();
            lastLocation = new Location();
            lastLocation.Latitude = 0;
            lastLocation.Longitude = 0;

            locator = Plugin.Geolocator.CrossGeolocator.Current;
            locator.DesiredAccuracy = 50;

            // get new location every minute
            TimeSpan updateFreq = new TimeSpan(TimeSpan.TicksPerMinute);
            //only update every minute and if distance changed is > 10 metres
            locator.StartListeningAsync(updateFreq, 1);


            locator.PositionChanged += Locator_PositionChanged;
            locator.PositionError += Locator_PositionError;

            client = new HttpClient();

        }
        private string DBTime(DateTime t)
        {
            // convert time into format suitable for adding to SQL DB
            int year = t.Year;
            int month = t.Month;
            int date = t.Day;
            int hour = t.Hour;
            int min = t.Minute;
            int sec = t.Second;
            return string.Format("{0}{1}{2} {3}:{4}:{5}",
                year, month.ToString("00"), date.ToString("00"),
                hour.ToString("00"), min.ToString("00"), sec.ToString("00"));

        }
        //public interface IMessage
        //{
        //    void LongAlert(string message);
        //    void ShortAlert(string message);
        //}
        //public class MessageAndroid : IMessage
        //{
        //    public void LongAlert(string message)
        //    {
        //        Android.Widget.Toast.MakeText(Application.Context, message, Android.Widget.ToastLength.Long).Show();
        //    }

        //    public void ShortAlert(string message)
        //    {
        //        Android.Widget.Toast.MakeText(Application.Context, message, Android.Widget.ToastLength.Short).Show();
        //    }
        //}

        private void popup(string message)
        {
            //  DependencyService.Get<IMessage>().ShortAlert(message);
           // Android.Widget.Toast.MakeText(Android.App.Activity., message, Android.Widget.ToastLength.Long).Show();
        }
        public void TimerCheck(Object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
        //    ++timerInvokes;
            autoEvent.Set();
        }
        private void RandomiseForTesting(ref double lat, ref double lng)
        {
            //lat += rand.Next(100) / 1000.0;
            //lng += rand.Next(100) / 1000.0;
        }

        private string UploadResult(HttpClient client, double lat, double lng, DateTime t)
        {
            //string result;
            RandomiseForTesting(ref lat, ref lng);

            // need to wait  a bit if busy??
            //var autoEvent = new AutoResetEvent(false);
            //int timerInvokes = 0;
            //Timer tim = new Timer(TimerCheck, autoEvent, 1000, 250);

            //while (client.IsBusy && ++timerInvokes < 10)
            //{
            //    autoEvent.WaitOne();
            //}
            //if (timerInvokes < 10)
            //{

            //client.Headers[HttpRequestHeader.ContentType] = "application/json";
            lat = Math.Round(lat, 5);
            lng = Math.Round(lng, 5);
            string json = string.Format("{{\"latitude\":{0:0.00000},\"longitude\":{1:0.0000},\"recorded_at\":\"{2}\",\"owner\":{3}}}",
                lat, lng, DBTime(t), Location.owner);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            Uri uri = new Uri(UrlBase.urlBase + "SaveLocation");
            //result = client.UploadString(uri, "POST",json);
            var response = client.PostAsync(uri, content);
            //}
            //else
            //{
            //    result = "WebClient timed out";
            //}
            //tim.Dispose();
            return response.ToString();
        }

        private void Locator_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            //WebClient client = new WebClient();
            if (locator.IsGeolocationEnabled && locator.IsGeolocationAvailable)
            {
                var position = e.Position;

                double diffLat = position.Latitude - lastLocation.Latitude;

                double diffLon = position.Longitude - lastLocation.Longitude;
                double distance = Math.Sqrt(Math.Abs(diffLat * diffLat + diffLon * diffLon));

                if (distance < 0.002)
                {
                    // not really moved; delay transmissions for up to 5 minutes
                    if (++positionCount < 5)
                        return;
                }
                // position has changed or has been same for 5 consecutive calls; transmit anyway.
                lastLocation.Latitude = position.Latitude;
                lastLocation.Longitude = position.Longitude;
                
                Location thisLocation = new Location();
                thisLocation.Latitude = position.Latitude;
                thisLocation.Longitude = position.Longitude;
                thisLocation.time = DateTime.Now;
                thisLocation.same = (positionCount >= 2);
                positionCount = 0;



                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                    //using (WebClient client = new WebClient())
                    //{
                    //string json = string.Format("{{\"latitude\":{0},\"longitude\":{1},\"recorded_at\":\"{2}\",\"owner\":{3}}}", 
                    //    position.Latitude, position.Longitude, DBTime(thisLocation.time),Location.owner);


                    //string result = client.UploadString(UrlBase.urlBase + "SaveLocation", json);
                    string result = UploadResult(client, position.Latitude, position.Longitude, thisLocation.time);
                       
                       // builder.SetContentTitle(string.Format("lat:{0},long:{1}", position.Latitude.ToString("00.0000"), position.Longitude.ToString("00.0000")));
                        // builder.SetAutoCancel(true);

                        if (savedLocations.Count > 0)
                        {
                            // send saved locations now
                            try
                            {
                                popup("trying to send others...1");
                                foreach (Location loc in savedLocations)
                                {
                                    //json = string.Format("{{\"latitude\":{0},\"longitude\":{1},\"recorded_at\":\"{2}\",\"owner\":{3}}}",
                                    //    loc.Latitude, loc.Longitude, DBTime(loc.time), Location.owner);
                                    //popup("trying to send others...2");
                                    //client.Headers[HttpRequestHeader.ContentType] = "application/json";
                                    //client.UploadString(UrlBase.urlBase + "SaveLocation", json);
                                    result = UploadResult(client, loc.Latitude, loc.Longitude, loc.time);
                                }
                                popup("trying to send others...3");
                                builder.SetContentTitle(string.Format("{0} stored locations saved", savedLocations.Count));
                                savedLocations.Clear();
                                popup("trying to send others...4");
                            }
                            catch (Exception ex2)
                            {
                                builder.SetContentTitle("Error: " +ex2.Message);
                                builder.SetContentText(savedLocations.Count + " positions stored");
                                notification = builder.Build();
                                notificationManager.Notify(notificationID, notification);
                                return;
                            }
                        }
                        
                        builder.SetContentText("Posted to Web at " + DateTime.Now.ToShortTimeString());
                        notification = builder.Build();
                        notificationManager.Notify(notificationID, notification);
                        
                        //client.Dispose();
                    //}
                }
                catch (Exception ex)
                {
                    //if (ex.Message.Contains("Bad Request") == false)
                    //{
                        // cannot get web access at present.?
                        // need to temporarily store some locations

                        // may need to miss out some values if storage has been exceeded        

                        if (savedLocations.Count < maxLocations)
                        {
                            savedLocations.Add(thisLocation);
                        }
                        else
                        {
                            // limit size of list for now, first remove any which are at same location
                            for (int i = savedLocations.Count - 1; i >= 0; i -= 1)
                            {
                                Location loc = savedLocations[i];
                                if (loc.same)
                                    savedLocations.RemoveAt(i);
                            }
                            if (savedLocations.Count >= maxLocations)
                            {
                                // still too many, remove every other one
                                for (int i = savedLocations.Count - 1; i >= 0; i -= 2)
                                {
                                    savedLocations.RemoveAt(i);
                                }

                            }
                            builder.SetContentTitle("Shortened list @" + DateTime.Now.ToShortTimeString());
                            builder.SetContentText(savedLocations.Count + " positions stored");
                        }
                        builder.SetContentTitle("Error @" + DateTime.Now.ToShortTimeString());
                        builder.SetContentText(ex.Message + " " + savedLocations.Count + " positions stored");
                        notification = builder.Build();
                        notificationManager.Notify(notificationID, notification);
                    //}
                }
                finally
                {
                //    client.Dispose();
                }
            }
            else
            {
                builder.SetContentTitle("GPS error:" );
                builder.SetContentText("Not available at" + DateTime.Now.ToShortTimeString());
                notification = builder.Build();
                notificationManager.Notify(notificationID, notification);
            }

        }

        class TimerState
        {
            public int counter = 0;
            public Timer tmr;
        }

        void CheckLocationStatus(Object state)
        {
            TimerState s = (TimerState)state;
            //s.counter++;
            if (locator.IsGeolocationEnabled && locator.IsGeolocationAvailable)
            {
                // it's OK now, can stop timer
                s.tmr.Dispose();
                s.tmr = null;
                TimeSpan gpsFreq = new TimeSpan(2 * TimeSpan.TicksPerMinute);
                locator.StopListeningAsync();
                locator.StartListeningAsync(gpsFreq, 0);

                locator.PositionChanged += Locator_PositionChanged;
                locator.PositionError += Locator_PositionError;
            }
            else
            {
                // warn user again
                //_player.Start();
                builder.SetContentTitle("GPS error:");
                builder.SetContentText("Trying to restart: " + DateTime.Now.ToShortTimeString());
                notification = builder.Build();
                notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
                notificationID = (int)NotificationFlags.ForegroundService;

                //locator.StopListeningAsync();
                //locator.StartListeningAsync(120000, 0);
                //StartForeground(notificationID, notification);

                //var locator = Plugin.Geolocator.CrossGeolocator.Current;
            }
        }
        private void Locator_PositionError(object sender, Plugin.Geolocator.Abstractions.PositionErrorEventArgs e)
        {
            // warn user that GPS has stopped working
            builder.SetContentTitle("GPS error: ");
            builder.SetContentText(e.Error.ToString() + ": " + DateTime.Now.ToShortTimeString());
            notification = builder.Build();
            notificationManager.Notify(notificationID, notification);
            //_player.Start();

            // create a timer to re-check and re-warn if necesssary
            TimerState s = new TimerState();
            TimerCallback timerDelegate = new TimerCallback(CheckLocationStatus);
            Timer timer = new Timer(timerDelegate, s, 60000, 60000);
            s.tmr = timer;


        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Log.Debug("LocationService", "LocationService stopped");
        }

        public override Android.OS.IBinder OnBind (Android.Content.Intent intent)
		{
            return null; 
			//binder = new LocationServiceBinder (this);
			//return binder;
        }


	}

	//public class LocationServiceBinder : Binder
	//{
	//	LocationService service;
    
	//	public LocationServiceBinder (LocationService service)
	//	{
	//		this.service = service;
	//	}

	//	public LocationService GetLocationService ()
	//	{
	//		return service;
	//	}
	//}

}