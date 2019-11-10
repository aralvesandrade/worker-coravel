using System;

namespace worker_sqlexpress.Domain
{
    public class JobResult
    {
        public int Id { get; set; }
        public string JobName { get; set; }
        public string ResultJson { get; set; }
        public DateTime DateExecuted { get; set; }
        public TimeSpan Runtime { get; set; }
        public DateTime DateExpires { get; set; }
    }
}