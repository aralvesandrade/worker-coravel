using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dapper;
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

            var jobResult = new JobResult { JobName = job.Name };

            var connectionMySql = "Server=localhost;Database=hubcommerce-db;User=root;Password=123;";

            using (MySqlConnection conexao = new MySqlConnection(connectionMySql))
            {
                var query = job.SqlQuery.ToLower();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                _logger.LogInformation($"Executar query: {query}");

                try
                {
                    var result = conexao.Query(query) as IEnumerable<IDictionary<string, object>>;
                    result = result.Select(r => r.Distinct().ToDictionary(d => d.Key, d => d.Value));

                    if (result.Count() > 0)
                    {
                        var json = JsonConvert.SerializeObject(result);
                        Console.WriteLine(json);

                        sw.Stop();
                        jobResult.ResultJson = json;
                        jobResult.Runtime = sw.Elapsed;
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

                try
                {
                    _db.JobsResult.Add(jobResult);
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao gravar registro na tabela JOBRESULT - Erro: {ex.Message}");
                }
            }
        }
    }
}