using System;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
//using Newtonsoft.Json;
//using Phahung_BN.Models;


namespace Phahung_BN
{
    public class FiresbaseConnect
    {
        private string path;
        private string projID;
        private FirestoreDb _firestoreDb;

        public FiresbaseConnect()
        {
            path = @"./phahung-db-74c6652232b9.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            projID = "phahung-db";
            _firestoreDb = FirestoreDb.Create(projID);
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.GetApplicationDefault(),
                ProjectId = "phahung-db",
            });

            Console.WriteLine("******************************************************************** DB CONNECTED ...");
        }

        public FirestoreDb get_FirestoreDb ()
        {
            return _firestoreDb;
        }
    }
}
