using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Phahung_BN.Models
{
    [FirestoreData]
    public class BlogPreviewModel
    {
        [FirestoreProperty]
        public string id { get; set; }

        [FirestoreProperty]
        public string title { get; set; }

        [FirestoreProperty]
        public string author { get; set; }

        [FirestoreProperty]
        public ulong likes { get; set; }

        [FirestoreProperty]
        public ulong createAt { get; set; }

        [FirestoreProperty]
        public string image { get; set; }

        [FirestoreProperty]
        public string[] tags { get; set; }

        [FirestoreProperty]
        public string status { get; set; }
    }
}
