using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PengBugTracker.Helpers;
using PengBugTracker.Models;

namespace PengBugTracker.Controllers
{   
    [Authorize]
    public class ProjectsController : Controller
    {        
        private ApplicationDbContext db = new ApplicationDbContext();
        private RoleHelper roleHelper = new RoleHelper();
        private ProjectHelper projectHelper = new ProjectHelper();

        //GET: Manage Users
        public ActionResult ManageProjectUsers(int id)
        {
            ViewBag.ProjectId = id;

            #region PM section
            var pmId = projectHelper.ListUsersOnProjectRole(id, "Manager").FirstOrDefault();
            ViewBag.ProjectManagerId = new SelectList(roleHelper.UsersInRole("Manager"), "Id", "Email", pmId);
            #endregion

            #region Dev section
            ViewBag.Developers = new MultiSelectList(roleHelper.UsersInRole("Developer"), "Id", "Email", projectHelper.ListUsersOnProjectRole(id, "Developer"));
            #endregion

            #region Sub section
            ViewBag.Submitters = new MultiSelectList(roleHelper.UsersInRole("Submitter"), "Id", "Email", projectHelper.ListUsersOnProjectRole(id, "Submitter").FirstOrDefault());
            #endregion

            return View();

    }
        //POST: Manage Users
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageProjectUsers(int projectId, string projectManagerId, List<string> developers, List<string> submitters)
        {
            foreach (var user in projectHelper.UsersOnProject(projectId).ToList())
            {
                projectHelper.RemoveUserFromProject(user.Id, projectId);
            }

            //In order to ensure that I always have the correct and current set of assign users
            //I will first remove all users from the project and then I will add back the selected users

            if (!string.IsNullOrEmpty(projectManagerId))
            {
                projectHelper.AddUserToProject(projectManagerId, projectId);
            }
            if(developers != null)
            {
                foreach(var developerId in developers)
                {
                    projectHelper.AddUserToProject(developerId, projectId);
                }
            }
            if (submitters != null)
            {
                foreach (var submitterId in submitters)
                {
                    projectHelper.AddUserToProject(submitterId, projectId);
                }
            }
            return RedirectToAction("ManageProjectUsers", new { id = projectId });
        }

        // GET: Projects (for viewing only) No post
        public ActionResult ProjectIndex()
        {
            
            var project = db.Projects.ToList();
            
            
            return View(project);
        }

        //Get: My Project Index (for viewing only) No post
        public ActionResult MyProjectIndex()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            var project = projectHelper.ListUserProjects(user.Id);
            var myRole = roleHelper.ListUserRoles(userId).FirstOrDefault();
            var myProjects = new List<Project>();
            switch (myRole)
            {
                case "Developer": 
                    myProjects = db.Users.Find(userId).Projects.ToList();
                    break;
                case "Submitter":
                    myProjects = db.Users.Find(userId).Projects.ToList();
                    break;
                case "Manager":
                    myProjects = db.Users.Find(userId).Projects.ToList();
                    break;
            }

            return View("MyProjectIndex",myProjects);
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

        // GET: Projects/Create//////////////////////////////////////////////////////////////////////////////////////////
        
        [Authorize(Roles ="Admin,Manager")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,Created")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.Created = DateTime.Now;
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("ProjectIndex");
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
                return RedirectToAction("ProjectIndex");
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
            return RedirectToAction("ProjectIndex");
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
