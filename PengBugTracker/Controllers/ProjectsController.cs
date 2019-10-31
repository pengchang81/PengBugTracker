using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PengBugTracker.Models;

namespace PengBugTracker.Controllers
{
    public class ProjectsController : Controller
    {
        //[Authorize]
        private ApplicationDbContext db = new ApplicationDbContext();
        ////private RoleHelper roleHelper = new RoleHelper();
        ////private ProjectHelper projectHelper = new ProjectHelper();

        ////public ActionResult ManageUsers(int id)
        ////{
        ////    View.Bag.ProjectId = id;

        //    #region PM section
        //    var pmId = projectHelper.ListUsersOnProjectInRole(id, "Project_Manager").FirstOrDefault();
        //    ViewBag.ProjectManagerId = new SelectList(roleHelper.UsersInRole("Project_Manager"), "Id", "Email", pmId);
        //    #endregion

        //    #region Dev section
        //    ViewBag.Developers = new MultiSelectList(roleHelper.UsersInRole("Developer"), "Id", "Email", projectHelper.ListUsersOnProjecInRole(id, "Developer"));
        //    #endregion

        //    #region Sub section
        //    ViewBag.Submitters = new MultiSelectList(roleHelper.UsersInRole("Submitter",) "Id", "Email", projectHelper.ListUsersOnProjectInRole(id, "Submitter").FirstOrDefault());
        //    #endregion

        //    return View();


        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ManageUsers(int projectId, string projectManagerId, List<string> developers, List<string> submitters)
        //{
        //    foreach(var user in projectHelper.UsersOnProject(projectId).ToList())
        //    {
        //        projectHelper.RemoveUserFromProject(user.Id, projectId);
        //    }

        //    //In order to ensure that I always have the correct and current set of assign users
        //    //I will first remove all users from the project and then I will add back the selected users

        //    if(!string.IsNullOrEmpty(projectManagerId))
        //    {
        //        ProjectHelper.AddUserToProject(projectManagerId,projectId)

        //    }

        //}





        // GET: Projects
        public ActionResult Index()
        {
            return View(db.Projects.ToList());
        }

        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // GET: Projects/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Created,Updated")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Created,Updated")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
