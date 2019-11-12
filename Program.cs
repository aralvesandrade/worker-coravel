using Coravel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using worker_sqlexpress.Data;
using worker_sqlexpress.Service;
using worker_sqlexpress.Service.Interface;

namespace worker_sqlexpress
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var jobService = serviceScope.ServiceProvider.GetService<IJobService>();
                var jobs = jobService.GetAll();

                jobService.Process(jobs[3]);
                //jobService.DeleteDateExpires();

                /*
                host.Services.UseScheduler(scheduler =>
                    {
                        foreach (var item in jobs)
                        {
                            scheduler.OnWorker(item.Name);

                            scheduler.Schedule(
                                () =>
                                {
                                    jobService.Process(item);
                                })
                            .EverySeconds(item.Seconds);
                        }
                    }
                );

                host.Services.UseScheduler(scheduler =>
                    {
                        scheduler.OnWorker("Date expires delete");

                        scheduler.Schedule(
                            () =>
                            {
                                jobService.DeleteDateExpires();
                            })
                        .EverySeconds(10);
                    }
                );
                */
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();

                    var connectionSQLServer = "Server=localhost;Database=worker-db;User Id=sa;Password=SqlExpress123;";
                    services.AddDbContext<SQLServerContext>(options => options.UseSqlServer(connectionSQLServer));

                    services.AddTransient<IJobService, JobService>();

                    services.AddScheduler();
                });
    }
}
