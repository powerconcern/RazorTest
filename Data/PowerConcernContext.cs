using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PowerConcern.Models
{
    public class PowerConcernContext : DbContext
    {
        public PowerConcernContext (DbContextOptions<PowerConcernContext> options)
            : base(options)
        {
        }

        public DbSet<Meter> Meter { get; set; }
        public DbSet<Configuration> Configuration { get; set; }
    }
}
