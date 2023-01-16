using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Phahung_BN.Models
{
    [FirestoreData]
    public class CommentModel
    {
        [FirestoreProperty]
        public string id { get; set; }

        [FirestoreProperty]
        public string createAt { get; set; }

        [FirestoreProperty]
        public string owner { get; set; }

        [FirestoreProperty]
        public int likes { get; set; }

        [FirestoreProperty]
        public string content { get; set; }

        [FirestoreProperty]
        public string comments { get; set; }

        [FirestoreProperty]
        public bool isParent { get; set; }

        [FirestoreProperty]
        public string parentId { get; set; }

        [FirestoreProperty]
        public bool visible { get; set; }

    }
}
