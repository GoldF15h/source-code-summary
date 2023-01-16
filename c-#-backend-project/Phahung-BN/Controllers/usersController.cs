using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;
using Phahung_BN.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


// For Authentication
using Firebase.Auth;
using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Phahung_BN.Services;
using Phahung_BN.Middleware;
using System.IO;
using Firebase.Storage;
using Phahung_BN.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.Threading;
using Newtonsoft.Json;

using Microsoft.AspNetCore.Mvc.Filters;


namespace Phahung_BN.Controllers
{
  [Route("")]
  public class usersController : Controller
  {
    private FirestoreDb _firestoreDb;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IWebHostEnvironment _env;
    private const string FirebaseApiKey = "AIzaSyBvIi9lOZb6CPnHWnvdswOSDT_znj_TyIo";
    private const string FirebaseBucket = "phahung-db.appspot.com";


    public usersController(FiresbaseConnect opt, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
    {
      //string path = @".\\phahung-db-74c6652232b9.json";
      //System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
      //FirebaseApp.Create(new AppOptions()
      //{
      //    Credential = GoogleCredential.GetApplicationDefault(),
      //    ProjectId = "phahung-db",
      //});
      _env = env;
      _firestoreDb = opt.get_FirestoreDb();
      _httpContextAccessor = httpContextAccessor;
    }

    //For Authentication
    private FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig(FirebaseApiKey));
    // GET signup : request signup page
    [HttpGet("/signup")]
    public IActionResult signUp()
    {
      return View();
    }

    // POST signup : create account

    [HttpPost("/signup")]
    public async Task<object> signUp([FromBody] UserModel userModel, [FromBody] string image)
    {
      string _uid = "";
      CollectionReference collectionReference = _firestoreDb.Collection("users");


      await authProvider.CreateUserWithEmailAndPasswordAsync(userModel.Email, userModel.Password).ContinueWith(task =>
      {

        if (task.IsCanceled)
        {
          Console.WriteLine("CreateUserWithEmailAndPasswordAsync was canceled.");
          return;
        }
        if (task.IsFaulted)
        {
          Console.WriteLine("CreateUserWithEmailAndPasswordAsync encountered an error");
          return;
        }
        // Firebase user has been created.
        FirebaseAuthLink authLink = task.Result;
        _uid = authLink.User.LocalId;
      });

      // Remove password from user model
      PrivateUserModel _privateUserModel = new PrivateUserModel();

      _privateUserModel.UID = _uid;
      _privateUserModel.Email = userModel.Email;
      _privateUserModel.FirstName = userModel.FirstName;
      _privateUserModel.LastName = userModel.LastName;
      _privateUserModel.IsBan = false;
      _privateUserModel.role = userModel.Role;
      _privateUserModel.likedBlogs = new List<string>();
      _privateUserModel.likedComments = new List<string>();

            // Add user to firestore
            await collectionReference.AddAsync(_privateUserModel);
      ResponseModel response = new ResponseModel("user created", 200);
      var jsonResponse = JsonConvert.SerializeObject(response);

      return jsonResponse;

      //var fbAuthLink = await authProvider.SignInWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
      //string token = fbAuthLink.FirebaseToken;
      //Console.WriteLine(userModel.Email + userModel.Password);
      //// saving the token in a session
      //if (token != null)
      //{
      //    Console.WriteLine(token);
      //    //HttpContext.Session.SetString("_UserToken", token);
      //    Console.WriteLine("Token Saved!!!");
      //    // return RedirectToAction("Index","Home");
      //    return Content("token saved!!!");
      //}

      //return Content(token);
    }

    [HttpGet("/signin")]
    // GET signin : request signin page
    public IActionResult signIn()
    {
      return View();
    }
    // POST signin : sign in to created account
    [HttpPost("/signin")]
    public async Task<IActionResult> signIn([FromBody] UserModel userModel)
    {
      //log in the user
      var fbAuthLink = await authProvider.SignInWithEmailAndPasswordAsync(userModel.Email, userModel.Password);
      string token = fbAuthLink.FirebaseToken;
      //var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
      //saving the token in a session variable
      Console.WriteLine(FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.ToString() + "\n#####################");
      if (token != null)
      {

        //HttpContext.Session.SetString("_UserToken", token);
        FirebaseToken decodedToken = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
        string uid = decodedToken.Uid;
        Console.WriteLine("########### LOGIN success ###########");
        Console.WriteLine(token.GetType());
        Console.WriteLine("UID of " + userModel.Email + " is " + uid);
        Response.Cookies.Append("token", token, new CookieOptions { HttpOnly = true, Expires = DateTime.Now.AddDays(1) });
      }
      //return RedirectToAction("Index","Home");
      //this.getSession(_token);
      return Content(token);
    }


    // POST get-session
    [HttpPost("/get-session")]
    public PrivateUserModel getSession()
    {
      PrivateUserModel user = new PrivateUserModel();
      try
      {
        user = (PrivateUserModel)_httpContextAccessor.HttpContext.Items["User"];

      }
      catch
      {

      }
      return user;
    }
    [HttpGet("/users")]
    public async Task<List<PrivateUserModel>> getAllUsers(int page, int perpage, string isban)
    {
      CollectionReference collectionReference = _firestoreDb.Collection("users");
      Query alluser_Query = null;
      Console.WriteLine(isban);
      if (isban != null)
      {
        if (isban == "true")
        {
          alluser_Query = _firestoreDb.Collection("users").WhereEqualTo("IsBan", true).WhereEqualTo("role",1);
        }
        else
        {
          alluser_Query = _firestoreDb.Collection("users").WhereEqualTo("IsBan", false).WhereEqualTo("role", 1);
                }

      }
      else
      {
        alluser_Query = _firestoreDb.Collection("users").WhereEqualTo("role", 1);
            }

      QuerySnapshot alluser_QuerySnapshot = await alluser_Query.GetSnapshotAsync();
      List<PrivateUserModel> allUsers = new List<PrivateUserModel>();
      List<PrivateUserModel> segmentedAllUsers = new List<PrivateUserModel>();
      foreach (DocumentSnapshot documentSnapshot in alluser_QuerySnapshot.Documents)
      {
        PrivateUserModel user = documentSnapshot.ConvertTo<PrivateUserModel>();
        allUsers.Add(user);
      }
      if (page != 0 && perpage != 0)
      {
        int i = (page - 1) * perpage;
        while (i < allUsers.Count && i < (page - 1) * perpage + perpage)
        {
          segmentedAllUsers.Add(allUsers[i]);
          i++;
        }
        return segmentedAllUsers;
      }
      return allUsers;

    }
    [HttpGet("/admin/users")]
    public async Task<List<PrivateUserModel>> getAllAdmins(int page, int perpage, string isban)
    {
        CollectionReference collectionReference = _firestoreDb.Collection("users");
        Query alluser_Query = null;
        Console.WriteLine(isban);
        if (isban != null)
        {
            if (isban == "true")
            {
                alluser_Query = _firestoreDb.Collection("users").WhereEqualTo("IsBan", true).WhereEqualTo("role",0);
            }
            else
            {
                alluser_Query = _firestoreDb.Collection("users").WhereEqualTo("IsBan", false).WhereEqualTo("role",0);
                }

        }
        else
        {
            alluser_Query = _firestoreDb.Collection("users").WhereEqualTo("role", 0);
            }

        QuerySnapshot alluser_QuerySnapshot = await alluser_Query.GetSnapshotAsync();
        List<PrivateUserModel> allUsers = new List<PrivateUserModel>();
        List<PrivateUserModel> segmentedAllUsers = new List<PrivateUserModel>();
        foreach (DocumentSnapshot documentSnapshot in alluser_QuerySnapshot.Documents)
        {
            PrivateUserModel user = documentSnapshot.ConvertTo<PrivateUserModel>();
            allUsers.Add(user);
        }
        if (page != 0 && perpage != 0)
        {
            int i = (page - 1) * perpage;
            while (i < allUsers.Count && i < (page - 1) * perpage + perpage)
            {
                segmentedAllUsers.Add(allUsers[i]);
                i++;
            }
            return segmentedAllUsers;
        }
        return allUsers;

    }




    [HttpGet("/users/{id}")]
    public async Task<PrivateUserModel> getUserById(string id)
    {
      CollectionReference collectionReference = _firestoreDb.Collection("users");
      Query alluser_Query = _firestoreDb.Collection("users").WhereEqualTo("UID", id);
      QuerySnapshot alluser_QuerySnapshot = await alluser_Query.GetSnapshotAsync();
      PrivateUserModel user = new PrivateUserModel();
      foreach (DocumentSnapshot documentSnapshot in alluser_QuerySnapshot.Documents)
      {
        user = documentSnapshot.ConvertTo<PrivateUserModel>();
      }

      return user;
    }

    // DELETE /users/:id/ban

    [HttpDelete("/users/{id}/ban")]
    [Authorize(roles.admin)]
    public async Task<IActionResult> ban(string id)
    {
      UserRecordArgs args = new UserRecordArgs()
      {
        Uid = id,
        Disabled = true,
      };
      UserRecord userRecord = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.UpdateUserAsync(args);
      // See the UserRecord reference doc for the contents of userRecord.
      Console.WriteLine($"Successfully updated user: {userRecord.Uid}");

      CollectionReference collectionReference = _firestoreDb.Collection("users");
      Query alluser_Query = _firestoreDb.Collection("users").WhereEqualTo("UID", id);
      QuerySnapshot alluser_QuerySnapshot = await alluser_Query.GetSnapshotAsync();
      DocumentReference documentReference = alluser_QuerySnapshot[0].Reference;
      DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
      PrivateUserModel user = documentSnapshot.ConvertTo<PrivateUserModel>();
      user.IsBan = true;
      await documentReference.SetAsync(user, SetOptions.MergeAll);

      return new JsonResult(new { message = $"ban user id {id}" }) {StatusCode=StatusCodes.Status200OK};
    }

    [HttpDelete("/users/{id}/unban")]
    [Authorize(roles.admin)]
    public async Task<IActionResult> unban(string id)
    {
      UserRecordArgs args = new UserRecordArgs()
      {
        Uid = id,
        Disabled = false,
      };
      UserRecord userRecord = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.UpdateUserAsync(args);
      // See the UserRecord reference doc for the contents of userRecord.
      Console.WriteLine($"Successfully updated user: {userRecord.Uid}");

      CollectionReference collectionReference = _firestoreDb.Collection("users");
      Query alluser_Query = _firestoreDb.Collection("users").WhereEqualTo("UID", id);
      QuerySnapshot alluser_QuerySnapshot = await alluser_Query.GetSnapshotAsync();
      DocumentReference documentReference = alluser_QuerySnapshot[0].Reference;
      DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
      PrivateUserModel user = documentSnapshot.ConvertTo<PrivateUserModel>();
      user.IsBan = false;
      await documentReference.SetAsync(user, SetOptions.MergeAll);

      return new JsonResult(new { message = $"unban user id {id}" }) {StatusCode=StatusCodes.Status200OK};
    }

    [HttpPut("/user/changepassword")]
    public async Task<IActionResult> changePassword([FromBody] string newPassword)
    {
      UserRecordArgs args = new UserRecordArgs()
      {
        Uid = getSession().UID,
        Password = newPassword
      };
      UserRecord userRecord = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.UpdateUserAsync(args);
      //DocumentReference documentReference = _firestoreDb.Collection("users").Document(id);
      return Content("change user's Password");
    }

    [HttpPut("/user/update")]
    public async Task<PrivateUserModel> updateUser([FromBody] PrivateUserModel updatedUser)
    {
      PrivateUserModel user = getSession();
      Query query = _firestoreDb.Collection("users").WhereEqualTo("UID", user.UID);
      QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
      DocumentReference userReference = querySnapshot[0].Reference;
      //string[] mergeFields = {"FirstName","LastName"};
      FileUploadViewModel fileUpload = new FileUploadViewModel();
      //fileUpload.Email = updatedUser.Email;
      //fileUpload.Password = updatedUser.
      //updatedUser.imageURL = UploadImage();
      await userReference.SetAsync(updatedUser, SetOptions.MergeAll);

      return updatedUser;
    }

    // DELETE /unsubscription
    [HttpDelete("/unsubscription/{uid}")]
    public async Task<IActionResult> unsubscription(string uid)
    {
      Query query = _firestoreDb.Collection("users").WhereEqualTo("UID", uid);
      QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
      DocumentReference userReference = querySnapshot[0].Reference;
      await userReference.DeleteAsync();

      await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.DeleteUserAsync(uid);
      return Content("Delete User with  : " + uid);
    }

    [HttpPut("/blogs/{blogId}/like")]
    public async Task<IActionResult> likeBlog(string blogId)
    {
      PrivateUserModel user = getSession();
      Query query = _firestoreDb.Collection("users").WhereEqualTo("UID", user.UID);
      QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
      DocumentReference userReference = querySnapshot[0].Reference;

      DocumentReference blogReference = _firestoreDb.Collection("blogs").Document(blogId);
      DocumentSnapshot blogSnapshot = await blogReference.GetSnapshotAsync();
      BlogModel blog = blogSnapshot.ConvertTo<BlogModel>();

      blog.likes = blog.likes + 1;
      user.likedBlogs.Add(blogId);

      await blogReference.SetAsync(blog, SetOptions.MergeAll);
      await userReference.SetAsync(user, SetOptions.MergeAll);
      return Content("like Blog : " + blogId);

    }


    [HttpPut("/blogs/{blogId}/dislike")]
    public async Task<IActionResult> dislikeBlog(string blogId)
    {
      PrivateUserModel user = getSession();

      DocumentReference blogReference = _firestoreDb.Collection("blogs").Document(blogId);
      DocumentSnapshot blogSnapshot = await blogReference.GetSnapshotAsync();
      BlogModel blog = blogSnapshot.ConvertTo<BlogModel>();

      blog.likes = blog.likes - 1;
      user.likedBlogs.Remove(blogId);

      await blogReference.SetAsync(blog, SetOptions.MergeAll);
      await updateUser(user);
      return Content("dislike Blog : " + blogId);
    }

    [HttpGet("/likedblogs")]
    public async Task<List<BlogPreviewModel>> getLikedBlogs()
    {
      List<BlogPreviewModel> likedBlogs = new List<BlogPreviewModel>();
      PrivateUserModel user = await updateLikeBlogs();
      foreach (string blogId in user.likedBlogs)
      {
        DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId);
        DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();

        likedBlogs.Add(documentSnapshot.ConvertTo<BlogPreviewModel>());
      }

      return likedBlogs;

    }

    [HttpPut("/blogs/{blogId}/comments/{id}/like")]
    public async Task<IActionResult> likeComment(string blogId, string id)
    {
        PrivateUserModel user = getSession();
        user.likedComments.Add(id);

        DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(id);
        DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
        ParentCommentModel parentComment = documentSnapshot.ConvertTo<ParentCommentModel>();
        parentComment.likes = parentComment.likes + 1;

        await documentReference.SetAsync(parentComment,SetOptions.MergeAll);
        await updateUser(user);

        return new JsonResult(new { message = "like comment" }) { StatusCode = StatusCodes.Status200OK };
    }

    [HttpPut("/blogs/{blogId}/comments/{id}/dislike")]
    public async Task<IActionResult> dislikeComment(string blogId, string id)
    {
        PrivateUserModel user = getSession();
        user.likedComments.Remove(id);

        DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(id);
        DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
        ParentCommentModel parentComment = documentSnapshot.ConvertTo<ParentCommentModel>();
        parentComment.likes = parentComment.likes - 1;
        
        await documentReference.SetAsync(parentComment, SetOptions.MergeAll);
        await updateUser(user);
        
        return new JsonResult(new { message = "dislike comment" }) { StatusCode = StatusCodes.Status200OK };

    }

    [HttpPut("/blogs/{blogId}/comments/{parentId}/subcomment/{subId}/like")]
    public async Task<IActionResult> likeSubComment(string blogId, string parentId, string subId)
    {
      try {
        PrivateUserModel user = getSession();

        user.likedComments.Add(subId);
        DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentId).Collection("subComments").Document(subId);
        DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
        if (documentSnapshot.Exists) {
          SubCommentModel subComment = documentSnapshot.ConvertTo<SubCommentModel>();
          subComment.likes = subComment.likes + 1;
          await documentReference.SetAsync(subComment,SetOptions.MergeAll);
          await updateUser(user);
          return new JsonResult(new { message = "like sub comment" }) { StatusCode = StatusCodes.Status200OK };
        }
        return new JsonResult(new { message = "Document doesn't exist" }) { StatusCode = StatusCodes.Status500InternalServerError };
      }
      catch {
        return new JsonResult(new { message = "error" }) { StatusCode = StatusCodes.Status400BadRequest };
      }

    }

    [HttpPut("/blogs/{blogId}/comments/{parentId}/subcomment/{subId}/dislike")]
    public async Task<IActionResult> dislikeSubComment(string blogId, string parentId, string subId)
    {
        try {
          PrivateUserModel user = getSession();

          user.likedComments.Remove(subId);
          DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId).Collection("parentComments").Document(parentId).Collection("subComments").Document(subId);
          DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
          if (documentSnapshot.Exists) {
            SubCommentModel subComment = documentSnapshot.ConvertTo<SubCommentModel>();
            subComment.likes = subComment.likes - 1;
            await documentReference.SetAsync(subComment,SetOptions.MergeAll);
            await updateUser(user);
            return new JsonResult(new { message = "dislike sub comment" }) { StatusCode = StatusCodes.Status200OK };
          }
          return new JsonResult(new { message = "Document doesn't exist" }) { StatusCode = StatusCodes.Status500InternalServerError };
      }
      catch {
        return new JsonResult(new { message = "error" }) { StatusCode = StatusCodes.Status400BadRequest };
      }

    }

    [HttpPut("/users/update-image")]
    public async Task<object> UploadImageByURL(BlogUploadByFileViewModel model)
    {
      string token = Request.Headers["authorization"];
      var file = model.File;
      // Console.WriteLine("inputfile = " + file.FileName);
      //Console.WriteLine("GET ID FOR UPDATE " + userId);
      PrivateUserModel user = getSession();
      Query query = _firestoreDb.Collection("users").WhereEqualTo("UID", user.UID);
      QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
      DocumentReference userReference = querySnapshot[0].Reference;
      DocumentSnapshot documentSnapshot = await userReference.GetSnapshotAsync();
      PrivateUserModel privateUserModel = documentSnapshot.ConvertTo<PrivateUserModel>();

      FileStream ms;
      try
      {

        if (file.Length > 0)
        {
          // Setup Upload Path
          string folderName = "users-image";
          string path = Path.Combine(_env.WebRootPath, $"images/{folderName}");
          // Create Tmp file (will be removed in finally statement)
          string filepath = path + $"/{file.FileName}";
          //string filepath = path + "/hello.jpg";
          var fs = new FileStream(filepath, FileMode.Create);
          file.CopyTo(fs);
          fs.Close();
          //Convert file to filestream
          ms = new FileStream(Path.Combine(path, file.FileName), FileMode.Open);
          // ms = new FileStream(Path.Combine(path, "hello.jpg"), FileMode.Open);

          // use to cancel the upload midway
          var cancellation = new CancellationTokenSource();

          var task = new FirebaseStorage(
              FirebaseBucket,
              new FirebaseStorageOptions
              {
                AuthTokenAsyncFactory = () => Task.FromResult(token),
                ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
              })
              .Child("users")
              .Child("profile-images")
              .Child($"{user.UID}")
              .PutAsync(ms, cancellation.Token);

          task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

          try
          {
            var downloadURL = await task;
            ms.Close();
            user.picture = downloadURL;
            await updateUser(user);
            return new JsonResult(new { url = downloadURL }) { StatusCode = StatusCodes.Status200OK };

          }
          catch (Exception ex)
          {
            Console.WriteLine("######### " + ex.ToString());
            return new JsonResult(new { message = "Cannot upload file to server" }) { StatusCode = StatusCodes.Status500InternalServerError };
          }

          finally
          {
            // Delete file after upload to firebase storage
            var fileInfo = new FileInfo(filepath);
            fileInfo.Delete();
          }

        }
        else
        {
          Console.WriteLine("no file input");
          return new JsonResult(new { message = "Please upload file" }) { StatusCode = StatusCodes.Status400BadRequest };
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("!!!!!!!!!  " + ex.ToString());
        return new JsonResult(new { message = "Bad Request" }) { StatusCode = StatusCodes.Status400BadRequest };
      }
    }

    // POST get-session
    [HttpPost("/logout")]
    public object logout()
    {
      var jsonResponse = new JsonResult(new { message = "logout success" }) { StatusCode = StatusCodes.Status200OK };
      return jsonResponse;
    }

    [HttpPut("/user/updatelikedblogs")]
    public async Task<PrivateUserModel> updateLikeBlogs()
    {
      Boolean isUserUpdate = false;
      List<string> notExistBlogId = new List<string>();
      PrivateUserModel user = getSession();
      foreach (string blogId in user.likedBlogs)
      {
        DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(blogId);
        DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
        BlogModel blog = documentSnapshot.ConvertTo<BlogModel>();
        if (blog == null)
        {
          notExistBlogId.Add(blogId);
          isUserUpdate = true;
        }
      }
      if (isUserUpdate)
      {
        foreach (string blogId in notExistBlogId)
        {
          user.likedBlogs.Remove(blogId);
        }
        await updateUser(user);
      }

      return getSession();
    }
  }



}
