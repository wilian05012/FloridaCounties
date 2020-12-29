using System;

namespace FloridaCounties.Dto.City {
    public class Cities {
        public string type { get; set; }
        public Feature[] features { get; set; }
    }

    public class Feature {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    //public class Geometry {
    //    public string type { get; set; }
    //    public float[][][] coordinates { get; set; }
    //}

    public class Properties {
        public string PLACEFP { get; set; }
        public int BEBR_ID { get; set; }
        public string NAME { get; set; }
        public string COUNTY { get; set; }
        public int TAX_COUNT { get; set; }
        public string TAXAUTHCD { get; set; }
        public string NOTES { get; set; }
        public float ACRES { get; set; }
        public string DESCRIPT { get; set; }
        public DateTime FGDLAQDATE { get; set; }
        public int AUTOID { get; set; }
        public float SHAPE_AREA { get; set; }
        public float SHAPE_LEN { get; set; }
    }
}

