using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using PengBugTracker.Models;

namespace PengBugTracker.Helpers
{
    public class TicketHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private RoleHelper roleHelper = new RoleHelper();
        public int SetDefaultTicketStatus()
        {
            return db.TicketStatus.FirstOrDefault(ts => ts.StatusName == "Open").Id;
        }

        public List<Ticket> ListMyTickets()
        {
            var myTickets = new List<Ticket>();
            var userId = HttpContext.Current.User.Identity.GetUserId();
            var user = db.Users.Find(userId);

            //Step 1: Obtain my role or Determine my role
            var myRole = roleHelper.ListUserRoles(userId).FirstOrDefault();

            //Step 2: Use that role to build the appropriate set of Tickets

            //Optional version
            //switch(myRole)
            //{
            //    case "Admin":
            //        myTickets.AddRange(db.Tickets);
            //        break;
            //    case"Project_Manager";
            //        myTickets.AddRange(user.Projects.SelectMany(p => p.Tickets));
            //        break;
            //    case "Developer";
            //        myTickets.AddRange(db.Tickets.Where(t => t.DeveloperId == userId));
            //        break;
            //    case "Submitter";
            //        myTickets.AddRange(db.Tickets.Where(t => t.SubmitterId == userId));
            //        break;
            //    default:
            //        break;
            //}            
            
            //Optional verson2
            if(myRole == "Admin")
            {
                myTickets.AddRange(db.Tickets);
            }
            else if(myRole == "Manager")
            {
                myTickets.AddRange(user.Projects.SelectMany(p=>p.Tickets));
            }
            else if(myRole == "Developer")
            {
                myTickets.AddRange(db.Tickets.Where(t => t.DeveloperId==userId));
            }
            else if(myRole == "Submitter")
            {
                myTickets.AddRange(db.Tickets.Where(t => t.SubmitterId==userId));
            }
            return myTickets;                                                        
        }
    }

}