using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public sealed class Invoice
    {
        public int Id { get; set; }

        public decimal? Sum { get; set; }

        public int? Status { get; set; }

        public string Hash { get; set; }
    }
}
