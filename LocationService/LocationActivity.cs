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
      //  public static string urlBase = "http://CE568/WebMap/WebMap.svc/";
    }



    [Activity (Label = "Where Am I?", MainLauncher = true)]
	public class LocationActivity : Activity
	{
        //       Toast messageToast;
        bool ServiceRunning = false;
        ISharedPreferences prefs;
        ISharedPreferencesEditor editor;

        protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);
            // retreive saved user
            prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            editor = prefs.Edit();

            try
            {
                Location.owner = prefs.GetInt("owner", 0);
            }
            catch
            {
                Location.owner = 0;
            }
            try
            {
                ServiceRunning = prefs.GetBoolean("running", false);
            }
            catch
            {
                ServiceRunning = false;
            }

            SetActions();


        }
        private void Message(string m)
        {
            Toast messageToast = Toast.MakeText(this,m, ToastLength.Long);
            messageToast.SetGravity(GravityFlags.Center, 0, 0);
         //   messageToast.SetMargin(30, 30);
            messageToast.Show();

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
            if (ServiceRunning)
            {
                start.Enabled = false;
            }
            else
            {
                stop.Enabled = false;
            }

            start.Click += delegate {

                serviceIntent = new Intent(this, typeof(LocationService));
                StartService(serviceIntent);
                start.Enabled = false;
                stop.Enabled = true;
                Message("Location recording started. It will continue in background, including after reboot, until 'stop service' is used");
                editor.PutBoolean("running", true);
                editor.Apply();
            };


            stop.Click += delegate {
               // StopService(new Intent("com.xamarin.LocationService"));
                serviceIntent = new Intent(this, typeof(LocationService));
                StopService(serviceIntent);
                stop.Enabled = false;
                if (Location.owner > 0)
                    start.Enabled = true;
                Message("Location recording has been halted");
                editor.PutBoolean("running", false);
                editor.Apply();

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


            int idpos1 = result.IndexOf("\"id\":") + 5;
            int idpos2 = result.IndexOf(",", idpos1);
            int id = int.Parse(result.Substring(idpos1, idpos2 - idpos1));
            Location.owner = id;
            editor.PutInt("owner", id);
            editor.Apply();        // applies changes asynchronously on newer APIsd;
        }



        private void UserOK_Click(object sender, EventArgs e)
        {
            string usertext = FindViewById<EditText>(Resource.Id.editUser).Text;
            string pwtext = FindViewById<EditText>(Resource.Id.editPass).Text;
            var textError = FindViewById<TextView>(Resource.Id.textError);
            var pw2 = FindViewById<EditText>(Resource.Id.editPass2);
            var lbl2 = FindViewById<TextView>(Resource.Id.textPass2);
            

            if (usertext.Contains(" ") || pwtext.Contains(" "))
            {
                Message("Username or password cannot have any spaces!");
            }
            else if (pwtext.Length < 4 || pwtext.Length > 10)
                Message("Password must be between 4 and 10 characters");
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

                                json = string.Format("{{\"name\":\"{0}\",\"pw\":\"{1}\",\"id\":-1}}", usertext, pwtext);
                                //var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                                result = client.UploadString(UrlBase.urlBase + "Login", json);

                                if (result.Contains(usertext) && result.Contains(pwtext) && result.Contains("id"))
                                {
                                    textError.Text = "You have created a new account!";
                                    pw2.Visibility = ViewStates.Invisible;
                                    lbl2.Visibility = ViewStates.Invisible;
                                    SaveID(result);
                                    SetContentView(Resource.Layout.Main);
                                    SetActions();
                                }
                            }
                            else
                            {
                                Message("Passwords do not match");
                            }
                        }
                        else
                        {

                            json = string.Format("{{\"name\":\"{0}\",\"pw\":\"{1}\"}}", usertext, pwtext);

                            client.Headers[HttpRequestHeader.ContentType] = "application/json";
                            result = client.UploadString(UrlBase.urlBase + "Login", json);

                            if (result.Contains(usertext) && result.Contains(pwtext) && result.Contains("id"))
                            {
                                Message("You are logged in");
                                SaveID(result);
                                SetContentView(Resource.Layout.Main);
                                SetActions();
                            }
                            else if (result.Contains(usertext))
                            {
                                Message("User exists, but incorrect password. Please choose a new username if you wish to create a new account");
                            }
                            else
                            {
                                pw2.Visibility = ViewStates.Visible;
                                lbl2.Visibility = ViewStates.Visible;
                                Message("Please confirm password to create a new account");
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    // cannot get web access at present.
                    Message("Web error: " + ex.Message);
                }
            }
        }


        protected override void OnStart ()
		{
			base.OnStart ();

        }

		protected override void OnDestroy ()
		{
			base.OnDestroy ();
        }


     
	}
}


