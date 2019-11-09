using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using worker_sqlexpress.Domain;

namespace worker_sqlexpress.Data.Map.SQLServer
{
    public class JobMap : IEntityTypeConfiguration<Job>
    {
        public void Configure(EntityTypeBuilder<Job> builder)
        {
            // Primary Key
            builder.HasKey(c => c.Id);

            // Table & Column Mappings
            builder.ToTable("Job");

            builder.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.SqlQuery)
                .IsRequired();

            builder.Property(c => c.Seconds)
                .IsRequired();
        }
    }
}