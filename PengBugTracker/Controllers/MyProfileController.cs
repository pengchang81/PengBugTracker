using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PengBugTracker.Helpers;
using PengBugTracker.Models;

namespace PengBugTracker.Controllers
{
    [Authorize]
    [RequireHttps]
    public class MyProfileController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: MyProfileModel

        public ActionResult MyProfile()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);

            var data = new MyProfileModel();
            data.FirstName = user.FirstName;
            data.LastName = user.LastName;
            data.Email = user.Email;
            

            return View(data);
        }
               
        // POST : MyProfileViewModel
        [HttpPost]

        public ActionResult MyProfile(MyProfileModel model)
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;            

            db.SaveChanges();

            return RedirectToAction("MyProfile");

        }
    }
}


