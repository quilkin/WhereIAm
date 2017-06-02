using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WebMap
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {
            //if (Request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            //{
            //    //  System.Web.HttpResponse response = this.Response;
            //    Response.StatusCode = 0x000000c8;

            //    return;


            //}
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //var https = Request.ServerVariables["HTTPS"];
            //bool newLocation = Request.Path.Contains("SaveLocation");
            //// allow only updates from the field to access with non-https (to avoid ssl overhead)
            //if (https == "on" && newLocation)
            //{
            //    Response.Redirect("http://" + Request.ServerVariables["HTTP_HOST"] + Request.ServerVariables["UNENCODED_URL"]);
            //}

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {
            //if (Request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            //{
            //   // System.Web.HttpResponse response = this.Response;
            //    Response.StatusCode = 0x000000c8;
            //    return;
            //}
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}