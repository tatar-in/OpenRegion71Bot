using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenRegion71Bot
{
    class Deserialize
    {
        public class Problem
        {
            public class Photo
            {
                public string SMALL { get; set; }
                public string BIG { get; set; }
            }

            public class QUEST
            {
                public string DATE { get; set; }
                public string TEXT { get; set; }
                public string FIO { get; set; }
                public List<Photo> PHOTOS { get; set; }
            }

            public class ANSWER
            {
                public string DATE { get; set; }
                public string TEXT { get; set; }
                public string FIO { get; set; }
                public string STATUS_ID { get; set; }
                public string STATUS { get; set; }
                public string USER_AGREE { get; set; }
                public List<Photo> PHOTOS { get; set; }
            }

            public class RESULT
            {
                public string CATEGORY_NAME { get; set; }
                public string THEME_NAME { get; set; }
                public string ID { get; set; }
                public string CATEGORY { get; set; }
                public string THEME { get; set; }
                public string ADDRESS { get; set; }
                public List<string> COORDINATES { get; set; }
                public string SOURCE_ID { get; set; }
                public string SOURCE { get; set; }
                public QUEST QUEST { get; set; }
                public string CHILD_ISSUE { get; set; }
                public string FRAYON { get; set; }
                public string FCITY { get; set; }
                public string FSTREET { get; set; }
                public string FHOUSE { get; set; }
                public string FIAS { get; set; }
                public string FIAS_TYPE { get; set; }
                public string IS_REWORK { get; set; }
                public ANSWER ANSWER { get; set; }
                public string PARENT_ISSUE { get; set; }
            }
            public List<RESULT> RESULTS { get; set; }
        }
        public class FRayon
        {
            public class RESULT
            {
                public string ID { get; set; }
                public string NAME { get; set; }
            }
            public List<RESULT> RESULTS { get; set; }
        }
        public class Theme
        {
            public class CHILD2
            {
                public string ID { get; set; }
                public string NAME { get; set; }
                public string SORT { get; set; }
                public List<string> PROPERTY_DOPFIELDS_PROPERTY_VALUE_ID { get; set; }

                [JsonPropertyName("~PROPERTY_DOPFIELDS_PROPERTY_VALUE_ID")]
                public List<string> PROPERTYDOPFIELDSPROPERTYVALUEID { get; set; }
                public string ADRESS_NEED { get; set; }
                public string PHOTO_NEED { get; set; }
                public object FIELDS { get; set; }
            }
            public class CHILD
            {
                public string ID { get; set; }
                public string NAME { get; set; }
                public string LEFT_MARGIN { get; set; }

                [JsonPropertyName("~LEFT_MARGIN")]
                public string LEFTMARGIN { get; set; }
                public object UF_SVG { get; set; }

                [JsonPropertyName("~UF_SVG")]
                public object UFSVG { get; set; }
                public string UF_FLAT_COLOR { get; set; }

                [JsonPropertyName("~UF_FLAT_COLOR")]
                public string UFFLATCOLOR { get; set; }
                public string UF_FLAT_NAME { get; set; }

                [JsonPropertyName("~UF_FLAT_NAME")]
                public string UFFLATNAME { get; set; }
                public string UF_FLAT_SORT { get; set; }

                [JsonPropertyName("~UF_FLAT_SORT")]
                public string UFFLATSORT { get; set; }
                public List<string> UF_SOURCE { get; set; }

                [JsonPropertyName("~UF_SOURCE")]
                public List<int> UFSOURCE { get; set; }
                public object UF_POS_ID { get; set; }

                [JsonPropertyName("~UF_POS_ID")]
                public object UFPOSID { get; set; }
                public List<CHILD> CHILDS { get; set; }
            }
            public class RESULT
            {
                public string ID { get; set; }
                public string NAME { get; set; }
                public string PICTURE { get; set; }
                public string LEFT_MARGIN { get; set; }

                [JsonPropertyName("~LEFT_MARGIN")]
                public string LEFTMARGIN { get; set; }
                public string UF_COLOR { get; set; }
                public string UF_SVG { get; set; }

                [JsonPropertyName("~UF_SVG")]
                public string UFSVG { get; set; }
                public string UF_FLAT_COLOR { get; set; }

                [JsonPropertyName("~UF_FLAT_COLOR")]
                public string UFFLATCOLOR { get; set; }
                public string UF_FLAT_NAME { get; set; }

                [JsonPropertyName("~UF_FLAT_NAME")]
                public string UFFLATNAME { get; set; }
                public string UF_FLAT_SORT { get; set; }

                [JsonPropertyName("~UF_FLAT_SORT")]
                public string UFFLATSORT { get; set; }
                public List<string> UF_SOURCE { get; set; }

                [JsonPropertyName("~UF_SOURCE")]
                public List<int> UFSOURCE { get; set; }
                public object UF_POS_ID { get; set; }

                [JsonPropertyName("~UF_POS_ID")]
                public object UFPOSID { get; set; }
                public string SVG_PATH { get; set; }
                public List<CHILD> CHILDS { get; set; }
            }
            public List<RESULT> RESULTS { get; set; }
        }
    }
}
