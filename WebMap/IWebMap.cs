using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace WebMap
{

    [ServiceContract]
    public interface IWebMap
    {

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/Login", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Login Login(Login login);

        //[OperationContract]
        //[WebInvoke(Method = "POST", UriTemplate = "/Signup", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        //string Signup(Login login);
              
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SaveLocation", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string SaveLocation(Location loc);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/GetLocations", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType(typeof(List<Location>))]
        IEnumerable<Location> GetLocations(int userID);
    }

}
