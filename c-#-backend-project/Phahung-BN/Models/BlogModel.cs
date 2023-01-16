using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Phahung_BN.Models
{
    [FirestoreData]
    public class BlogModel
    {

        public BlogModel()
        {
            createAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        [FirestoreProperty]
        public string id { get; set; }

        [FirestoreProperty]
        public string title { get; set; }

        [FirestoreProperty]
        public Object content { get; set; }

        [FirestoreProperty]
        public string contentX { get; set; }

        [FirestoreProperty]
        public string author { get; set; }

        [FirestoreProperty]
        public ulong likes { get; set; }

        [FirestoreProperty]
        public long createAt { get; set; }

        [FirestoreProperty]
        public string image { get; set; }

        [FirestoreProperty]
        public string status { get; set; }

        [FirestoreProperty]
        public string[] tags { get; set; }

        public string toString()
        {
            return "Create blog!!!";
        }
    }
    
}
