using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

using System.Web.Script.Serialization;
using System.Runtime.Serialization;

namespace WebPhone
{

    [DataContract]
    public class Location
    {
        [DataMember(Name = "latitude")]
        public double Latitude { get; set; }
        [DataMember(Name = "longitude")]
        public double Longitude { get; set; }
        //[DataMember(Name = "speed")]
        //public double Speed { get; set; }
        //[DataMember(Name = "bearing")]
        //public double Bearing { get; set; }
        //[DataMember(Name = "altitude")]
        //public double Altitude { get; set; }
        [DataMember(Name = "recorded_at")]
        public String Time { get; set; }
        [DataMember(Name = "owner")]
        public int Owner { get; set; }

        public Location(double lat, double lon, String t, int ow)
        {
            Latitude = lat;
            Longitude = lon;
            Time = t;
            Owner = ow;
        }

    }

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class WebPhone : IWebPhone, IDisposable
    {
        SqlConnection mapConnection;

        List<Location> locations;
        //List<WeatherData> weatherdata;
        DataTable dataLogins;

        string connection = Connections.connection;
        string smtpserver = Connections.smtpserver;
        string smtpUserName = Connections.smtpUserName;
        string smtpPassword = Connections.smtpPassword;

        public static string TimeString(DateTime time)
        {
            if (time == DateTime.MinValue)
                return System.DBNull.Value.ToString();
            return string.Format("{0}{1}{2} {3}:{4}:{5}",
                time.Year, time.Month.ToString("00"), time.Day.ToString("00"),
                time.Hour.ToString("00"), time.Minute.ToString("00"), time.Second.ToString("00"));
        }
        public static string TimeStringNoSecs(DateTime time)
        {
            if (time == DateTime.MinValue)
                return System.DBNull.Value.ToString();
            return string.Format("{0}{1}{2} {3}:{4}",
                time.Year, time.Month.ToString("00"), time.Day.ToString("00"),
                time.Hour.ToString("00"), time.Minute.ToString("00"));
        }

        public WebPhone()
        {
        }
        public void Dispose()
        {
            if (dataLogins != null)
                dataLogins.Dispose();
            if (mapConnection != null)
                mapConnection.Dispose();
        }

        private string getIP()
        {
            OperationContext oOperationContext = OperationContext.Current;
            MessageProperties oMessageProperties = oOperationContext.IncomingMessageProperties;
            RemoteEndpointMessageProperty oRemoteEndpointMessageProperty = (RemoteEndpointMessageProperty)oMessageProperties[RemoteEndpointMessageProperty.Name];

            string szAddress = oRemoteEndpointMessageProperty.Address;
            int nPort = oRemoteEndpointMessageProperty.Port;
            return szAddress;
        }

        public class LogEntry
        {
            public string IP { get; set; }
            public string Function { get; set; }
            public string Args { get; set; }
            public string Error { get; set; }
            public string Result { get; set; }

            public LogEntry(string ip, string func, string args)
            {
                IP = ip;
                Args = args;
                Function = func;

            }
            public LogEntry()
            {
            }
            public void Save(SqlConnection conn)
            {
                //if (this.Error != null)
                //    this.Error = this.Error.Substring(0, 125);
                //if (this.Result != null)
                //    this.Result = this.Result.Substring(0, 125);
                string query = string.Format("insert into log (time,ip,func,args,result,error) values ('{0}','{1}','{2}','{3}','{4}','{5}')",
                    WebPhone.TimeString(DateTime.Now), this.IP, this.Function, this.Args, this.Result, this.Error);

                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, conn))
                {
                    command.ExecuteNonQuery();
                }

            }
        }

        public string SaveLocation(Location loc)
        {
            LogEntry log = new LogEntry(getIP(), "SaveLocation", new JavaScriptSerializer().Serialize(loc));

            // Only save a new location if it is different enough from pevious ones, 
            //  except that the last two locations always stored so that we can see how long we have been stopped for

            int successRows = 0;
            string query = string.Format("SELECT TOP 2 lat, lon, dt, id FROM locations  where owner = {0}  order by id desc", loc.Owner);
            string result = "";
            try
            {
                mapConnection = new SqlConnection(connection);
                mapConnection.Open();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return ex.Message;

            }

            // get latest two entries
            using (SqlDataAdapter loginAdapter = new SqlDataAdapter(query, mapConnection))
            {
                dataLogins = new DataTable();
                loginAdapter.Fill(dataLogins);

                if (dataLogins.Rows.Count >= 2)
                {
                    DataRow dr = dataLogins.Rows[0];
                    double latitude1 = Convert.ToDouble(dr["lat"]);
                    double longitude1 = Convert.ToDouble(dr["lon"]);
                    DateTime time1 = (DateTime)dr["dt"];
                    int id = (int)dr["id"];

                    dr = dataLogins.Rows[1];
                    double latitude2 = Convert.ToDouble(dr["lat"]);
                    double longitude2 = Convert.ToDouble(dr["lon"]);
                    DateTime time2 = (DateTime)dr["dt"];


                    double diffLat = Math.Abs(latitude1 - latitude2);
                    double diffLon = Math.Abs(longitude1 - longitude2);
                    double distance = Math.Sqrt(Math.Abs(diffLat * diffLat + diffLon * diffLon));
                    if (distance < 0.001)
                    {
                        // last two entries were same location. See if this is different now.
                        latitude2 = loc.Latitude;
                        longitude2 = loc.Longitude;
                        diffLat = Math.Abs(latitude1 - latitude2);
                        diffLon = Math.Abs(longitude1 - longitude2);
                        distance = Math.Sqrt(Math.Abs(diffLat * diffLat + diffLon * diffLon));
                        if (distance < 0.001)
                        {
                            // Still not moved much. Don't add new location, just update time for last one
                            string T = TimeString(DateTime.Now);
                            query = string.Format("update locations set dt = '{0}' where id= {1}", T, id);

                            try
                            {
                                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, mapConnection))
                                {
                                    successRows = command.ExecuteNonQuery();
                                }
                                if (successRows == 1)
                                    result = string.Format("Time updated for user {0} at {1}", loc.Owner, DateTime.Now);
                                else
                                    result = string.Format("Database error: update not saved");
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(ex.Message);
                                log.Error = ex.Message;
                                result = ex.Message;
                            }
                            finally
                            {
                                log.Result = result;
                                log.Save(mapConnection);
                                mapConnection.Close();

                            }
                            return result;
                        }
                    }
                }

            }
            // new position, add a new entry
            try
            {
                string T = TimeString(DateTime.Now);

                query = string.Format("insert into locations (lat,lon,dt,owner) values ('{0}','{1}','{2}',{3})",
                        loc.Latitude, loc.Longitude, T, loc.Owner);


                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, mapConnection))
                {
                    successRows = command.ExecuteNonQuery();
                }
                if (successRows == 1)
                    result = string.Format("Location {0} {1} saved OK at {2} for {3}", loc.Latitude, loc.Longitude, DateTime.Now, loc.Owner);
                else
                    result = string.Format("Database error: Location not saved");

            }

            catch (Exception ex)
            {

                Trace.WriteLine(ex.Message);
                log.Error = ex.Message;
                //return ex.Message;
            }


            finally
            {
                log.Result = result;
                log.Save(mapConnection);
                mapConnection.Close();
            }
            return result;

        }

        //public string SaveWeather(WeatherData w)
        //{
        //    string result = "";
        //    int successRows = 0;
        //    try
        //    {
        //        mapConnection = new SqlConnection(connection);
        //        mapConnection.Open();
        //    }
        //    catch (Exception ex)
        //    {
        //        Trace.WriteLine(ex.Message);
        //        return ex.Message;

        //    }
        //    LogEntry log = new LogEntry(getIP(), "SaveWeather", new JavaScriptSerializer().Serialize(w));
        //    try
        //    {
        //        string T = TimeString(DateTime.Now);

        //        string query = string.Format("insert into weather (shadeT,indoorT,hum,rainperhour,winddir,windspeed,time) values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
        //               (int)(w.OutdoorTemp * 10 + 0.5),
        //               (int)(w.IndoorTemp * 10 + 0.5),
        //               w.Humidity, w.Rainfall, w.Direction, w.Speed, T);


        //        using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, mapConnection))
        //        {
        //            successRows = command.ExecuteNonQuery();
        //        }
        //        if (successRows == 1)
        //            result = string.Format("Weather saved OK at {0} ", DateTime.Now);
        //        else
        //            result = string.Format("Database error: Weather not saved");

        //    }

        //    catch (Exception ex)
        //    {

        //        Trace.WriteLine(ex.Message);
        //        log.Error = ex.Message;
        //        //return ex.Message;
        //    }


        //    finally
        //    {
        //        log.Result = result;
        //        log.Save(mapConnection);
        //        mapConnection.Close();
        //    }
        //    return result;

        //}
    }
}

    