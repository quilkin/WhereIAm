﻿using System;
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
                time.Hour.ToString("00"),time.Minute.ToString("00"),time.Second.ToString("00"));
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
            LogEntry log = new LogEntry(getIP(), "Login", login.Name + " " +login.PW);


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

        

        public IEnumerable<Location> GetLocations()
        {

            string query = string.Format("SELECT TOP {0} lat, lon, dt, id, owner FROM locations order by id desc",300);

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
                    Int32 owner = 0;
                    if (dr["owner"] != DBNull.Value)
                        owner = Convert.ToInt32(dr["owner"]);
                    locations.Add(new Location(latitude, longitude, time.ToString(), owner));
                };
            }
            log.Result = locations.Count.ToString() + " locations";
            log.Save(mapConnection);
            mapConnection.Close();
            return locations;
        }


        public string SaveLocation(Location loc)
        {
            LogEntry log = new LogEntry(getIP(), "SaveLocation", new JavaScriptSerializer().Serialize(loc));

            // Only save a new location if it is different enough from pevious ones, 
            //  except that the last two locations always stored so that we can see how long we have been stopped for

            int successRows = 0;
            string query = string.Format("SELECT TOP 2 lat, lon, dt, id FROM locations  where owner = {0}  order by id desc",loc.Owner);
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


                    double diffLat = latitude1 - latitude2;
                    double diffLon = longitude1 - longitude2;
                    double distance = Math.Sqrt(Math.Abs(diffLat * diffLat - diffLon * diffLon));
                    if (distance < 0.001)
                    {
                        // last two entries were same location. See if this is different now.
                        latitude2 = loc.Latitude;
                        longitude2 = loc.Longitude;
                        diffLat = latitude1 - latitude2;
                        diffLon = longitude1 - longitude2;
                        distance = Math.Sqrt(Math.Abs(diffLat * diffLat - diffLon * diffLon));
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
                                    result = string.Format("Time updated for {0} at {2}", loc.Owner, DateTime.Now);
                                else
                                    result = string.Format("Database error: update not saved");
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(ex.Message);
                                log.Error = ex.Message;
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
                        loc.Latitude, loc.Longitude, T,loc.Owner);


                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, mapConnection))
                {
                    successRows = command.ExecuteNonQuery();
                }
                if (successRows == 1)
                    result = string.Format("Location {0} {1} saved OK at {2} for {3}", loc.Latitude, loc.Longitude, DateTime.Now,loc.Owner);
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


    }
}