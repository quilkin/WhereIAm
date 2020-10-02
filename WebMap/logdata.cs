using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Data.SqlClient;

namespace WebMap
{
    [DataContract]
    public class Login
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "pw")]
        public string PW { get; set; }
        [DataMember(Name = "email")]
        public string Email { get; set; }
        [DataMember(Name = "code")]
        public int Code { get; set; }
        [DataMember(Name = "id")]
        public int ID { get; set; }
        [DataMember(Name = "role")]
        public int Role { get; set; }

        public Login(string name, string pw)
        {
            Name = name;
            PW = pw;
        }
        public Login(string name, string pw, string email)
        {
            Name = name;
            PW = pw;
            Email = email;
        }
        public int CalcCode()
        {
            return (Name.Length + PW.Length) * 417 + Email.Length;
        }
    }

    //[DataContract]
    //public class WeatherData
    //{

    //    public static DateTime JSTimeToNetTime(long time)
    //    {
    //        DateTime t = new DateTime(1970, 1, 1);
    //        return t.AddMilliseconds(time);
    //    }
    //    /// <summary>
    //    /// keeping member names small to keep json stringifications smaller
    //    /// </summary>
    //    [DataMember]
    //    public string S { get; set; }
    //    [DataMember]
    //    public int T { get; set; }
    //    [DataMember]
    //    public int  V { get; set; }


    //    public WeatherData()
    //    {
    //        S = string.Empty;
    //        T = 0;
    //        V = 0;
    //    }
    //    public WeatherData(string id, DateTime dt, List<float> vals)
    //    {
    //        S = id;
    //        T = (int)TimeSpan.FromTicks(dt.Ticks).TotalMinutes;
    //        V = vals;
    //    }

    //}

    [DataContract]
    public class DataRequest
    {
        //[DataMember]
        //public List<int> IDlist { get; set; }
        [DataMember]
        public int From{ get; set; }
        [DataMember]
        public int To { get; set; }

        public DataRequest()
        {
            //IDlist = new List<int>(); 
            From = 0;
            To =  0;
        }
        public DataRequest( int from, int to)
        {
           // IDlist = idlist;
            From = from;
            To = to;
        }
    }

    [DataContract]
    public class UploadResult
    {
        [DataMember]
        public int Overlaps { get; set; }
        [DataMember]
        public int Saved { get; set; }

        public UploadResult()
        {
            Overlaps = 0;
            Saved = 0;
        }
        public UploadResult(int sv, int ov)
        {
            Overlaps = ov;
            Saved = sv;
        }
    }

    [DataContract]
    public class Sensor
    {

        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Serial { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int Owner { get; set; }
        [DataMember]
        public int Period { get; set; }
        [DataMember]
        public int AlarmLow { get; set; }
        [DataMember]
        public int AlarmHigh { get; set; }

        public Sensor()
        {
            ID = 0;
            Serial = string.Empty;
            Owner = 0;
        }
        public Sensor(int id,string serial, string name, string description,int alarmlow,int alarmhigh,int period, int owner)
        {
            ID = id;
            Serial = serial;
            Name = name;
            Description = description;
            AlarmLow = alarmlow;
            AlarmHigh = alarmhigh;
            Period = period;
            Owner = owner;
        }
     

    }


    [DataContract]
    public class Location
    {
        [DataMember(Name = "latitude")]
        public double Latitude { get; set; }
        [DataMember(Name = "longitude")]
        public double Longitude { get; set; }
        [DataMember(Name = "speed")]
        public double Speed { get; set; }
        [DataMember(Name = "bearing")]
        public double Bearing { get; set; }
        [DataMember(Name = "altitude")]
        public double Altitude { get; set; }
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

    [DataContract]
    public class WeatherData
    {
        [DataMember(Name = "indoorT")]
        public double IndoorTemp { get; set; }
        [DataMember(Name = "shadeT")]
        public double OutdoorTemp { get; set; }
        [DataMember(Name = "hum")]
        public int Humidity { get; set; }
        [DataMember(Name = "rain")]
        public int Rainfall { get; set; }
        [DataMember(Name = "winddir")]
        public int Direction { get; set; }
        [DataMember(Name = "windspeed")]
        public int Speed { get; set; }
        [DataMember(Name = "recorded_at")]
        public String Time { get; set; }
        [DataMember(Name = "owner")]
        public int Owner { get; set; }

        public static DateTime JSTimeToNetTime(long time)
        {
            DateTime t = new DateTime(1970, 1, 1);
            return t.AddMilliseconds(time);
        }
        public WeatherData(double shadeT, double indoorT, int hum, int rain, int dir, int speed, String t)
        {
            IndoorTemp = indoorT;
            OutdoorTemp = shadeT;
            Humidity = hum;
            Rainfall = rain;
            Direction = dir;
            Speed = speed;
            Time = t;
        }

    }

    [DataContract]
    public class CamperData
    {
        [DataMember(Name = "vanT")]
        public int VanTemp { get; set; }
        [DataMember(Name = "shadeT")]
        public int ShadeTemp { get; set; }
        [DataMember(Name = "fridgeT")]
        public int FridgeTemp { get; set; }
        [DataMember(Name = "battV")]
        public int BatteryVolts { get; set; }
        [DataMember(Name = "panelV")]
        public int PanelVolts { get; set; }
        [DataMember(Name = "panelP")]
        public int PanelPower { get; set; }
        [DataMember(Name = "loadC")]
        public int LoadCurrent { get; set; }
        [DataMember(Name = "yield")]
        public int YieldToday { get; set; }
        [DataMember(Name = "maxP")]
        public int MaxPowerToday { get; set; }


        [DataMember(Name = "seq")]
        public int Sequence { get; set; }
        [DataMember(Name = "period")]
        public int Period { get; set; }
        [DataMember(Name = "time")]
        public String Time { get; set; }
        [DataMember(Name = "owner")]
        public int Owner { get; set; }

        public static DateTime JSTimeToNetTime(long time)
        {
            DateTime t = new DateTime(1970, 1, 1);
            return t.AddMilliseconds(time);
        }
        public CamperData(int vanT, int shadeT, int fridgeT, int battV, int panelV, int panelP, int loadC, int yield, int maxP, int seq)
        {
            VanTemp = vanT;
            ShadeTemp = shadeT;
            FridgeTemp = fridgeT;
            BatteryVolts = battV;
            PanelVolts = panelP;
            PanelPower = panelP;
            LoadCurrent = loadC;
            YieldToday = yield;
            MaxPowerToday = maxP;
            Sequence = seq;
        }
        public CamperData(int vanT, int shadeT, int fridgeT, int battV, int panelV, int panelP, int loadC, int yield, int maxP, string time)
        {
            VanTemp = vanT;
            ShadeTemp = shadeT;
            FridgeTemp = fridgeT;
            BatteryVolts = battV;
            PanelVolts = panelV;
            PanelPower = panelP;
            LoadCurrent = loadC;
            YieldToday = yield;
            MaxPowerToday = maxP;
            Time = time;
        }

    }

    public class LogEntry
    {
        public string IP { get; set; }
        public string Function { get; set; }
        public string Args { get; set; }
        public string Error { get; set; }
        public string Result { get; set; }

        public LogEntry(string ip,string func,string args)
        {
            IP = ip;
            Args = args;
            Function = func;

        }
        public LogEntry()
        {
        }
        public string Save(SqlConnection conn)
        {
            try
            {
                string query;
                if (Error != null && Error.Length > 2)
                {
                    query = string.Format("insert into log (time,ip,func,args,result,error) values ('{0}','{1}','{2}','{3}','{4}','{5}')",
                        WebMap.TimeString(DateTime.Now), this.IP, this.Function, this.Args, this.Result, this.Error);
                }
                else
                {
                    query = string.Format("insert into log (time,ip,func,result) values ('{0}','{1}','{2}','{3}')",
                        WebMap.TimeString(DateTime.Now), this.IP, this.Function, this.Result);
                }

                using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }
    }
}