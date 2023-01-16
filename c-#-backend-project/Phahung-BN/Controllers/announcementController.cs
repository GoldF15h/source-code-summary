using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Phahung_BN.Models;
using Google.Cloud.Firestore;
using Phahung_BN.Middleware;
using Newtonsoft.Json;
using System.Web;
using System.Text.Json;

namespace Phahung_BN.Controllers
{
    [Route("")]
    public class announcementController : Controller
    {

        private FirestoreDb _firestoreDb;
        public announcementController(FiresbaseConnect opt)
        {
            _firestoreDb = opt.get_FirestoreDb();
        }

        //[HttpGet("announcement/all")]
        //public async Task<List<AnnouncementModel>> getAnnouncements(int page, int perpage)
        //{
        //    Query allann_Query = _firestoreDb.Collection("announcements").OrderByDescending("createdAtforsorting");
        //    QuerySnapshot allann_QuerySnapshot = await allann_Query.GetSnapshotAsync();
        //    List<AnnouncementModel> allAnn = new List<AnnouncementModel>();
        //    List<AnnouncementModel> segmentedAnn = new List<AnnouncementModel>();
        //    foreach (DocumentSnapshot documentSnapshot in allann_QuerySnapshot.Documents)
        //    {
        //        if (documentSnapshot.Exists)
        //        {
        //            AnnouncementModel ann = documentSnapshot.ConvertTo<AnnouncementModel>();
        //            allAnn.Add(ann);
        //        }
        //    }


        //    if (page != 0 && perpage != 0)
        //    {
        //        int i = (page - 1) * perpage;
        //        while (i < allAnn.Count && i < (page - 1) * perpage + perpage)
        //        {
        //            segmentedAnn.Add(allAnn[i]);
        //            i++;
        //        }
        //        return segmentedAnn;
        //    }
        //    return allAnn;

        //}

        [HttpGet("announcement/all")]
        [Authorize(roles.admin, roles.user)]
        public async Task<List<AnnouncementModel>> getAnnouncements(int page, int perpage)
        {
            Query allann_Query = _firestoreDb.Collection("announcements").OrderByDescending("createdAtforsorting");
            QuerySnapshot allann_QuerySnapshot = await allann_Query.GetSnapshotAsync();
            List<AnnouncementModel> allAnn = new List<AnnouncementModel>();
            List<AnnouncementModel> segmentedAnn = new List<AnnouncementModel>();
            foreach (DocumentSnapshot documentSnapshot in allann_QuerySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    AnnouncementModel ann = documentSnapshot.ConvertTo<AnnouncementModel>();
                    allAnn.Add(ann);
                }
            }


            if (page != 0 && perpage != 0)
            {
                int i = (page - 1) * perpage;
                while (i < allAnn.Count && i < (page - 1) * perpage + perpage)
                {
                    segmentedAnn.Add(allAnn[i]);
                    i++;
                }
                return segmentedAnn;
            }
            return allAnn;

        }

        [HttpGet("announcement/{id}")]
        [Authorize(roles.admin, roles.user)]
        public async Task<AnnouncementModel> getAnnouncementById(string id)
        {
            DocumentReference documentReference = _firestoreDb.Collection("announcements").Document(id);
            DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
            AnnouncementModel ann = documentSnapshot.ConvertTo<AnnouncementModel>();
            return ann;

        }

        [HttpPost("announcement/new")]
        [Authorize(roles.admin, roles.user)]
        public async Task<StoredAnnouncementModel> createAnnouncement([FromBody] StoredAnnouncementModel ann)
        {

            CollectionReference collectionReference = _firestoreDb.Collection("announcements");
            DocumentReference documentReference = await collectionReference.AddAsync(ann);
            ann.id = documentReference.Id;
            ann.createdAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            ann.createdAtforsorting = Convert.ToString(ann.createdAt);
            await documentReference.SetAsync(ann, SetOptions.MergeAll);
            return ann;

        }

        [HttpPut("announcement/{id}/update")]
        [Authorize(roles.admin, roles.user)]
        public async Task<StoredAnnouncementModel> updateAnnouncement(string id, [FromBody] StoredAnnouncementModel ann)
        {
            DocumentReference documentReference = _firestoreDb.Collection("announcements").Document(id);
            string[] mergeFields = { "title", "description","createdAt","createdAtforsorting"};
            ann.createdAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            ann.createdAtforsorting = Convert.ToString(ann.createdAt);
            await documentReference.SetAsync(ann, SetOptions.MergeFields(mergeFields));
            DocumentSnapshot updatedAnn = await documentReference.GetSnapshotAsync();
            return updatedAnn.ConvertTo<StoredAnnouncementModel>();

        }

        [HttpDelete("announcement/{id}")]
        [Authorize(roles.admin, roles.user)]
        public async Task<IActionResult> deleteAnnouncement(string id)
        {
            DocumentReference documentReference = _firestoreDb.Collection("announcements").Document(id);
            await documentReference.DeleteAsync();
            string output = "Delete Announcement with " + id;
            return Content(output);
        }


    }
}