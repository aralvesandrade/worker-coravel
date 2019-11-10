using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
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
            new Job() { Id = 1, Name = "Usuários ativos", SqlQuery = "SELECT * FROM USUARIO WHERE ATIVO = 1", Seconds = 5 },
            new Job() { Id = 2, Name = "Usuários desativados", SqlQuery = "SELECT * FROM USUARIO WHERE ATIVO = 0", Seconds = 10 }
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

        public void DeleteDateExpires()
        {
            var now = DateTime.Now;

            Console.Write($"Now: {now}");

            var optionsBuilder = new DbContextOptionsBuilder<SQLServerContext>();
            var connectionSQLServer = "Server=localhost;Database=worker-db;User Id=sa;Password=SqlExpress123;";
            optionsBuilder.UseSqlServer(connectionSQLServer);

            using (var db = new SQLServerContext(optionsBuilder.Options))
            {
                var jobsResult = db.JobsResult.Where(x => x.DateExpires < now).ToList();

                if (jobsResult.Count > 0)
                {
                    _logger.LogInformation($"Foram encontrados {jobsResult.Count} que podem ser excluidos");

                    try
                    {
                        db.JobsResult.RemoveRange(jobsResult);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Erro ao excluir registros da tabela JOBRESULT - Erro {ex.Message}");
                        throw;
                    }
                }
                else
                    _logger.LogInformation("Nenhum registro encontrado");
            }
        }

        public void Process(Job job)
        {
            _logger.LogInformation($"Execution : {job.Name}");

            var jobResult = new JobResult { JobName = job.Name };

            var connectionMySql = "Server=localhost;Database=hubcommerce-db;User=root;Password=123;";

            using (MySqlConnection conn = new MySqlConnection(connectionMySql))
            {
                var query = job.SqlQuery.ToLower();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                _logger.LogInformation($"Executar query: {query}");

                try
                {
                    var result = conn.Query(query) as IEnumerable<IDictionary<string, object>>;
                    result = result.Select(r => r.Distinct().ToDictionary(d => d.Key, d => d.Value));

                    if (result.Count() > 0)
                    {
                        var json = JsonConvert.SerializeObject(result);
                        _logger.LogInformation($"JSON: {json}");

                        Thread.Sleep(5000);
                        sw.Stop();
                        var now = DateTime.Now;
                        jobResult.ResultJson = json;
                        jobResult.DateExecuted = now;
                        jobResult.Runtime = sw.Elapsed;
                        jobResult.DateExpires = now.AddMinutes(10);
                    }
                    else
                    {
                        sw.Stop();
                        _logger.LogInformation("Nenhum registro encontrado");
                    }
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    _logger.LogError($"Erro ao consultar a query: {query} - Erro {ex.Message}");
                }                                
            }

            var optionsBuilder = new DbContextOptionsBuilder<SQLServerContext>();
            var connectionSQLServer = "Server=localhost;Database=worker-db;User Id=sa;Password=SqlExpress123;";
            optionsBuilder.UseSqlServer(connectionSQLServer);

            using (var db = new SQLServerContext(optionsBuilder.Options))
            {
                try
                {
                    db.JobsResult.Add(jobResult);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao inserir registro na tabela JOBRESULT - Erro: {ex.Message}");
                }    
            }
        }
    }
}