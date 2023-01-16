using Phahung_BN.Models;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Phahung_BN.Services
{
    public class UserService : IUserService
    {
        private FirestoreDb _firestoreDb;

        public UserService(FiresbaseConnect opt)
        {
            //string path = @".\\phahung-db-74c6652232b9.json";
            //System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            //FirebaseApp.Create(new AppOptions()
            //{
            //    Credential = GoogleCredential.GetApplicationDefault(),
            //    ProjectId = "phahung-db",
            //});
            _firestoreDb = opt.get_FirestoreDb();
        }

        public async Task<PrivateUserModel> GetUserInfo(string uid)
        {
            PrivateUserModel result = new PrivateUserModel();
            CollectionReference collectionReference = _firestoreDb.Collection("users");
            Query query = collectionReference.WhereEqualTo("UID", uid);
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            {
                DocumentReference documentReference = _firestoreDb.Collection("users").Document(documentSnapshot.Id);

                DocumentSnapshot docSnapshot = await documentReference.GetSnapshotAsync();

                result = docSnapshot.ConvertTo<PrivateUserModel>();
            }

            return result;
        }
    }
}