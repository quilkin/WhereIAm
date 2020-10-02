using System;
using Android.App;
using Android.Util;
using Android.Content;
using Android.OS;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
        Plugin.Geolocator.Abstractions.IGeolocator locator;
        int positionCount = 0;
        const string NOCONNECTION = "No connection available";
        Location lastLocation;
        List<Location> savedLocations;
        const int maxLocations = 200;
        Random rand = new Random();

       // HttpClient client;

        public object CrossGeolocator { get; private set; }

        public override StartCommandResult OnStartCommand (Android.Content.Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Debug ("LocationService", "LocationService started");
			StartService();
			return StartCommandResult.Sticky;
		}

		void StartService ()
		{

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

            // get new location every 2 minutes
           // TimeSpan updateFreq = new TimeSpan(TimeSpan.TicksPerMinute * 2);
            // locator.StartListeningAsync(updateFreq, 0);
            locator.StartListeningAsync(120000,0);


            locator.PositionChanged += Locator_PositionChanged;
            locator.PositionError += Locator_PositionError;

            //client = new HttpClient();

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

        private void RandomiseForTesting(ref double lat, ref double lng)
        {
            lat += (rand.Next(100) / 1000.0) - 0.05;
            lng += (rand.Next(100) / 1000.0) - 0.05;
        }

        private async Task<HttpResponseMessage> UploadResult(double lat, double lng, DateTime t)
        {
      //      RandomiseForTesting(ref lat, ref lng);

            lat = Math.Round(lat, 5);
            lng = Math.Round(lng, 5);
            using (HttpClient httpClient = new HttpClient())
            {
                string json = string.Format("{{\"latitude\":{0:0.00000},\"longitude\":{1:0.00000},\"recorded_at\":\"{2}\",\"owner\":{3}}}",
                lat, lng, DBTime(t), Location.owner);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                HttpResponseMessage response = null;
                Uri uri = new Uri(UrlBase.urlBase + "SaveLocation");
                try
                {
                   response = await httpClient.PostAsync(uri, content);
                }
                catch (Exception ex)
                {
                    if (response == null)
                    {
                        response = new HttpResponseMessage();
                    }
                    response.StatusCode = HttpStatusCode.InternalServerError;
                    response.ReasonPhrase = string.Format("Post failed: {0}", ex.Message);

                }
                return response;
 
            }
        }

        private async void Locator_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
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

                    HttpResponseMessage result = await UploadResult( position.Latitude, position.Longitude, thisLocation.time);
      
                    if (result.IsSuccessStatusCode)
                    {
                        if (savedLocations.Count > 0)
                        {
                            // send saved locations now
                            try
                            {
                                foreach (Location loc in savedLocations)
                                {

                                    result = await UploadResult( loc.Latitude, loc.Longitude, loc.time);
                                }
                                builder.SetContentTitle(string.Format("{0} stored locations saved", savedLocations.Count));
                                savedLocations.Clear();

                            }
                            catch (Exception ex2)
                            {
                                builder.SetContentTitle("Error: " + ex2.Message);
                                builder.SetContentText(savedLocations.Count + " positions stored");
                                notification = builder.Build();
                                notificationManager.Notify(notificationID, notification);
                                return;
                            }
                        }

                        builder.SetContentText("Posted to Web at " + DateTime.Now.ToShortTimeString());

                    }
                    else
                    {
                        // cannot get web access at present.
                        builder.SetContentTitle("No internet @" + DateTime.Now.ToShortTimeString());
                        builder.SetContentText(savedLocations.Count + " positions stored");
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

                    }
                }
                catch (Exception ex)
                {
                    builder.SetContentTitle("Error @" + DateTime.Now.ToShortTimeString());
                    builder.SetContentText(ex.Message + " " + savedLocations.Count + " positions stored");
                }

                finally
                {
                    notification = builder.Build();
                    notificationManager.Notify(notificationID, notification);
                }
            }
            else
            {
                builder.SetContentTitle("GPS error:");
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
                //TimeSpan gpsFreq = new TimeSpan(2 * TimeSpan.TicksPerMinute);
                locator.StopListeningAsync();
               // locator.StartListeningAsync(gpsFreq, 0);
                locator.StartListeningAsync(120000, 0);

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

            }
        }
        private void Locator_PositionError(object sender, Plugin.Geolocator.Abstractions.PositionErrorEventArgs e)
        {
            // warn user that GPS has stopped working
            builder.SetContentTitle("GPS error: ");
            builder.SetContentText(e.Error.ToString() + ": " + DateTime.Now.ToShortTimeString());
            notification = builder.Build();
            notificationManager.Notify(notificationID, notification);

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

        }


	}



}