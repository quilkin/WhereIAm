using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace WebPhone
{
    
    [ServiceContract]
    public interface IWebPhone
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/SaveLocation", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        string SaveLocation(Location loc);
    }
    }
