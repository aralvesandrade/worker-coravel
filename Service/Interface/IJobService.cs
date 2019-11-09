using System.Collections.Generic;
using worker_sqlexpress.Domain;

namespace worker_sqlexpress.Service.Interface
{
    public interface IJobService
    {
        List<Job> GetAll();
        void Process(Job job);
    }
}