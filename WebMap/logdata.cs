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

    [DataContract]
    public class Logdata
    {

        public static DateTime JSTimeToNetTime(long time)
        {
            DateTime t = new DateTime(1970, 1, 1);
            return t.AddMilliseconds(time);
        }
        /// <summary>
        /// keeping member names small to keep json stringifications smaller
        /// </summary>
        [DataMember]
        public string S { get; set; }
        [DataMember]
        public int T { get; set; }
        [DataMember]
        public List<float> V { get; set; }

        public int ID { get; set; }

        public Logdata()
        {
            S = string.Empty;
            T = 0;
            V = new List<float>();
        }
        public Logdata(string id, DateTime dt, List<float> vals)
        {
            S = id;
            T = (int)TimeSpan.FromTicks(dt.Ticks).TotalMinutes;
            V = vals;
        }
        public Logdata(int id, DateTime dt, List<float> vals)
        {
            ID = id;
            T = (int)TimeSpan.FromTicks(dt.Ticks).TotalMinutes;
            V = vals;
        }
        public Logdata(string id, int totalmins, List<float> vals)
        {
            S = id;
            T = totalmins;
            V= vals;
        }
        public Logdata(int totalmins, List<float> vals)
        {
            T = totalmins;
            V = vals;
        }
        public Logdata(int id, int totalmins, List<float> vals)
        {
            ID = id;
            T = totalmins;
            V = vals;
        }

    }

    [DataContract]
    public class DataRequest
    {
        [DataMember]
        public List<int> IDlist { get; set; }
        [DataMember]
        public int From{ get; set; }
        [DataMember]
        public int To { get; set; }

        public DataRequest()
        {
            IDlist = new List<int>(); 
            From = 0;
            To =  0;
        }
        public DataRequest(List<int> idlist, int from, int to)
        {
            IDlist = idlist;
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
        public void Save(SqlConnection conn)
        {
            //if (this.Error != null)
            //    this.Error = this.Error.Substring(0, 125);
            //if (this.Result != null)
            //    this.Result = this.Result.Substring(0, 125);
            string query = string.Format("insert into log (time,ip,func,args,result,error) values ('{0}','{1}','{2}','{3}','{4}','{5}')",
                WebMap.TimeString(DateTime.Now), this.IP, this.Function, this.Args,this.Result,this.Error);

            using (System.Data.SqlClient.SqlCommand command = new SqlCommand(query, conn))
            {
                command.ExecuteNonQuery();
            }

        }
    }
}