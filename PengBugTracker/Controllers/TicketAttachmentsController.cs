﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PengBugTracker.Helpers;
using PengBugTracker.Models;

namespace PengBugTracker.Controllers
{
    public class TicketAttachmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private ImageUploadValidator imageUploadValidator = new ImageUploadValidator();
        private NotificationHelper notificationHelper = new NotificationHelper();
        private TicketHistoryHelper auditHelper = new TicketHistoryHelper();

        // GET: TicketAttachments
        public ActionResult Index()
        {
            var ticketAttachments = db.TicketAttachments.Include(t => t.Ticket).Include(t => t.User);
            return View(ticketAttachments.ToList());
        }

        // GET: TicketAttachments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Create
        public ActionResult Create(int id)
        {
            TicketAttachment model = new TicketAttachment()
            {
                TicketId = id
            };
            return View(model);
        }

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.                       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TicketId,Description,Created")] TicketAttachment ticketAttachment, HttpPostedFileBase attachment)
        {
            if (ModelState.IsValid)
            {
                if (attachment != null)
                {
                    if (ImageUploadValidator.IsWebFriendlyImage(attachment) || ImageUploadValidator.IsWebFriendlyFile(attachment))
                    {
                        var fileName = Path.GetFileName(attachment.FileName);
                        var justFileName = Path.GetFileNameWithoutExtension(fileName);
                        var ticketId = ticketAttachment.TicketId;

                        justFileName = StringUtilities.URLFriendly(justFileName);
                        fileName = $"{justFileName}_{DateTime.Now.Ticks}{Path.GetExtension(fileName)}";
                        attachment.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                        ticketAttachment.FilePath = "/Uploads/" + fileName;
                        ticketAttachment.Created = DateTime.Now;
                        ticketAttachment.UserId = User.Identity.GetUserId();


                        //=========================== Ticket History =======================================
                        var oldTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticketId);

                        oldTicket.Updated = DateTime.Now;
                        //ticketAttachment.Ticket.Updated = DateTime.Now;
                        db.Entry(oldTicket).State = EntityState.Modified;

                        var newTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticketId);

                        auditHelper.RecordHistoricalChanges(oldTicket, newTicket);
                        notificationHelper.AttachmentNotification(newTicket);   // create notification
                        //===================================================================================

                        db.TicketAttachments.Add(ticketAttachment);

                        db.SaveChanges();
                    }
                }
                //Response.Redirect(Request.RawUrl);
                return RedirectToAction("Index", "TicketAttachments", new { id = ticketAttachment.TicketId });
            }
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "OwnerUserId", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,UserId,Description,FilePath,Created,Updated")] TicketAttachment ticketAttachment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(ticketAttachment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "OwnerUserId", ticketAttachment.TicketId);
            ViewBag.UserId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.UserId);
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            db.TicketAttachments.Remove(ticketAttachment);
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
