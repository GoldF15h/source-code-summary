using Google.Cloud.Firestore;

namespace Phahung_BN.Models
{
    [FirestoreData]
    public class CategoryModel
    {
        [FirestoreProperty]
        public string id { get; set; }

        [FirestoreProperty]
        public string name { get; set; }

        [FirestoreProperty]
        public bool status { get; set; }

    }

}
