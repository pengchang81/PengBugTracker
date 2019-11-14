using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PengBugTracker.Helpers;
using PengBugTracker.Models;

namespace PengBugTracker.Controllers
{
    public class AdminController : Controller
    {        
        private ApplicationDbContext db = new ApplicationDbContext();
        private RoleHelper roleHelper = new RoleHelper();
        private ProjectHelper projectHelper = new ProjectHelper();
               
        // GET: Admin
        public ActionResult UserIndex()
        {
            var users = db.Users.Select(u => new UserProfileViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                DisplayName = u.DisplayName,
                AvatarUrl = u.AvatarUrl,
                Email = u.Email
            }).ToList();

            return View(users);            
        }

        //GET: UserRole

        public ActionResult ManageUserRole(string userId)
        {
            var currentRole = roleHelper.ListUserRoles(userId).FirstOrDefault();
            ViewBag.UserId = userId;
            ViewBag.UserRole = new SelectList(db.Roles.ToList(), "Name", "Name", currentRole);
            return View();
        }

        //Post:UserRole 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageUserRole(string userId, string userRole)
        {
            foreach (var role in roleHelper.ListUserRoles(userId))
            {
                roleHelper.RemoveUserFromRole(userId, role);
            }
            if (!string.IsNullOrEmpty(userRole))
            {
                roleHelper.AddUserToRole(userId, userRole);
            }
            return RedirectToAction("UserIndex");
        }


        public ActionResult ManageRoles()
        {
            ViewBag.UserIds = new MultiSelectList(db.Users.ToList(), "Id", "Email");
            ViewBag.Role = new SelectList(db.Roles, "Name", "Name");

            var users = new List<ManageRolesViewModel>();
            foreach (var user in db.Users.ToList())
            {
                users.Add(new ManageRolesViewModel
                {
                    UserName = $"{user.LastName},{user.FirstName}",
                    RoleName = roleHelper.ListUserRoles(user.Id).FirstOrDefault()
                });
            }
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageRoles(List<string> userIds, string role)
        {
            //Step 1: Unenroll all the seleted Users from ANY roles they may currently occupy.            
            foreach (var userId in userIds)
            {
                //What is th Role of this person??
                var userRole = roleHelper.ListUserRoles(userId).FirstOrDefault();
                if (userRole != null)
                {
                    roleHelper.RemoveUserFromRole(userId, userRole);
                }
            }

            //Step 2: Add them back to the selected Role
            if (!string.IsNullOrEmpty(role))
            {
                foreach (var userId in userIds)
                {
                    roleHelper.AddUserToRole(userId, role);
                }
            }
            return RedirectToAction("ManageRoles", "Admin");
        }

        [Authorize(Roles = "Admin, Manager")]
        public ActionResult ManageProjectUsers()
        {
            ViewBag.Projects = new MultiSelectList(db.Projects, "Id", "Name");
            ViewBag.Developers = new MultiSelectList(roleHelper.UsersInRole("Developer"), "Id", "Email");
            ViewBag.Submitters = new MultiSelectList(roleHelper.UsersInRole("Submitter"), "Id", "Email");

            if (User.IsInRole("Admin"))
            {
                ViewBag.ProjectManagerId = new SelectList(roleHelper.UsersInRole("Project_Manager"), "Id", "Email");
            }

            //Lets create a View Model for purposes of displaying User's and thier associated Projects
            var myData = new List<UserProjectListViewModel>();
            UserProjectListViewModel userVm = null;
            foreach (var user in db.Users.ToList())
            {
                userVm = new UserProjectListViewModel()
                {
                    Name = $"{user.LastName}, {user.FirstName}",
                    ProjectNames = projectHelper.ListUserProjects(user.Id).Select(p => p.Name).ToList()
                };

                if (userVm.ProjectNames.Count() == 0)
                    userVm.ProjectNames.Add("N/A");

                myData.Add(userVm);
            }
            return View(myData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageProjectUsers(List<int> projects, string projectManagerId, List<string> developers, List<string> submitters)
        {
            //Remove users from evvery project I have selected
            if (projects != null)
            {
                foreach (var projectId in projects)
                {
                    //Remove everyone from THIS project
                    foreach (var user in projectHelper.UsersOnProject(projectId).ToList())
                    {
                        projectHelper.RemoveUserFromProject(user.Id, projectId);
                    }

                    //Add back a PM if I can

                    if (!string.IsNullOrEmpty(projectManagerId))
                    {
                        projectHelper.AddUserToProject(projectManagerId, projectId);
                    }

                    if (developers != null)
                    {
                        foreach (var developerId in developers)
                        {
                            projectHelper.AddUserToProject(developerId, projectId);
                        }
                    }
                    if (submitters != null)
                    {
                        foreach(var submitterId in submitters)
                        {
                            projectHelper.AddUserToProject(submitterId, projectId);
                        }
                    }
                }
            }

            return RedirectToAction("ManageProjectUsers");
        }





    }   

}


