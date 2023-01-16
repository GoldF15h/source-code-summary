using Google.Cloud.Firestore;
using System;

namespace Phahung_BN.Models
{
    [FirestoreData]
    public class AnnouncementModel
    {
        [FirestoreProperty]
        public string id { get; set; }

        [FirestoreProperty]
        public string title { get; set; }

        [FirestoreProperty]
        public string description { get; set; }

        [FirestoreProperty]
        public long createdAt { get; set; }

    }
}
