using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

using System.Web.Script.Serialization;

namespace WebMap
{

    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class WebMap : IWebMap, IDisposable
    {
        SqlConnection mapConnection;

        List<Location> locations;
        List<WeatherData> weatherdata;
        List<CamperData> camperdata;
        DataTable dataLogins;

        string connection = Connections.connection;
        string smtpserver = Connections.smtpserver;
        string smtpUserName = Connections.smtpUserName;
        string smtpPassword = Connections.smtpPassword;

        List<int> needingNewPW;

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

        public WebMap()
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



        /// <summary>
        /// Log in to the system
        /// </summary>
        /// <param name="login">login object with just a username and password</param>
        /// <returns>login object with details of role and user id</returns>
        public Login Login(Login login)
        {
            LogEntry log = new LogEntry(getIP(), "Login", login.Name + " " + login.PW);


            string query = "SELECT Id, name, pw, email, role FROM logins";
            try
            {
                mapConnection = new SqlConnection(connection);
                mapConnection.Open();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                // return ex.Message;
            }
            //int userRole = 0;
            //int userID = 0;
            using (SqlDataAdapter loginAdapter = new SqlDataAdapter(query, mapConnection))
            {
                dataLogins = new DataTable();
                loginAdapter.Fill(dataLogins);
                bool exists = false;

                int length = dataLogins.Rows.Count;
                for (int row = 0; row < length; row++)
                {
                    DataRow dr = dataLogins.Rows[row];
                    string dbname = (string)dr["name"];
                    dbname = dbname.Trim();
                    string dbpw = (string)dr["pw"];
                    dbpw = dbpw.Trim();
                    if (dbname == login.Name && dbpw == login.PW)
                    {
                        // exising credentials OK
                        login.Role = (int)dr["role"];
                        login.ID = (int)dr["Id"];
                        exists = true;
                        break;
                    }
                    if (dbname == login.Name && "NewPassword" == login.PW)
                    {
                        // new password required
                        login.ID = (int)dr["Id"];
                        query = string.Format("update logins set clubID = 1 where Id = '{0}'\n\r", login.ID);
                        using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, mapConnection))
                        {
                            command.ExecuteNonQuery();
                        }
                        exists = true;
                        break;
                    }
                    if (dbname == login.Name && dbpw != login.PW)
                    {
                        // exising credentials not OK
                        login.PW = "";
                        exists = true;
                        break;
                    }
                    if (dbname != login.Name)
                    {
                        // no such user
                        if (login.ID == -1)
                        {
                            // new user 
                            query = string.Format("insert into logins (name, pw) values ('{0}','{1}')\n\r", login.Name, login.PW);
                            using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, mapConnection))
                            {
                                command.ExecuteNonQuery();
                            }
                            query = string.Format("SELECT Id FROM logins where name = '{0}'\n\r", login.Name);
                            using (SqlDataAdapter loginAdapter2 = new SqlDataAdapter(query, mapConnection))
                            {
                                dataLogins = new DataTable();
                                loginAdapter2.Fill(dataLogins);
                                if (dataLogins.Rows.Count == 1)
                                {
                                    dr = dataLogins.Rows[row];
                                    int dbID = (int)dr["id"];
                                    login.ID = dbID;
                                }
                            }
                            exists = true;
                            break;
                        }
                    }
                }
                if (!exists)
                {
                    login.Name = "";
                    login.PW = "";
                }
            }
            log.Result = login.Name;
            log.Save(mapConnection);
            mapConnection.Close();
            return login;

        }
        //public IEnumerable<Location> GetLocationsGet()
        //{
        //    return GetLocations(1);
        //}

        // these needed to get round OPTIONS bug in IIS returning 405 error
        public Login LoginOptions(Login login)
        {
            return null;
        }
        public IEnumerable<Location> GetLocationsOptions(int userID)
        {
            return null;
        }
        public IEnumerable<Location> GetLocations(int userID)
        {
            int howMany = 300;
            if (userID < 0)
            {
                //just updating map, only get a few
                userID = -userID;
                howMany = 5;
            }
            string query = string.Format("SELECT TOP {0} lat, lon, dt, id FROM locations  where owner = {1} order by id desc", howMany, userID);

            LogEntry log = new LogEntry(getIP(), "GetLocations", null);

            locations = new List<Location>();
            try
            {
                mapConnection = new SqlConnection(connection);
                mapConnection.Open();
            }
            catch (Exception ex)
            {

                Trace.WriteLine(ex.Message);
                log.Result = ex.Message;
                log.Save(mapConnection);
                mapConnection.Close();
                return null;

            }

            // get latest 'howmany' entries
            using (SqlDataAdapter loginAdapter = new SqlDataAdapter(query, mapConnection))
            {
                dataLogins = new DataTable();
                loginAdapter.Fill(dataLogins);
                int length = dataLogins.Rows.Count;
                for (int row = 0; row < length; row++)
                {
                    DataRow dr = dataLogins.Rows[row];
                    double latitude = Convert.ToDouble(dr["lat"]);
                    double longitude = Convert.ToDouble(dr["lon"]);
                    DateTime time = (DateTime)dr["dt"];
                    //Int32 owner = 0;
                    //if (dr["owner"] != DBNull.Value)
                    //    owner = Convert.ToInt32(dr["owner"]);
                    locations.Add(new Location(latitude, longitude, time.ToString(), userID));
                };
            }
            log.Result = locations.Count.ToString() + " locations";
            log.Save(mapConnection);
            mapConnection.Close();
            return locations;
        }

        /// <summary>
        /// Get all saved data  between specified times.
        /// Times are passed as 'smalldatetime' i.e. number of minutes since 01.01.1970
        /// </summary>
        /// <param name="from">start of data reqd</param>
        /// <param name="to">end of data reqd</param>
        /// <returns></returns>
        //public IEnumerable<WeatherData> GetWeather(DataRequest req)
        public IEnumerable<WeatherData> GetWeather(int start)
        {
            DataRequest req = new DataRequest(start, start + 10000);
            LogEntry log = new LogEntry(getIP(), "GetWeather", req.ToString());

            string result = "";
            weatherdata = new List<WeatherData>();

            try
            {
                mapConnection = new SqlConnection(connection);
                mapConnection.Open();
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Trace.WriteLine(ex.Message);
                log.Result = ex.Message;
                log.Save(mapConnection);
                mapConnection.Close();
                return null;

            }


            string query = string.Format("SELECT * FROM weather  WHERE weather.time >= '{0}' and weather.time <= '{1}' ORDER BY weather.time",
                    req.From, req.To);
            using (SqlDataAdapter loginAdapter = new SqlDataAdapter(query, mapConnection))
            {
                dataLogins = new DataTable();
                loginAdapter.Fill(dataLogins);
                int length = dataLogins.Rows.Count;
                for (int row = 0; row < length; row++)
                {
                    DataRow dr = dataLogins.Rows[row];
                    int indoorT = Convert.ToInt32(dr["indoorT"]);
                    int shadeT = Convert.ToInt32(dr["shadeT"]);
                    int hum = Convert.ToInt32(dr["hum"]);
                    int rain = Convert.ToInt32(dr["rainperhour"]);
                    int winddir = Convert.ToInt32(dr["winddir"]);
                    int windspeed = Convert.ToInt32(dr["windspeed"]);
                    DateTime time = (DateTime)dr["time"];
                    weatherdata.Add(new WeatherData(shadeT, indoorT, hum, rain, winddir, windspeed, time.ToString()));
                };
            }
            result = log.Result = locations.Count.ToString() + " weather records";
            log.Save(mapConnection);
            mapConnection.Close();
            return weatherdata;
            //return result;
        }


        public string SaveLocation(Location loc)
        {
            LogEntry log = new LogEntry(getIP(), "SaveLocation", new JavaScriptSerializer().Serialize(loc));
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

            //// First check to see if this user has asked for a new password
            //string query = string.Format("SELECT clubID FROM logins where Id = '{0}'\n\r", loc.Owner);
            //using (SqlDataAdapter loginAdapter2 = new SqlDataAdapter(query, mapConnection))
            //{
            //    dataLogins = new DataTable();
            //    loginAdapter2.Fill(dataLogins);
            //    if (dataLogins.Rows.Count == 1)
            //    {
            //        DataRow dr = dataLogins.Rows[0];
            //        int pwNeeded = (int)dr["clubID"];
            //        if (pwNeeded > 0)
            //        {
            //            query = string.Format("update logins set clubID = 0 where Id = '{0}'\n\r", loc.Owner);
            //            using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, mapConnection))
            //            {
            //                command.ExecuteNonQuery();
            //            }
            //            return "NewPasswordRequired";

            //        }
            //    }
            //}

            // Only save a new location if it is different enough from pevious ones, 
            //  except that the last two locations always stored so that we can see how long we have been stopped for

            int successRows = 0;
            string query = string.Format("SELECT TOP 2 lat, lon, dt, id FROM locations  where owner = {0}  order by id desc", loc.Owner);
            string result = "";


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
                    //DateTime time1 = (DateTime)dr["dt"];
                    int id = (int)dr["id"];

                    dr = dataLogins.Rows[1];
                    double latitude2 = Convert.ToDouble(dr["lat"]);
                    double longitude2 = Convert.ToDouble(dr["lon"]);
                    //DateTime time2 = (DateTime)dr["dt"];


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
                                    result = string.Format("Time updated for user {0}", loc.Owner);
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
                //string T = TimeString(loc.Time);

                query = string.Format("insert into locations (lat,lon,dt,owner) values ('{0}','{1}','{2}',{3})",
                        loc.Latitude, loc.Longitude, loc.Time, loc.Owner);


                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, mapConnection))
                {
                    successRows = command.ExecuteNonQuery();
                }
                if (successRows == 1)
                    result = string.Format("Location {0} {1} saved OK for user {2}", loc.Latitude, loc.Longitude, loc.Owner);
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

        public string SaveWeather(WeatherData w)
        {
            string result = "";
            int successRows = 0;
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
            LogEntry log = new LogEntry(getIP(), "SaveWeather", new JavaScriptSerializer().Serialize(w));
            try
            {
                string T = TimeString(DateTime.Now);

                string query = string.Format("insert into weather (shadeT,indoorT,hum,rainperhour,winddir,windspeed,time) values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
                       (int)(w.OutdoorTemp * 10 + 0.5),
                       (int)(w.IndoorTemp * 10 + 0.5),
                       w.Humidity, w.Rainfall, w.Direction, w.Speed, T);


                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, mapConnection))
                {
                    successRows = command.ExecuteNonQuery();
                }
                if (successRows == 1)
                    result = string.Format("Weather saved OK at {0} ", DateTime.Now);
                else
                    result = string.Format("Database error: Weather not saved");

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
        public string SaveCamper(CamperData w)
        {
            string result = "";
            int successRows = 0;
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
            LogEntry log = new LogEntry(getIP(), "SaveCamper", new JavaScriptSerializer().Serialize(w));
            try
            {
                DateTime recordTime;
                if (w.Sequence < 1000000) { 
                    // datalogger doesn't know real time; if sequence nuber is > 0 then calculate actual time of records
                    int missingSeconds = w.Sequence * w.Period;
                    recordTime = DateTime.Now.AddSeconds(-missingSeconds);
                }
                else
                {
                    // w.sequence has realtime in minutes from 01.01.1970
                    double minutes = (double)w.Sequence;
                    recordTime = new DateTime(1970, 1, 1);
                    recordTime = recordTime.AddMinutes(minutes);
                    if (recordTime.IsDaylightSavingTime())
                    {
                        recordTime = recordTime.AddMinutes(60);

                    }
                }
                string T = TimeString(recordTime);
 
                string query = string.Format("insert into camper (shadeT,vanT,fridgeT,battV,panelV,panelP,loadC,yield,maxP,time) values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')",
                       w.ShadeTemp,
                       w.VanTemp,
                       w.FridgeTemp,
                       w.BatteryVolts,
                       w.PanelVolts,
                       w.PanelPower,
                       w.LoadCurrent,
                       w.YieldToday,
                       w.MaxPowerToday,
                       T);


                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, mapConnection))
                {
                    successRows = command.ExecuteNonQuery();
                }
                if (successRows == 1)
                    result = string.Format("Data saved OK at {0} ", DateTime.Now);
                else
                    result = string.Format("Database error: camper not saved");

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

        public IEnumerable<CamperData> GetCamper(int howmany)
        {
 //           DataRequest req = new DataRequest(start, start + 10000);
            LogEntry log = new LogEntry(getIP(), "GetCamper", howmany.ToString());

            string result = "";
            camperdata = new List<CamperData>();

            try
            {
                mapConnection = new SqlConnection(connection);
                mapConnection.Open();
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Trace.WriteLine(ex.Message);
                log.Result = ex.Message;
                log.Save(mapConnection);
                mapConnection.Close();
                return null;

            }


            //string query = string.Format("SELECT * FROM camper  WHERE camper.time >= '{0}' and camper.time <= '{1}' ORDER BY camper.time",
            //        req.From, req.To);
            string query = string.Format("SELECT TOP {0} * FROM camper ORDER BY camper.time desc",howmany);

            using (SqlDataAdapter loginAdapter = new SqlDataAdapter(query, mapConnection))
            {
                dataLogins = new DataTable();
                loginAdapter.Fill(dataLogins);
                int length = dataLogins.Rows.Count;
                for (int row = 0; row < length; row++)
                {
                    DataRow dr = dataLogins.Rows[row];
                    int vanT = Convert.ToInt32(dr["vanT"]);
                    int shadeT = Convert.ToInt32(dr["shadeT"]);
                    int fridgeT = Convert.ToInt32(dr["fridgeT"]);
                    int battV = Convert.ToInt32(dr["battV"]);
                    int panelV = Convert.ToInt32(dr["panelV"]);
                    int panelP = Convert.ToInt32(dr["panelP"]);
                    int loadC = Convert.ToInt32(dr["loadC"]);
                    int yield = Convert.ToInt32(dr["yield"]);
                    int maxP = Convert.ToInt32(dr["maxP"]);


                    DateTime time = (DateTime)dr["time"];

                    camperdata.Add(new CamperData(vanT, shadeT, fridgeT, battV, panelV, panelP, loadC, yield, maxP, time.ToString("MM/dd/yyyy HH:mm:ss")));

                };
            }
            result = log.Result = camperdata.Count.ToString() + " camper records";
            log.Save(mapConnection);
            mapConnection.Close();
            return camperdata;
        }


    }
}

   
