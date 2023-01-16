using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using Newtonsoft.Json;
using Phahung_BN.Models;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Phahung_BN.Middleware;
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
using System.Text.RegularExpressions;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Phahung_BN.Controllers
{
    [Route("[controller]")]
    public class blogsController : Controller
    {
        //private string path ;
        //private string projID;
        private FirestoreDb _firestoreDb;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;
        private const string FirebaseApiKey = "AIzaSyBvIi9lOZb6CPnHWnvdswOSDT_znj_TyIo";
        private const string FirebaseBucket = "phahung-db.appspot.com";
        private FirebaseAuthProvider authProvider = new FirebaseAuthProvider(new FirebaseConfig(FirebaseApiKey));

        public blogsController(FiresbaseConnect opt, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env)
        {
            _env = env;
            _firestoreDb = opt.get_FirestoreDb();
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: All blogs
        [HttpGet]
        public async Task<object> getBlogs(string tagName, string search, int page, int perpage)
        {

            Query allblog_Query = null;
            if (tagName != null)
            {
                try
                {
                    //Query tag_Query = _firestoreDb.Collection("categories").WhereEqualTo("name", tagName);
                    //QuerySnapshot tag_QuerySnapshot = await tag_Query.GetSnapshotAsync();
                    //DocumentReference tagReference = tag_QuerySnapshot[0].Reference;
                    //DocumentSnapshot tagSnapshot = await tagReference.GetSnapshotAsync();
                    //string tagID = tagSnapshot.Id;
                    //// Console.WriteLine(tagID);

                    allblog_Query = _firestoreDb.Collection("blogs").WhereArrayContains("tags", tagName);
                    Console.WriteLine("Found tag ########################################### " + tagName + " : " + tagName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    allblog_Query = _firestoreDb.Collection("blogs");
                    Console.WriteLine("NOT Found tag ########################################### " + tagName);
                }

            }
            else
            {
                allblog_Query = _firestoreDb.Collection("blogs");
                Console.WriteLine("No tag input ########################################### ");
            }

            QuerySnapshot allblog_QuerySnapshot = await allblog_Query.GetSnapshotAsync();
            List<BlogPreviewModel> allBlogs = new List<BlogPreviewModel>();
            List<BlogPreviewModel> segmentedBlog = new List<BlogPreviewModel>();

            foreach (DocumentSnapshot documentSnapshot in allblog_QuerySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    if (search != null)
                    {
                        Regex regex = new Regex(search, RegexOptions.IgnorePatternWhitespace);
                        Match match = regex.Match(documentSnapshot.GetValue<string>("title"));
                        if (match.Success)
                        {
                            BlogPreviewModel blog = documentSnapshot.ConvertTo<BlogPreviewModel>();
                            allBlogs.Add(blog);
                        }
                    }
                    else
                    {
                        BlogPreviewModel blog = documentSnapshot.ConvertTo<BlogPreviewModel>();
                        allBlogs.Add(blog);
                    }


                }
            }

            if (page != 0 && perpage != 0)
            {
                int i = (page - 1) * perpage;
                while (i < allBlogs.Count && i < (page - 1) * perpage + perpage)
                {
                    segmentedBlog.Add(allBlogs[i]);
                    i++;
                }
                return new JsonResult(segmentedBlog) { StatusCode = StatusCodes.Status200OK };
            }

            return new JsonResult(allBlogs) { StatusCode = StatusCodes.Status200OK };
        }



        // POST blogs
        [HttpPost]
        public async Task<object> createBlogs([FromBody] BlogModel blog)
        {
            try
            {
                CollectionReference collectionReference = _firestoreDb.Collection("blogs");
                var tmp = JObject.Parse(blog.content.ToString());
                blog.content = null;
                blog.contentX = JsonConvert.SerializeObject(tmp);
                DocumentReference documentReference = await collectionReference.AddAsync(blog);
                blog.id = documentReference.Id;
                await documentReference.SetAsync(blog, SetOptions.MergeAll);
                return new JsonResult(new { message = "Blog created" }) { StatusCode = StatusCodes.Status201Created };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return new JsonResult(new { message = "Bad request" }) { StatusCode = StatusCodes.Status400BadRequest };


        }

        // DELETE blogs/:id
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteBlogs(string id)
        {
            //Query query = _firestoreDb.Collection("blogs").WhereEqualTo("id", id);
            //QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            //foreach (DocumentSnapshot documentSnapshot in querySnapshot.Documents)
            //{
            //    DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(documentSnapshot.Id);
            //    await documentReference.DeleteAsync();
            //}
            //string output = "Delete Document with " + id;
            DocumentReference documentReference = GetBlogDocumentReferenceByID(id);
            await documentReference.DeleteAsync();

            return new JsonResult(new { message = "Blog deleted" }) { StatusCode = StatusCodes.Status200OK }; ;
        }

        // PUT blogs/:id
        [HttpPut("{id}")]
        public async Task<Object> updateBlogs(string id, [FromBody] BlogModel newblog)
        {
            try
            {
                DocumentReference documentReference = GetBlogDocumentReferenceByID(id);
                DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
                BlogModel blog = documentSnapshot.ConvertTo<BlogModel>();
                if (blog != null)
                {
                    var tmp = JObject.Parse(newblog.content.ToString());
                    blog.content = null;
                    blog.contentX = JsonConvert.SerializeObject(tmp);
                    blog.tags = newblog.tags;
                    blog.status = newblog.status;
                    blog.title = newblog.title;
                    blog.author = newblog.author;
                    blog.createAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                    blog.image = newblog.image;
                    await documentReference.SetAsync(blog, SetOptions.MergeAll);
                    return new JsonResult(new { message = "Blog updated" }) { StatusCode = StatusCodes.Status200OK };

                }
                else
                {
                    return new JsonResult(new { message = "Blog doesn't exist" }) { StatusCode = StatusCodes.Status400BadRequest };
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return new JsonResult(new { message = "Server error" }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [HttpPut("{id}/status")]
        public async Task<object> updateBlogsStatus(string id, [FromBody] BlogModel newblog)
        {
            string status = newblog.status;
            Console.WriteLine("Status : " + status);
            try
            {
                DocumentReference documentReference = GetBlogDocumentReferenceByID(id);
                DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
                BlogModel blog = documentSnapshot.ConvertTo<BlogModel>();
                if (blog != null)
                {
                    if (status == "publish")
                    {
                        blog.status = "publish";
                        await documentReference.SetAsync(blog, SetOptions.MergeAll);
                        return new JsonResult(new { success = 1, status = "publish" }) { StatusCode = StatusCodes.Status200OK };
                    }
                    else if (status == "draft")
                    {
                        blog.status = "draft";
                        await documentReference.SetAsync(blog, SetOptions.MergeAll);
                        return new JsonResult(new { success = 1, status = "draft" }) { StatusCode = StatusCodes.Status200OK };
                    }
                    else
                    {
                        return new JsonResult(new { success = 0, message = "Unknown status" }) { StatusCode = StatusCodes.Status400BadRequest };
                    }

                }
                else
                {
                    return new JsonResult(new { message = "Blog doesn't exist" }) { StatusCode = StatusCodes.Status500InternalServerError };
                }

            }
            catch
            {

                return new JsonResult(new { message = "Cannot update status" }) { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        // GET blogs/:id
        [HttpGet("{id}")]
        public async Task<BlogModel> getBlogById(string id)
        {
            DocumentReference documentReference = GetBlogDocumentReferenceByID(id);
            DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();
            BlogModel blog = documentSnapshot.ConvertTo<BlogModel>();
            if (blog != null)
            {
                blog.content = System.Text.Json.JsonDocument.Parse(blog.contentX);
                blog.contentX = null;
                return blog;
            }
            return null;

        }



        [HttpPost("test")]
        public IActionResult test([FromBody] TestModel obj)
        {
            Console.WriteLine(obj.content);
            var tmp = JObject.Parse(obj.content.ToString());
            return Content(tmp.ToString(), "application/json", Encoding.UTF8);
        }

        public DocumentReference GetBlogDocumentReferenceByID(string id)
        {
            DocumentReference documentReference = _firestoreDb.Collection("blogs").Document(id);

            return documentReference;
        }

        [HttpPost("upload-image-by-file")]
        [Authorize(roles.admin)]
        public async Task<object> UploadImageByURL(BlogUploadByFileViewModel model)
        {
            string token = Request.Headers["authorization"];
            var file = model.File;
            FileStream ms;
            try
            {

                if (file.Length > 0)
                {
                    // Setup Upload Path
                    string folderName = "blogs-image";
                    string path = Path.Combine(_env.WebRootPath, $"images/{folderName}");
                    // Create Tmp file (will be removed in finally statement)
                    string filepath = path + $"/{file.FileName}";
                    var fs = new FileStream(filepath, FileMode.Create);
                    file.CopyTo(fs);
                    fs.Close();
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
                        .Child("blogs")
                        .Child("images")
                        .Child($"{file.FileName}")
                        .PutAsync(ms, cancellation.Token);

                    task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                    try
                    {
                        var downloadURL = await task;
                        ms.Close();
                        return new JsonResult(new { success = 1, file = new { url = downloadURL } }) { StatusCode = StatusCodes.Status200OK };

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

                }
                return new JsonResult(new { message = "Please upload file" }) { StatusCode = StatusCodes.Status400BadRequest };
            }
            catch(Exception ex)
            {
                Console.WriteLine("######### " + ex.Message);
                return new JsonResult(new { message = "Bad Request" }) { StatusCode = StatusCodes.Status400BadRequest };
            }
        }

    }
}
