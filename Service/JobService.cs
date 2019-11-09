using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using worker_sqlexpress.Data;
using worker_sqlexpress.Domain;
using worker_sqlexpress.Service.Interface;

namespace worker_sqlexpress.Service
{
    public class JobService : IJobService
    {
        private readonly ILogger<JobService> _logger;
        private readonly SQLServerContext _db;
        private readonly List<Job> _jobs = new List<Job> {
            new Job() { Id = 1, Name = "Usuários ativos", SqlQuery = "SELECT * FROM USUARIO WHERE ATIVO = 1", Seconds = 1 },
            new Job() { Id = 2, Name = "Usuários desativados", SqlQuery = "SELECT * FROM USUARIO WHERE ATIVO = 0", Seconds = 5 }
        };

        public JobService(ILogger<JobService> logger, SQLServerContext db)
        {
            _logger = logger;
            _db = db;
        }

        public List<Job> GetAll()
        {
            //return _db.Jobs.ToList();
            return _jobs;
        }

        public void Process(Job job)
        {
            _logger.LogInformation($"Execution : {job.Name}");
        }
    }
}