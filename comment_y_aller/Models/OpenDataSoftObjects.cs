using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace comment_y_aller.Models
{
    public class OpenDataSoftParameters
    {
        public List<string> dataset { get; set; }
        public string timezone { get; set; }
        public int rows { get; set; }
        public string format { get; set; }
    }

    public class OpenDataSoftFields
    {
        public double _2_metre_temperature { get; set; }
        public DateTime timestamp { get; set; }
        public double total_water_precipitation { get; set; }
        public double relative_humidity { get; set; }
        public List<double> position { get; set; }
        public DateTime forecast { get; set; }
    }

    public class OpenDataSoftGeometry
    {
        public string type { get; set; }
        public List<double> coordinates { get; set; }
    }

    public class OpenDataSoftRecord
    {
        public string datasetid { get; set; }
        public string recordid { get; set; }
        public OpenDataSoftFields fields { get; set; }
        public OpenDataSoftGeometry geometry { get; set; }
        public DateTime record_timestamp { get; set; }
    }

    public class OpenDataSoftRootObject
    {
        public int nhits { get; set; }
        public OpenDataSoftParameters parameters { get; set; }
        public List<OpenDataSoftRecord> records { get; set; }
    }
}
