using System.Threading.Tasks;
using Quartz;

namespace unbooru.JobDetailTest;

public class TestJob : IJob
{
    private readonly IScheduler _scheduler;
    
    public int AnimeID { get; set; }
    public bool Force { get; set; }

    public TestJob(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        
    }
}