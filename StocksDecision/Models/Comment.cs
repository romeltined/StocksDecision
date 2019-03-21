using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StocksDecision.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}