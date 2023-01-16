using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Phahung_BN.Models;
using System;
using Firebase.Auth;
using Google.Cloud.Firestore;
using Phahung_BN.Middleware;
using System.IO;
using Firebase.Storage;
using Phahung_BN.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.Threading;



namespace Phahung_BN.Controllers
{
    [Route("")]
    public class ImageController : Controller
    {
        private FirestoreDb _firestoreDb;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;
        private const string FirebaseApiKey = "AIzaSyBvIi9lOZb6CPnHWnvdswOSDT_znj_TyIo";
        private const string FirebaseBucket = "phahung-db.appspot.com";
        private FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig(FirebaseApiKey));

        public ImageController(FiresbaseConnect opt, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            _env = env;
            _firestoreDb = opt.get_FirestoreDb();
            _httpContextAccessor = httpContextAccessor;
        }


        [HttpPost("/upload-image")]
        [Authorize(roles.user, roles.admin)]
        public async Task<IActionResult> UploadImage(FileUploadViewModel model)
        {
            string token = Request.Headers["authorization"];
            var file = model.File;
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
                    file.CopyTo(new FileStream(filepath, FileMode.Create));
                    // Convert file to filestream
                    ms = new FileStream(Path.Combine(path, file.FileName), FileMode.Open);

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
                        .Child($"{file.FileName}")
                        .PutAsync(ms, cancellation.Token);

                    task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                    try
                    {
                        var downloadURL = await task;

                        return Ok(downloadURL);
                    }
                    catch (Exception ex)
                    {
                        return new JsonResult(new { message = ex.StackTrace }) { StatusCode = StatusCodes.Status500InternalServerError };
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

                }
                return new JsonResult(new { message = "Please upload file" }) { StatusCode = StatusCodes.Status400BadRequest };
            }
            catch (Exception ex)
            {
                return new JsonResult(new { message = ex.StackTrace }) { StatusCode = StatusCodes.Status400BadRequest };
            }
        }



    }
}
