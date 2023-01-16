using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Phahung_BN.Models
{
    [FirestoreData]
    public class ParentCommentModel
    {
        [FirestoreProperty]
        public string id { get; set; }

        [FirestoreProperty]
        public long createAt { get; set; }

        [FirestoreProperty]
        public CommentOwnerModel owner { get; set; }

        [FirestoreProperty]
        public int likes { get; set; }

        [FirestoreProperty]
        public string content { get; set; }

        [FirestoreProperty]
        public List<SubCommentModel> comments { get; set; }

        [FirestoreProperty]
        public bool isParent { get; set; }

        [FirestoreProperty]
        public bool visible { get; set; }

    }
}