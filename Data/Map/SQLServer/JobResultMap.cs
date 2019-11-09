using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using worker_sqlexpress.Domain;

namespace worker_sqlexpress.Data.Map.SQLServer
{
    public class JobResultMap : IEntityTypeConfiguration<JobResult>
    {
        public void Configure(EntityTypeBuilder<JobResult> builder)
        {
            // Primary Key
            builder.HasKey(c => c.Id);

            // Table & Column Mappings
            builder.ToTable("JobResult");

            builder.Property(c => c.JobName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.ResultJson)
                .IsRequired();
        }
    }
}