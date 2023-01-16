using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using Phahung_BN.Models;
using Google.Cloud.Firestore;
using Phahung_BN.Middleware;

namespace Phahung_BN.Controllers
{
    [Route("")]
    public class commentController : ControllerBase
    {
        private FirestoreDb _firestoreDb;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly usersController _usersController;

        public commentController(FiresbaseConnect opt, IHttpContextAccessor httpContextAccessor, usersController usersController)
        {
            _firestoreDb = opt.get_FirestoreDb();
            _httpContextAccessor = httpContextAccessor;
            _usersController = usersController;
        }
        // ใช้ได้
        [HttpGet("blogs/{blogId}/comments")]
        public async Task<List<ParentCommentModel>> getAllComments(string blogId)
        {
            Query allcomment_Query = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").OrderBy("createAt");
            QuerySnapshot allcomment_QuerySnapshot = await allcomment_Query.GetSnapshotAsync();
            List<ParentCommentModel> allComments = new List<ParentCommentModel>();
            
            try {
                var user = (PrivateUserModel) _httpContextAccessor.HttpContext.Items["User"];
                foreach (DocumentSnapshot documentSnapshot in allcomment_QuerySnapshot)
                {                
                    ParentCommentModel parentCommentModel = documentSnapshot.ConvertTo<ParentCommentModel>();
                    
                    PrivateUserModel targetUser = await (Task<PrivateUserModel>) _usersController.getUserById(parentCommentModel.owner.UID);
                    Query allSubcom = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentCommentModel.id).Collection("subComments").OrderBy("createAt");
                    Console.WriteLine(targetUser.IsBan);
                    QuerySnapshot allSubcomSnapshot = await allSubcom.GetSnapshotAsync();
                    List<SubCommentModel> subComments = new List<SubCommentModel>();
                    foreach (DocumentSnapshot subcomSnapshot in allSubcomSnapshot)
                    {
                        SubCommentModel subCommentModel = subcomSnapshot.ConvertTo<SubCommentModel>();
                        if (targetUser.IsBan) continue;
                        else if (user.role == 0) {
                            parentCommentModel.comments.Add(subCommentModel);
                        } else {
                        if (subCommentModel.visible) {
                            parentCommentModel.comments.Add(subCommentModel);
                        }
                    }
                    }
                    if (targetUser.IsBan) continue;
                    else if (user.role == 0) {    
                        allComments.Add(parentCommentModel);
                    } 
                    else {
                        if (parentCommentModel.visible) {
                            allComments.Add(parentCommentModel);
                        }
                }
                return allComments;
            }
            } 
            catch (Exception e) {
                foreach (DocumentSnapshot documentSnapshot in allcomment_QuerySnapshot)
                {                
                    ParentCommentModel parentCommentModel = documentSnapshot.ConvertTo<ParentCommentModel>();
                    
                    PrivateUserModel targetUser = await (Task<PrivateUserModel>) _usersController.getUserById(parentCommentModel.owner.UID);
                    Query allSubcom = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentCommentModel.id).Collection("subComments").OrderBy("createAt");
                    QuerySnapshot allSubcomSnapshot = await allSubcom.GetSnapshotAsync();
                    List<SubCommentModel> subComments = new List<SubCommentModel>();
                    foreach (DocumentSnapshot subcomSnapshot in allSubcomSnapshot)
                    {
                        SubCommentModel subCommentModel = subcomSnapshot.ConvertTo<SubCommentModel>();
                        if (targetUser.IsBan) continue;
                        else if (subCommentModel.visible) {
                            parentCommentModel.comments.Add(subCommentModel);
                        }
                    }
                    
                    if (targetUser.IsBan) continue;
                    else if (parentCommentModel.visible) {
                        allComments.Add(parentCommentModel);
                    }
                }
                return allComments;
            }
            return allComments;
        }
        // ใช้ได้
        [HttpGet("blogs/{blogId}/comments/{id}")]
        public async Task<ParentCommentModel> GetCommentById(string blogId,string id)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(id);
            DocumentSnapshot documentSnapshots = await documentReference.GetSnapshotAsync();
            ParentCommentModel ParentCommentModel = documentSnapshots.ConvertTo<ParentCommentModel>();

            return ParentCommentModel;
        }

        // ใช้ได้
        [HttpPost("blogs/{blogId}/comments")]
        public async Task<object> createComment(string blogId,[FromBody] ParentCommentModel comment)
        {
            CollectionReference collectionReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments");

            var user = (PrivateUserModel) _httpContextAccessor.HttpContext.Items["User"];
            comment.owner = new CommentOwnerModel()
            {
                UID = user.UID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                picture = user.picture
            };

            comment.createAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            comment.isParent = true;
            comment.visible = true;
            comment.comments = new List<SubCommentModel>();

            DocumentReference documentReference = await collectionReference.AddAsync(comment);

            comment.id = documentReference.Id;
            await documentReference.SetAsync(comment, SetOptions.MergeAll);

            return new JsonResult(comment) { StatusCode=StatusCodes.Status201Created};

        }

        // ใช้ได้
        [HttpPost("blogs/{blogId}/comments/{id}")]
        public async Task<object> updateComment(string blogId,string id, [FromBody] ParentCommentModel comment)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(id);
            DocumentSnapshot oldParentCommentSnapshot = await documentReference.GetSnapshotAsync();
            ParentCommentModel oldParentComment = oldParentCommentSnapshot.ConvertTo<ParentCommentModel>();
            oldParentComment.content = comment.content;
            await documentReference.SetAsync(oldParentComment, SetOptions.MergeAll);
            return new JsonResult(oldParentComment) { StatusCode=StatusCodes.Status200OK };

        }

        [HttpPut("blogs/{blogId}/comments/{id}/hide")]
        [Authorize(roles.admin)]
        public async Task<object> hideComment(string blogId,string id, [FromBody] ParentCommentModel comment)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(id);
            DocumentSnapshot oldParentCommentSnapshot = await documentReference.GetSnapshotAsync();
            ParentCommentModel oldParentComment = oldParentCommentSnapshot.ConvertTo<ParentCommentModel>();
            oldParentComment.visible = false;
            await documentReference.SetAsync(oldParentComment, SetOptions.MergeAll);
            return new JsonResult(oldParentComment) { StatusCode=StatusCodes.Status200OK };
        }

        [HttpPut("blogs/{blogId}/comments/{id}/show")]
        [Authorize(roles.admin)]
        public async Task<object> showComment(string blogId,string id, [FromBody] ParentCommentModel comment)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(id);
            DocumentSnapshot oldParentCommentSnapshot = await documentReference.GetSnapshotAsync();
            ParentCommentModel oldParentComment = oldParentCommentSnapshot.ConvertTo<ParentCommentModel>();
            oldParentComment.visible = true;
            await documentReference.SetAsync(oldParentComment, SetOptions.MergeAll);
            return new JsonResult(oldParentComment) { StatusCode=StatusCodes.Status200OK };
        }
        [HttpPut("blogs/{blogId}/comments/{parentId}/subcomment/{subId}/hide")]
        [Authorize(roles.admin)]
        public async Task<object> hideSubComment(string blogId,string subId, string parentId, [FromBody] ParentCommentModel comment)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentId).Collection("subComments").Document(subId);
            DocumentSnapshot oldSubcommentSnapshot = await documentReference.GetSnapshotAsync();
            SubCommentModel oldSubcomment = oldSubcommentSnapshot.ConvertTo<SubCommentModel>();
            oldSubcomment.visible = false;
            await documentReference.SetAsync(oldSubcomment, SetOptions.MergeAll);
            return new JsonResult(oldSubcomment) { StatusCode=StatusCodes.Status200OK };
        }

        [HttpPut("blogs/{blogId}/comments/{parentId}/subcomment/{subId}/show")]
        [Authorize(roles.admin)]
        public async Task<object> showSubComment(string blogId,string subId, string parentId, [FromBody] ParentCommentModel comment)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentId).Collection("subComments").Document(subId);
            DocumentSnapshot oldSubcommentSnapshot = await documentReference.GetSnapshotAsync();
            SubCommentModel oldSubcomment = oldSubcommentSnapshot.ConvertTo<SubCommentModel>();
            oldSubcomment.visible = true;
            await documentReference.SetAsync(oldSubcomment, SetOptions.MergeAll);
            return new JsonResult(oldSubcomment) { StatusCode=StatusCodes.Status200OK };
        }

        // ใช้ได้
        [HttpDelete("blogs/{blogId}/comments/{id}")]
        public async Task<IActionResult> deleteComment(string blogId,string id)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(id);
            await documentReference.DeleteAsync();

            return new JsonResult(new { message = "Comment deleted"}) { StatusCode=StatusCodes.Status200OK };
        }

        

        // ใช้ได้
        [HttpGet("blogs/{blogId}/comments/{parentId}/subcomment")]
        public async Task<List<SubCommentModel>> GetAllSubCommentById(string blogId,string parentId)
        {
            Query allcomment_Query = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentId).Collection("subComments");
            QuerySnapshot allcomment_QuerySnapshot = await allcomment_Query.GetSnapshotAsync();
            List<SubCommentModel> allComments = new List<SubCommentModel>();

            var user = (PrivateUserModel) _httpContextAccessor.HttpContext.Items["User"];

            foreach (DocumentSnapshot documentSnapshot in allcomment_QuerySnapshot)
            {

                SubCommentModel subCommentModel = documentSnapshot.ConvertTo<SubCommentModel>();
                PrivateUserModel targetUser = await (Task<PrivateUserModel>) _usersController.getUserById(subCommentModel.owner.UID);
                if (targetUser.IsBan) continue;
                if (user.role == 0) {    
                    allComments.Add(subCommentModel);
                } else {
                    if (subCommentModel.visible)  {
                        allComments.Add(subCommentModel);
                    }
                }
            }

            return allComments;
        }

        // ใช้ได้
        [HttpGet("blogs/{blogId}/comments/{parentId}/subcomment/{subId}")]
        public async Task<SubCommentModel> GetSubCommentById(string blogId,string parentId, string subId)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentId).Collection("subComments").Document(subId);
            DocumentSnapshot documentSnapshots = await documentReference.GetSnapshotAsync();
            SubCommentModel SubCommentModel = documentSnapshots.ConvertTo<SubCommentModel>();

            return SubCommentModel;
        }

        // ใช้ได้
        [HttpPost("blogs/{blogId}/comments/{parentId}/subcomment")]
        public async Task<IActionResult> createSubComment(string blogId,[FromBody] SubCommentModel comment,string parentId)
        {
            CollectionReference collectionReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentId).Collection("subComments");

            var user = (PrivateUserModel) _httpContextAccessor.HttpContext.Items["User"];
            
            comment.parentId = parentId;
            comment.parentOwner = comment.parentOwner;
            comment.createAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            comment.visible = true;
            comment.owner = new CommentOwnerModel()
            {
                UID = user.UID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                picture= user.picture
            };


            DocumentReference documentReference = await collectionReference.AddAsync(comment);

            comment.id = documentReference.Id;
            await documentReference.SetAsync(comment, SetOptions.MergeAll);

            return new JsonResult(comment) { StatusCode=StatusCodes.Status201Created};
        }
        

        // ใช้ได้
        [HttpPut("blogs/{blogId}/comments/{parentId}/subcomment/{subId}")]
        public async Task<SubCommentModel> updateSubComment(string blogId, string parentId,string subId, [FromBody] SubCommentModel comment)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentId).Collection("subComments").Document(subId);
            DocumentSnapshot documentSnapshots = await documentReference.GetSnapshotAsync();
            SubCommentModel subcomment = documentSnapshots.ConvertTo<SubCommentModel>();
            subcomment.content = comment.content;
            await documentReference.SetAsync(subcomment, SetOptions.MergeAll);
            return comment;
        }

        // ใช้ได้
        [HttpDelete("blogs/{blogId}/comments/{parentId}/subcomment/{subId}")]
        public async Task<IActionResult> deleteSubComment(string blogId, string parentId,string subId)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentId).Collection("subComments").Document(subId);
            await documentReference.DeleteAsync();

            return new JsonResult(new { message = "Comment deleted"}) { StatusCode=StatusCodes.Status200OK };
        }

        
    }
}