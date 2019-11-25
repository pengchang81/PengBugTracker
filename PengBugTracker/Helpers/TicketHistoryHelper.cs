using Microsoft.AspNet.Identity;
using PengBugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace PengBugTracker.Helpers
{
    public class TicketHistoryHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public void RecordHistoricalChanges(Ticket oldTicket, Ticket newTicket)
        {
            if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
            {
                var newHistoryRecord = new TicketHistory
                {
                    Property = "TicketStatusId",
                    OldValue = oldTicket.TicketStatus.StatusName,
                    NewValue = newTicket.TicketStatus.StatusName,
                    Changed = (DateTime)newTicket.Updated,
                    TicketId = newTicket.Id,
                    UserId = HttpContext.Current.User.Identity.GetUserId()
                };

                db.TicketHistories.Add(newHistoryRecord);
            }

            if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)

            {
                var newHistoryRecord = new TicketHistory
                {
                    Property = "TicketPriorityId",
                    OldValue = oldTicket.TicketPriority.PriorityName,
                    NewValue = newTicket.TicketPriority.PriorityName,
                    Changed = (DateTime)newTicket.Updated,
                    TicketId = newTicket.Id,
                    UserId = HttpContext.Current.User.Identity.GetUserId()
                };

                db.TicketHistories.Add(newHistoryRecord);
            }

            if (oldTicket.AssignedToUserId != newTicket.AssignedToUserId)

            {
                var newHistoryRecord = new TicketHistory
                {
                    Property = "AssignedToUserId",
                    OldValue = oldTicket.AssignedToUserId == null ? "UnAssigned" : oldTicket.AssignedToUser.FullName,
                    NewValue = newTicket.AssignedToUserId == null ? "UnAssigned" : newTicket.AssignedToUser.FullName,
                    Changed = (DateTime)newTicket.Updated,
                    TicketId = newTicket.Id,
                    UserId = HttpContext.Current.User.Identity.GetUserId()
                };

                db.TicketHistories.Add(newHistoryRecord);
            }

            db.SaveChanges();

        }
    }
}