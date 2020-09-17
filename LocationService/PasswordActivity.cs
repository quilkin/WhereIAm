using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LocationService
{
    [Activity(Label = "PasswordActivity")]
    public class PasswordActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.password);
            SetTitle(Resource.String.new_password);

            SetActions();
        }

        private void SetActions()
        {
            var userOK = FindViewById<Button>(Resource.Id.buttonNewOK);
            userOK.Click += UserOK_Click;
            var cancel = FindViewById<Button>(Resource.Id.newCancel);
            cancel.Click += Cancel_Click;
            var usertext = FindViewById<TextView>(Resource.Id.textUsername);
            usertext.SetText(Location.username, TextView.BufferType.Normal);
        }
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Finish();
        }
        private void UserOK_Click(object sender, EventArgs e)
        {
            string pwtext = FindViewById<EditText>(Resource.Id.editNewPass).Text;
            var textError = FindViewById<TextView>(Resource.Id.textNewError);
            string pwtext2 = FindViewById<EditText>(Resource.Id.editNewPass2).Text; 
           // var lbl2 = FindViewById<TextView>(Resource.Id.textNewPass2);


            if (pwtext.Length < 4 || pwtext.Length > 10)
                textError.Text = "Password must be between 4 and 10 characters";
            else
            {
                textError.Text = "";
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    using (WebClient client = new WebClient())
                    {
                        string json, result;

                       // check contents
                        if (pwtext2 == pwtext)
                        {

                            json = string.Format("{{\"name\":\"{0}\",\"pw\":\"{1}\",\"id\":-2}}", Location.username, pwtext);
                            client.Headers[HttpRequestHeader.ContentType] = "application/json";

                            result = client.UploadString(UrlBase.urlBase + "Login", json);
                            if (result.Contains(Location.username) && result.Contains(pwtext) && result.Contains("id"))
                            {
                                textError.Text = "You have successfully updated your password";
                                //pw2.Visibility = ViewStates.Invisible;
                                //lbl2.Visibility = ViewStates.Invisible;
                                //SaveID(result);
                            }
                        }
                        else
                        {
                            textError.Text = "Passwords do not match";
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
    }
}