using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;


namespace Phahung_BN.Models
{
    [FirestoreData]
    public class CommentOwnerModel
    {

        [FirestoreProperty]
        [Required]
        public string UID { get; set; }

        [FirestoreProperty]
        [Required]
        public string FirstName { get; set; }

        [FirestoreProperty]
        [Required]
        public string LastName { get; set; }

        [FirestoreProperty]
        public string picture { get; set; }    

        [FirestoreProperty]
        [Required]
        public bool IsBan { get; set; }

    }


}
