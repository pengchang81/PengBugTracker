using PengBugTracker.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PengBugTracker.Helpers
{
    public class NotificationHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public void ManageNotifications(Ticket oldTicket, Ticket newTicket)
        {
            var ticketAssigned = oldTicket.AssignedToUserId == null && newTicket.AssignedToUserId != null;
            var ticketUnAssigned = oldTicket.AssignedToUserId != null && newTicket.AssignedToUserId == null;
            var ticketReAssigned = oldTicket.AssignedToUserId != null && newTicket.AssignedToUserId != null && oldTicket.AssignedToUserId != newTicket.AssignedToUserId;

            if (ticketAssigned)
                AssignmentNotification(oldTicket, newTicket);
            else if (ticketUnAssigned)
                UnAssignmentNotification(oldTicket, newTicket);
            else if (ticketReAssigned)
            {
                UnAssignmentNotification(oldTicket, newTicket);
                AssignmentNotification(oldTicket, newTicket);
            }

        }





        public void SendNotification(Ticket ticket, string body)
        {
            var notification = new TicketNotification
            {
                TicketId = ticket.Id,
                IsRead = false,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                RecipientId = ticket.AssignedToUserId ?? ticket.OwnerUserId,
                Created = DateTime.Now,
                NotificationBody = body
            };
            db.TicketNotifications.Add(notification);
            db.SaveChanges();
        }






        private void AssignmentNotification(Ticket oldTicket, Ticket newTicket)
        {
            var notification = new TicketNotification
            {
                TicketId = newTicket.Id,
                IsRead = false,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                RecipientId = newTicket.AssignedToUserId,
                Created = DateTime.Now,
                NotificationBody = $"You have been assigned to <b>Ticket</b> #{newTicket.Id}, for the <b>{newTicket.Project.Name}</b>."
            };
            db.TicketNotifications.Add(notification);
            db.SaveChanges();
        }

        private void UnAssignmentNotification(Ticket oldTicket, Ticket newTicket)
        {
            var notification = new TicketNotification
            {
                TicketId = newTicket.Id,
                IsRead = false,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                RecipientId = oldTicket.AssignedToUserId,
                Created = DateTime.Now,
                NotificationBody = $"You have been unassigned from <b>Ticket</b> #{newTicket.Id}, for the <b>{newTicket.Project.Name}</b>."
            };
            db.TicketNotifications.Add(notification);
            db.SaveChanges();
        }

        public void AttachmentNotification(Ticket newTicket)
        {
            var notification = new TicketNotification
            {
                TicketId = newTicket.Id,
                IsRead = false,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                RecipientId = newTicket.AssignedToUserId,
                Created = DateTime.Now,
                NotificationBody = $"There is a new attachment for <b>Ticket</b> #{newTicket.Id}, for the <b>{newTicket.Project.Name}</b>."
            };
            db.TicketNotifications.Add(notification);
            db.SaveChanges();
        }

        public void CommentNotification(Ticket newTicket)
        {
            var notification = new TicketNotification
            {
                TicketId = newTicket.Id,
                IsRead = false,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                RecipientId = newTicket.AssignedToUserId,
                Created = DateTime.Now,
                NotificationBody = $"There is a new comment for <b>Ticket</b> #{newTicket.Id}, for the <b>{newTicket.Project.Name}</b>."
            };
            db.TicketNotifications.Add(notification);
            db.SaveChanges();
        }

        public static List<TicketNotification> GetUnreadNotifications()
        {
            var currentUserId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.Include("Sender").Include("Recipient").Where(t => t.RecipientId == currentUserId && !t.IsRead).ToList();
        }
    }
}