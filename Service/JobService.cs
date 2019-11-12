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
            new Job() { Id = 2, Name = "Usuários desativados", SqlQuery = "SELECT * FROM USUARIO WHERE ATIVO = 0", Seconds = 10 },
            new Job() { Id = 3, Name = "Spleep 5 segundos", SqlQuery = "select 1 as id, \"Alexandre\" as name, SLEEP(5) as ativo from dual", Seconds = 10, TimeoutSeconds = 4 },
            new Job() { Id = 3, Name = "Spleep 2 segundos", SqlQuery = "select 1 as id, \"Alexandre\" as name, SLEEP(2) as ativo from dual", Seconds = 10 }
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

                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    MySqlDataReader reader = null;

                    try
                    {
                        if (job.TimeoutSeconds.HasValue)
                            cmd.CommandTimeout = job.TimeoutSeconds.Value;

                        cmd.CommandText = query;

                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        reader = cmd.ExecuteReader();

                        sw.Stop();

                        var r = Serialize(reader);
                        var json = JsonConvert.SerializeObject(r);

                        _logger.LogInformation($"Query executado com sucesso em {sw.Elapsed}");
                    }
                    catch (MySqlException ex)
                    {
                        switch (ex.Number)
                        {
                            case -1:
                                _logger.LogError($"O tempo limite do comando expirou, o tempo limite configurado para {job.TimeoutSeconds ?? 0} segundos.");
                                break;
                            default:
                                _logger.LogError($"Erro ao consultar a query: {query} - Erro {ex.Message}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Erro ao consultar a query: {query} - Erro {ex.Message}");
                    }
                    finally
                    {
                        reader?.Close();
                        conn?.Close();
                    }
                }
            }

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

        public IEnumerable<Dictionary<string, object>> Serialize(MySqlDataReader reader)
        {
            var results = new List<Dictionary<string, object>>();
            var cols = new List<string>();

            for (var i = 0; i < reader.FieldCount; i++) 
                cols.Add(reader.GetName(i));

            while (reader.Read()) 
                results.Add(SerializeRow(cols, reader));

            return results;
        }
        private Dictionary<string, object> SerializeRow(IEnumerable<string> cols, MySqlDataReader reader)
        {
            var result = new Dictionary<string, object>();

            foreach (var col in cols) 
                result.Add(col, reader[col]);

            return result;
        }
    }
}