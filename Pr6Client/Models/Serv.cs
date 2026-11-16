using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr6Client.Models
{
    public class Serv
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public int DurationMinutes { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }
    }
}
