using System;

namespace worker_sqlexpress.Domain
{
    public class Job
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SqlQuery { get; set; }
        public int Seconds { get; set; }
        public DateTime LastRun { get; set; }
        public DateTime NextExecution { get; set; }
        public int? TimeoutSeconds { get; set; }
    }
}