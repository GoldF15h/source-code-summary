using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Phahung_BN.Models
{
    [FirestoreData]
    public class PhahungModel
    {
        public string usersId { get; set; }

        [FirestoreProperty]
        public string age { get; set; }    

        [FirestoreProperty]
        public string firstname { get; set; }


        [FirestoreProperty]
        public string lastname { get; set; }


        [FirestoreProperty]
        public string nickname { get; set; }


    }
}
