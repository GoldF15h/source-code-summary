using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
   

namespace Phahung_BN.Models
{   
    [FirestoreData]
    public class UserModel
    {

        [FirestoreProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [FirestoreProperty]
        [Required]
        public string Password { get; set; }

        [FirestoreProperty]
        [Required]
        public string FirstName { get; set; }

        [FirestoreProperty]
        [Required]
        public string LastName { get; set; }

        [FirestoreProperty]
        public roles Role { get; set; }


    }

    
}
