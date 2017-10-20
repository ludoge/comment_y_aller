using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace comment_y_aller.Models
{
    public class ParisRootObject
    {
        public int nhits { get; set; }
        public Parameters parameters { get; set; }
        public List<Record> records { get; set; }
    }

    public class Parameters
    {
        public List<string> dataset { get; set; }
        public string timezone { get; set; }
        public int rows { get; set; }
        public string format { get; set; }
    }

    public class Fields
    {
        public string status { get; set; }
        public string contract_name { get; set; }
        public string name { get; set; }
        public string bonus { get; set; }
        public int bike_stands { get; set; }
        public int number { get; set; }
        public DateTime last_update { get; set; }
        public int available_bike_stands { get; set; }
        public string banking { get; set; }
        public int available_bikes { get; set; }
        public string address { get; set; }
        public List<double> position { get; set; }
        //public string status { get; set; }
        public string city { get; set; }
        public string kind { get; set; }
        public string station_type { get; set; }
        public string charging_status { get; set; }
        public string rental_status { get; set; }
        public int cars_counter_bluecar { get; set; }
        public int cars { get; set; }
        public string public_name { get; set; }
        public List<double> geo_point { get; set; }
        public int charge_slots { get; set; }
        public string postal_code { get; set; }
        //public int __invalid_name__cars_counter_utilib_1.4 { get; set; }
        public string subscription_status { get; set; }
        public int slots { get; set; }
        public string id { get; set; }
        //public string address { get; set; }
        public int cars_counter_utilib { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public List<double> coordinates { get; set; }
    }

    public class Record
    {
        public string datasetid { get; set; }
        public string recordid { get; set; }
        public Fields fields { get; set; }
        public Geometry geometry { get; set; }

        public Record()
        {

        }

        public Record(double lat, double lon)
        {
            fields = new Fields();

            fields.position = new List<double>();
            fields.geo_point = new List<double>();

            fields.position.Add(lat);
            fields.geo_point.Add(lat);

            fields.position.Add(lon);
            fields.geo_point.Add(lon);
        }
        public Record(decimal lat, decimal lon)
        {
            fields = new Fields();

            fields.position = new List<double>();
            fields.geo_point = new List<double>();

            fields.position.Add((double)lat);
            fields.geo_point.Add((double)lat);

            fields.position.Add((double)lon);
            fields.geo_point.Add((double)lon);
        }
    }
}
