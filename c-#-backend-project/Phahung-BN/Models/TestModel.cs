using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Phahung_BN.Models
{
    [FirestoreData]
    public class TestModel
    {

        [FirestoreProperty]
        public Object content { get; set; } 


    }
    public class inblock
    {
        [JsonProperty]
        public string id { get; set; }

        [JsonProperty]
        public string type { get; set; }

        [JsonProperty]
        public indata data { get; set; }

       
    }

    public class indata
    {
        [JsonProperty]
        public string text { get; set; }

        [JsonProperty]
        public string level { get; set; }

        [JsonProperty]
        public string file { get; set; }

        [JsonProperty]
        public string caption { get; set; }

        [JsonProperty]
        public string withBorder { get; set; }

        [JsonProperty]
        public string stretched { get; set; }

        [JsonProperty]
        public string withBackground { get; set; }
    }
   
}
