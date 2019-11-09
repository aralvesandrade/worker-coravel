using Microsoft.EntityFrameworkCore;
using worker_sqlexpress.Data.Map.SQLServer;
using worker_sqlexpress.Domain;

namespace worker_sqlexpress.Data
{
    public class SQLServerContext : DbContext
    {
        public SQLServerContext(DbContextOptions<SQLServerContext> options) : base(options)
        {
        }

        public virtual DbSet<Job> Jobs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfiguration(new JobMap());
        }
    }
}