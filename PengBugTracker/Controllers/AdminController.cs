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





        //    [Authorize(Roles ="Admin, Manager")]
        //    public ActionResult ManageProjectUsers(){
        //}


    }

}


