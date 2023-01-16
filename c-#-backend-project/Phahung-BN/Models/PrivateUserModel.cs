using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;


namespace Phahung_BN.Models
{
    [FirestoreData]
    public class PrivateUserModel
    {

        [FirestoreProperty]
        [Required]
        public string UID { get; set; }

        [FirestoreProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [FirestoreProperty]
        [Required]
        public string FirstName { get; set; }

        [FirestoreProperty]
        [Required]
        public string LastName { get; set; }

        [FirestoreProperty]
        [Required]
        public bool IsBan { get; set; }
        
        
        [FirestoreProperty]
        [Required]
        public roles role { get; set; }

        [FirestoreProperty]
        public string picture { get; set; }    


        [FirestoreProperty]
        public List<string> likedBlogs { get; set; }

        [FirestoreProperty]
        public List<string> likedComments { get; set; }   
    }

    public enum roles
    {
        admin,
        user
    }



}
