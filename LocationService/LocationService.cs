using System;
using Android.App;
using Android.Util;
using Android.Content;
using Android.OS;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace LocationService
{
    public class Location
    {
        public double Latitude;
        public double Longitude;
        public DateTime time;
        public bool same;
        public static int owner;
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
        Android.Media.MediaPlayer _player;
        Plugin.Geolocator.Abstractions.IGeolocator locator;
        //Plugin.TextToSpeech.Abstractions.ITextToSpeech TTS;
        int positionCount = 0;
        //double lastLatitude, lastLongitude;
        Location lastLocation;
        List<Location> savedLocations;
        const int maxLocations = 200;
        bool testfunc = true;
        int skip = 0, skips = 0;

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
            Location.owner = prefs.GetInt("owner", 0);

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
            //TTS = Plugin.TextToSpeech.CrossTextToSpeech.Current;
            locator.DesiredAccuracy = 50;
            //locator.StartListeningAsync(12000, 0);
            locator.StartListeningAsync(120000, 0);


            locator.PositionChanged += Locator_PositionChanged;
            locator.PositionError += Locator_PositionError;
            
           // _player = Android.Media.MediaPlayer.Create(this, Resource.Raw.Alarm);
            //TTS.Speak("service started");

            //binder = new LocationServiceBinder(this);

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

        private void Locator_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            if (locator.IsGeolocationEnabled && locator.IsGeolocationAvailable)
            {
                var position = e.Position;
                //if (testfunc)
                //{
                //    position.Latitude += 0.01;
                //}
                //testfunc = !testfunc;
                double diffLat = position.Latitude - lastLocation.Latitude;

                double diffLon = position.Longitude - lastLocation.Longitude;
                double distance = Math.Sqrt(Math.Abs(diffLat * diffLat + diffLon * diffLon));

                if (distance < 0.001)
                {
                    // not really moved; delay transmissions
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
                    using (WebClient client = new WebClient())
                    {
                        string json = string.Format("{{\"latitude\":{0},\"longitude\":{1},\"recorded_at\":\"{2}\",\"owner\":{3}}}", 
                            position.Latitude, position.Longitude, DBTime(thisLocation.time),Location.owner);
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
      
                        
                        string result = client.UploadString(UrlBase.urlBase + "SaveLocation", json);
                        builder.SetContentTitle(string.Format("lat:{0},long:{1}", position.Latitude.ToString("00.0000"), position.Longitude.ToString("00.0000")));
                        builder.SetAutoCancel(true);

                        if (savedLocations.Count > 0)
                        {
                            // send saved locations now
                            foreach (Location loc in savedLocations)
                            {
                                json = string.Format("{{\"latitude\":{0},\"longitude\":{1},\"recorded_at\":\"{2}\",\"owner\":{3}}}",
                                    loc.Latitude, loc.Longitude,DBTime(loc.time), Location.owner);
                                 client.Headers[HttpRequestHeader.ContentType] = "application/json";
                                client.UploadString(UrlBase.urlBase + "SaveLocation", json);
                            }
                            builder.SetContentTitle(string.Format("{0} stored locations saved", savedLocations.Count));
                            savedLocations.Clear();
                            //skips = 0;
                            //skip = 0;
                        }
                        
                        builder.SetContentText("Posted to Web at " + DateTime.Now.ToShortTimeString());
                        notification = builder.Build();
                        notificationManager.Notify(notificationID, notification);
                    }
                }
                catch (Exception ex)
                {
                    // cannot get web access at present.
                    // need to temporarily store some locations

                    // may need to miss out some values if storage has been exceeded        
                    //while (skip > 0)
                    //{ --skip; return; }

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
                            //if (skips == 0)
                            //    skips = 2;
                            //else
                            //    skips = skips + skips;
                        }
                        builder.SetContentTitle("Shortened list @" + DateTime.Now.ToShortTimeString());
                        builder.SetContentText(savedLocations.Count + " positions stored");
                    }
                    builder.SetContentTitle("Web error @" + DateTime.Now.ToShortTimeString());
                    builder.SetContentText(savedLocations.Count + " positions stored");
                    notification = builder.Build();
                    notificationManager.Notify(notificationID, notification);
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
                locator.StopListeningAsync();
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