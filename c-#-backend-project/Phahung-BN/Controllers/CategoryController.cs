using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Phahung_BN.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Phahung_BN.Controllers
{
    [Route("categories")]
    public class CategoryController : Controller
    {

        private FirestoreDb _firestoreDb;

        public CategoryController(FiresbaseConnect opt)
        {
            //path = @"D:\\Dev\\Github\\Phahung-BN\\Phahung-BN\\phahung-db-74c6652232b9.json";
            //System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            //projID = "phahung-db";
            _firestoreDb = opt.get_FirestoreDb();
        }
        // GET: Categories
        [HttpGet]
        public async Task<object> getCategories()
        {
            Query allCategoriesQuery = _firestoreDb.Collection("categories");
            QuerySnapshot allCategoriesQuerySnapshot = await allCategoriesQuery.GetSnapshotAsync();
            List<CategoryModel> allCategories = new List<CategoryModel>();


            foreach (DocumentSnapshot documentSnapshot in allCategoriesQuerySnapshot.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    CategoryModel category = documentSnapshot.ConvertTo<CategoryModel>();
                    allCategories.Add(category);
                }
            }

            return new JsonResult(allCategories) { StatusCode = StatusCodes.Status200OK }; 
        }
    }
}
