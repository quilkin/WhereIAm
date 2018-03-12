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

        [OperationContract]
        [WebInvoke(Method = "OPTIONS", UriTemplate = "/Login", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        Login LoginOptions(Login login);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/GetWeather", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType(typeof(List<WeatherData>))]
        //IEnumerable<WeatherData> GetWeather(DataRequest query);
        IEnumerable<WeatherData> GetWeather(int query);

        //[OperationContract]
        //[WebInvoke(Method = "POST", UriTemplate = "/SaveLocation", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        //string SaveLocation(Location loc);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/GetLocations", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType(typeof(List<Location>))]
        IEnumerable<Location> GetLocations(int userID);

        [OperationContract]
        [WebInvoke(Method = "OPTIONS", UriTemplate = "/GetLocations", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType(typeof(List<Location>))]
        IEnumerable<Location> GetLocationsOptions(int userID);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SaveWeather", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string SaveWeather(WeatherData weather);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SaveCamper", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string SaveCamper(CamperData camper);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SaveLocation", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string SaveLocation(Location loc);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/GetCamper", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType(typeof(List<WeatherData>))]
        //IEnumerable<WeatherData> GetWeather(DataRequest query);
        IEnumerable<CamperData> GetCamper(int query);
    }


    //[ServiceContract]
    //public interface IWebPhone
    //{
    //    [OperationContract]
    //    [WebInvoke(Method = "POST", UriTemplate = "/SaveLocation", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
    //    string SaveLocation(Location loc);
    //}
}
