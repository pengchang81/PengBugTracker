using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PengBugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }
        public string PriorityName { get; set; }
        public string Description { get; set; }

        //Nav
        public virtual ICollection<Ticket> Tickets { get; set; }

        public TicketPriority()
        {
            Tickets = new HashSet<Ticket>();

        }


    }
}