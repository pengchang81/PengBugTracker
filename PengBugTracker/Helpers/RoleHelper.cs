using PengBugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;

namespace PengBugTracker.Helpers
{
    public class RoleHelper
    {        
        private UserManager<ApplicationUser> userManager = 
            new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(
                new ApplicationDbContext()));
        

        private ApplicationDbContext db = new ApplicationDbContext();
        public bool IsUserInRole(string userId, string roleName)
        {
            return userManager.IsInRole(userId, roleName);
        }
        public ICollection<string> ListUserRoles(string userId)
        {
            return userManager.GetRoles(userId);
        }
        public bool AddUserToRole(string userId, string roleName)
        {
            var result = userManager.AddToRole(userId, roleName);
            return result.Succeeded;
        }
        public bool RemoveUserFromRole(string userId, string roleName)
        {
            var result = userManager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }
        public ICollection<ApplicationUser> UsersInRole(string roleName)
        {
            var resultList = new List<ApplicationUser>();
            var List = userManager.Users.ToList();
            foreach (var user in List)
            {
                if (IsUserInRole(user.Id, roleName))
                    resultList.Add(user);
            }
            return resultList;
        }
        public ICollection<ApplicationUser> UsersNotInRole(string roleName)
        {
            var resultList = new List<ApplicationUser>();
            var List = userManager.Users.ToList();
            foreach (var user in List)
            {
                if (!IsUserInRole(user.Id, roleName))
                    resultList.Add(user);
            }
            return resultList;
        }

        public bool IsUserDemo()
        {
            var user = HttpContext.Current.User.Identity.GetUserId();

            var demoARole = ListUserRoles(user).FirstOrDefault().Contains("DemoAdmin"); 
            var demoPRole = ListUserRoles(user).FirstOrDefault().Contains("DemoManager");
            var demoDRole = ListUserRoles(user).FirstOrDefault().Contains("DemoDeveloper");
            var demoSRole = ListUserRoles(user).FirstOrDefault().Contains("DemoSubmitter");

            if (demoARole || demoPRole || demoDRole || demoSRole == true)
            {
                return true;
            }
            return false;
        }
    }
}