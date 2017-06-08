using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using Android.Preferences;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Content.PM;

namespace LocationService
{

    public class UrlBase
    {
        public static string urlBase = "https://www.quilkin.co.uk/WebMap.svc/";
        //public static string urlBase = "https://CE568/WebMap/WebMap.svc/";
        //public static string urlBase = "http://timetrials.org.uk/Service1.svc/";
    }



    [Activity (Label = "LocationService", MainLauncher = true)]
	public class LocationActivity : Activity
	{
		//bool isBound = false;
		//bool isConfigurationChange = false;
		//LocationServiceBinder binder;
		//LocationServiceConnection locationServiceConnection;
      


		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);
            // retreive saved user
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            try
            {
                Location.owner = prefs.GetInt("owner", 0);
            }
            catch
            {
                Location.owner = 0;
            }

            SetActions();

			//// restore from connection there was a configuration change, such as a device rotation
			//locationServiceConnection = LastNonConfigurationInstance as LocationServiceConnection;

			//if (locationServiceConnection != null)
			//	binder = locationServiceConnection.Binder;


        }


        private void SetActions()
        {
            var start = FindViewById<Button>(Resource.Id.startService);
            var stop = FindViewById<Button>(Resource.Id.stopService);
            var user = FindViewById<Button>(Resource.Id.setUsername);

            Intent serviceIntent;
           // stop.Enabled = false;
            if (Location.owner == 0)
            {
                // need to log in before starting GPS service
                start.Enabled = false;
            }
            else
            {
                user.SetText(Resource.String.set_new_user);
            }

            start.Click += delegate {

                //Intent bi = new Intent("com.android.vending.billing.InAppBillingService.BIND");
                //bi.setPackage("com.android.vending");
                serviceIntent = new Intent(this, typeof(LocationService));
                StartService(serviceIntent);
                //StartService(new Intent("com.xamarin.LocationService"));
                start.Enabled = false;
                stop.Enabled = true;
            };


            stop.Click += delegate {
               // StopService(new Intent("com.xamarin.LocationService"));
                serviceIntent = new Intent(this, typeof(LocationService));
                StopService(serviceIntent);
                stop.Enabled = false;
                if (Location.owner > 0)
                    start.Enabled = true;
            //    locationServiceConnection.activity.isBound = false;
            };
            user.Click += delegate
            {
                SetContentView(Resource.Layout.user);
                var userOK = FindViewById<Button>(Resource.Id.buttonOK);
                userOK.Click += UserOK_Click;
                var cancel = FindViewById<Button>(Resource.Id.cancel);
                cancel.Click += Cancel_Click;

            };
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            SetContentView(Resource.Layout.Main);
            SetActions();
        }

        private void SaveID(string result)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor editor = prefs.Edit();

            int idpos1 = result.IndexOf("\"id\":") + 5;
            int idpos2 = result.IndexOf(",", idpos1);
            int id = int.Parse(result.Substring(idpos1, idpos2 - idpos1));
            Location.owner = id;
            editor.PutInt("owner", id);
            editor.Apply();        // applies changes asynchronously on newer APIsd;
        }

        //public async Task<string>  GetJsonAsync(string json,string method)
        //{
        //    //using (HttpClient client = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler()))
        //    using (HttpClient client = new HttpClient())
        //    {
        //        var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
        //        var response = await client.PostAsync(UrlBase.urlBase + method, content);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var result = response.Content.ReadAsStringAsync().Result;
        //            //dynamic result = response.Content.ReadAsStringAsync().Result;
        //            // Access variables from the returned JSON object
        //            //var appHref = result.links.applications.href;
        //            //return appHref;
        //            return result.ToString();
        //        }
        //        return "";
        //    }
        //}


        private void UserOK_Click(object sender, EventArgs e)
        {
            string usertext = FindViewById<EditText>(Resource.Id.editUser).Text;
            string pwtext = FindViewById<EditText>(Resource.Id.editPass).Text;
            var textError = FindViewById<TextView>(Resource.Id.textError);
            var pw2 = FindViewById<EditText>(Resource.Id.editPass2);
            var lbl2 = FindViewById<TextView>(Resource.Id.textPass2);
            

            if (usertext.Contains(" ") || pwtext.Contains(" "))
            {
                textError.Text = "Username or password cannot have any spaces!";
            }
            else if (pwtext.Length < 4 || pwtext.Length > 10)
                textError.Text = "Password must be between 4 and 10 characters";
            else
            {
                textError.Text = "";
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    //using (HttpClient client = new HttpClient(new Xamarin.Android.Net.AndroidClientHandler()))
                    using (WebClient client = new WebClient())
                    {
                        string json, result;
                        if (pw2.Visibility == ViewStates.Visible)
                        {
                            //already dsplayed confirmation text boxes; check contents
                            string pwtext2 = pw2.Text;
                            if (pwtext2 == pwtext)
                            {
                                //SHA512Managed sha512 = new SHA512Managed();
                                //byte[] hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(usertext+pwtext+"chloefrances"));
                                //string hexHash = BitConverter.ToString(hash);
                                //json = string.Format("{{\"hash\":\"{0}\"}}", hexHash);
                                //client.Headers[HttpRequestHeader.ContentType] = "application/json";
                                //result = client.UploadString(UrlBase.urlBase + "CheckHash", json);
                                //if (result.Contains("OK") )
                                //{
                                //    textError.Text = "You have connected to your account!";
                                //    pw2.Visibility = ViewStates.Invisible;
                                //    lbl2.Visibility = ViewStates.Invisible;
                                //    SaveID(hexHash);
                                //}

                                json = string.Format("{{\"name\":\"{0}\",\"pw\":\"{1}\",\"id\":-1}}", usertext, pwtext);
                                //var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                                result = client.UploadString(UrlBase.urlBase + "Login", json);
                                //result = client.PostAsync(UrlBase.urlBase + "Login", content).Result.ToString();
                                //var result = GetJsonAsync(json,"Login");
                                //string resultStr = result.ToString();
                                if (result.Contains(usertext) && result.Contains(pwtext) && result.Contains("id"))
                                {
                                    textError.Text = "You have created a new account!";
                                    pw2.Visibility = ViewStates.Invisible;
                                    lbl2.Visibility = ViewStates.Invisible;
                                    SaveID(result);
                                }
                            }
                            else
                            {
                                textError.Text = "Passwords do not match";
                            }
                        }
                        else
                        {
                            //SHA512Managed sha512 = new SHA512Managed();
                            //byte[] hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(usertext + pwtext + "chloefrances"));
                            //string hexHash = BitConverter.ToString(hash);
                            //json = string.Format("{{\"hash\":\"{0}\"}}", hexHash);
                            //client.Headers[HttpRequestHeader.ContentType] = "application/json";
                            //result = client.UploadString(UrlBase.urlBase + "CheckHash", json);
                            //if (result.Contains("OK"))
                            //{
                            //    textError.Text = "You have connected to your account!";
                            //    pw2.Visibility = ViewStates.Invisible;
                            //    lbl2.Visibility = ViewStates.Invisible;
                            //    SaveID(hexHash);
                            //}
                            json = string.Format("{{\"name\":\"{0}\",\"pw\":\"{1}\"}}", usertext, pwtext);

                            client.Headers[HttpRequestHeader.ContentType] = "application/json";
                            result = client.UploadString(UrlBase.urlBase + "Login", json);
                            //var result = GetJsonAsync(json,"Login");
                            //string resultStr = result.ToString();
                            if (result.Contains(usertext) && result.Contains(pwtext) && result.Contains("id"))
                            {
                                textError.Text = "You are logged in";
                                SaveID(result);

                            }
                            else if (result.Contains(usertext))
                            {
                                textError.Text = "User exists, but incorrect password. Please choose a new username if you wish to create a new account";
                            }
                            else
                            {
                                pw2.Visibility = ViewStates.Visible;
                                lbl2.Visibility = ViewStates.Visible;
                                textError.Text = "Please confirm password to create a new account";
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    // cannot get web access at present.
                    textError.Text = "web error: " + ex.Message;
                }
            }
        }


        protected override void OnStart ()
		{
			base.OnStart ();

			//var locationServiceIntent = new Intent ("com.xamarin.LocationService");
			//locationServiceConnection = new LocationServiceConnection (this);
			//BindService (locationServiceIntent, locationServiceConnection, Bind.AutoCreate);
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();

            //if (!isConfigurationChange)
            //{
            //    if (isBound)
            //    {
            //        UnbindService(locationServiceConnection);
            //        isBound = false;
            //    }
            //}
        }

		//// return the service connection if there is a configuration change
		//public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		//{
		//	base.OnRetainNonConfigurationInstance ();

		//	isConfigurationChange = true;

		//	return locationServiceConnection;
		//}

		//class LocationServiceConnection : Java.Lang.Object, IServiceConnection
		//{
		//	LocationActivity activity;
		//	LocationServiceBinder binder;

		//	public LocationServiceBinder Binder {
		//		get {
		//			return binder;
		//		}
		//	}

		//	public LocationServiceConnection (LocationActivity activity)
		//	{
		//		this.activity = activity;
		//	}
          
		//	public void OnServiceConnected (ComponentName name, IBinder service)
		//	{
		//		var LocationServiceBinder = service as LocationServiceBinder;
				
		//		if (LocationServiceBinder != null) {
		//			activity.binder = LocationServiceBinder;
		//			activity.isBound = true;

		//			// keep instance for preservation across configuration changes
		//			this.binder = LocationServiceBinder;
		//		}
		//	}

		//	public void OnServiceDisconnected (ComponentName name)
		//	{
		//		activity.isBound = false;
		//	}
		//}

     
	}
}


