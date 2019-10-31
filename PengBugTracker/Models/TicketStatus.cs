using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PengBugTracker.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }
        public string StatusName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; }

        public TicketStatus()
        {
            Tickets = new HashSet<Ticket>();

        }
    }
}