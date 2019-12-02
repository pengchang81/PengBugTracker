using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PengBugTracker.Helpers;
using PengBugTracker.Models;

namespace PengBugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TicketHelper ticketHelper = new TicketHelper();
        private RoleHelper roleHelper = new RoleHelper();
        private ProjectHelper projectHelper = new ProjectHelper();
        private NotificationHelper notificationHelper = new NotificationHelper();
        private TicketHistoryHelper ticketHistoryHelper = new TicketHistoryHelper();

        // GET: All Tickets: Everyone can view tickets        
        [Authorize(Roles = "Admin,Manager,Developer,Submitter")]
        public ActionResult Index()
        {
            var allTickets = db.Tickets.ToList();
            //var highPriorityTickets = db.TicketPriorities.FirstOrDefault(tp => tp.PriorityName == "High").Tickets;
            //var medPriorityTickets = db.Tickets.Where(t => t.TicketPriority.PriorityName == "Medium");

            //What role do I occupy
            return View(allTickets);

        }
        //Get: All Tickets MyIndex: Authorized can view   
        [Authorize(Roles = "Admin,Manager,Developer,Submitter")]
        public ActionResult MyIndex(string req)
        {
            //First get the Id of the logged in User
            var userId = User.Identity.GetUserId();

            //Then get the Role they occupy
            var myRole = roleHelper.ListUserRoles(userId).FirstOrDefault();

            var myTickets = new List<Ticket>();

            //Then based on the role name we will push different data into the view
            switch (myRole)
            {
                case "Developer":
                    myTickets = db.Tickets.Where(t => t.AssignedToUserId == userId).ToList();
                    break;
                case "Submitter":
                    myTickets = db.Tickets.Where(t => t.OwnerUserId == userId).ToList();
                    break;
                case "Manager":
                    //mytickets are going to be all the Tickets on all the Project I am no.
                    myTickets = db.Users.Find(userId).Projects.SelectMany(t => t.Tickets).ToList();
                    break;
                case "Admin":
                    myTickets = db.Tickets.ToList();
                    break;
            }

            //This is for Dashboard showing Immediate - Recent Tickets
            //switch (req)
            //{
            //    case "Immediate":
            //        myTickets = myTickets.Where(t => t.TicketPriority.PriorityName.Equals("Immediate")).ToList();
            //        break;
            //    case "Recent":
            //        var yesterday = DateTime.Now.AddHours(-24);
            //        myTickets = myTickets.Where(t => t.Created >= yesterday).ToList();
            //        break;
            //}

            //MyIndex wants to fill some view with MY Tickets only.
            //Step 1: Ask the question, "What role do I occypy"
            //If I am a Submitter my Tickets are the Tickets where the OwnerUserId equals my Id.
            //If I am the Developer, my Tickets are the Tickets where the AssignedToUserId is my Id
            return View(myTickets);
        }
        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            var myProjects = projectHelper.ListUserProjects(User.Identity.GetUserId());
            ProjectHelper projectsHelper = new ProjectHelper();
            List<ApplicationUser> projectusers = new List<ApplicationUser>();
            List<string> projectDevIds = new List<string>();
            foreach (Project project in myProjects)
            {
                projectDevIds = projectsHelper.ListUsersOnProjectRole(project.Id, "Developer");   
            }
            foreach (string devId in projectDevIds)
            {
                ApplicationUser devUser = db.Users.Find(devId);
                projectusers.Add(devUser);
            }
            ViewBag.AssignedToUserId = new SelectList(projectusers, "Id", "DisplayName", ticket.AssignedToUserId);
            return View(ticket);
        }

        // GET: Tickets Submitters Create
        [Authorize(Roles ="Submitter")]
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var myProjects = projectHelper.ListUserProjects(userId);
            var devs = roleHelper.UsersInRole("Developer").ToList();

            ViewBag.ProjectId = new SelectList(myProjects, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "PriorityName");            
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "TypeName");
            ViewBag.AssignToUserId = new SelectList(devs, "Id", "FullName");
            return View();
        }

        // POST: Tickets Submitters Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProjectId,TicketTypeId,TicketPriorityId,Title,Description")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.Now;
                ticket.OwnerUserId = User.Identity.GetUserId();
                ticket.TicketStatusId = db.TicketStatus.FirstOrDefault(t=>t.StatusName == "Open").Id;
                db.Tickets.Add(ticket);
                db.SaveChanges();
                return RedirectToAction("MyIndex");
            }

            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "PriorityName", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "StatusName", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "TypeName", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            var devs = roleHelper.UsersInRole("Developer").ToList();
            ViewBag.AssignedToUserId = new SelectList(devs, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "PriorityName", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "StatusName", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "TypeName", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId,Title,Description,Created,Updated")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                //Record the old Ticket before it gets updated for comparison
                //var oldTicket = db.Tickets.FInd(Ticket.Id);
                var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);
                if (ticket.AssignedToUserId != null)
                {
                    ticket.AssignedToUserId = ticket.AssignedToUserId;

                }
                ticket.Updated = DateTime.Now;
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();

                var newTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);

                //decide for yourself HistoryHelper as to whether a history record needs to be added...
                ticketHistoryHelper.RecordHistoricalChanges(oldTicket, newTicket);
                notificationHelper.ManageNotifications(oldTicket, newTicket);

                return RedirectToAction("Index");
            }
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "PriorityName", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatus, "Id", "StatusName", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "TypeName", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
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

        //GET: Assign Ticket
        [Authorize(Roles = "Admin,Manager,DemoAdmin,DemoManager")]
        public ActionResult AssignTicket(int? id)
        {
            RoleHelper roleHelper = new RoleHelper();
            var ticket = db.Tickets.Find(id);

            var users = roleHelper.UsersInRole("Developer").ToList();
            ViewBag.DeveloperId = new SelectList(users, "Id", "FullName", ticket.DeveloperId);

            return View(ticket);
        }

        //POST: Assign Ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignTicket(Ticket model)
        {
            var ticket = db.Tickets.Find(model.Id);
            ticket.DeveloperId = model.DeveloperId;

            db.SaveChanges();

            var callbackUrl = Url.Action("Details", "Tickets", new { id = ticket.Id }, protocol: Request.Url.Scheme);

            try
            {
                EmailService ems = new EmailService();
                IdentityMessage msg = new IdentityMessage();
                ApplicationUser user = db.Users.Find(model.TicketAttachments);

                msg.Body = $"You have been assigned a new Ticket.{Environment.NewLine}Please click the following link to view the details <a href = \"{callbackUrl}\">New Ticket <a/>";

                msg.Destination = user.Email;
                msg.Subject = "Invite to Household";

                await ems.SendMailAsync(msg);
            }
            catch (Exception ex)
            {
                await Task.FromResult(0);
            }

            return RedirectToAction("Index");
        }                          
    }
}
