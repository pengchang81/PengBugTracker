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
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TicketHelper ticketHelper = new TicketHelper();
        private RoleHelper roleHelper = new RoleHelper();
        private ProjectHelper projectHelper = new ProjectHelper();

        // GET: Tickets
        [Authorize]
        public ActionResult Index()
        {
            var allTickets = db.Tickets;
            var highPriorityTickets = db.TicketPriorities.FirstOrDefault(tp => tp.PriorityName == "High").Tickets;
            var medPriorityTickets = db.Tickets.Where(t => t.TicketPriority.PriorityName == "Medium");

            //What role do I occupy
            return View(ticketHelper.ListMyTickets());

        }
        //Get: My Index
        [Authorize(Roles = "Admin,ProjectManager,Developer,Submitter")]
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
            return View("Index", myTickets);
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
            return View(ticket);
        }

        // GET: Tickets/Create
        [Authorize(Roles ="Submitter")]
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var myProjects = projectHelper.ListUserProjects(userId);


            ViewBag.ProjectId = new SelectList(myProjects, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "PriorityName");            
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "TypeName");
            return View();
        }

        // POST: Tickets/Create
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
                return RedirectToAction("MyIndex", "Tickets");
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
            ViewBag.AssignedToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedToUserId);
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

                ticket.Updated = DateTime.Now;
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();

                var newTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);

                //decide for yourself HistoryHelper as to whether a history record needs to be added...
                //auditHelper.RecordHistoricalChanges(oldTicket, newTicket);

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
    }
}
