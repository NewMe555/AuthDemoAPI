using Microsoft.AspNetCore.Mvc;

namespace AuthDemoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        [HttpGet("threadinfo")]
        public IActionResult GetThreadInfo()
        {
            ThreadPool.GetMinThreads(out int workerMin, out int ioMin);
            ThreadPool.GetMaxThreads(out int workerMax, out int ioMax);
            ThreadPool.GetAvailableThreads(out int workerAvailable, out int ioAvailable);

            return Ok(new
            {
                WorkerThreads = new
                {
                    Min = workerMin,
                    Max = workerMax,
                    Available = workerAvailable,
                    InUse = workerMax - workerAvailable
                },
                IOThreads = new
                {
                    Min = ioMin,
                    Max = ioMax,
                    Available = ioAvailable,
                    InUse = ioMax - ioAvailable
                }
            });
        }
    }
}
// ----output to check total no of thread 
// {
// "workerThreads": {
// "min": 10,
// "max": 32767,
// "available": 32766,
// "inUse": 1
// },
// "ioThreads": {
// "min": 1,
// "max": 1000,
// "available": 1000,
// "inUse": 0
// }
// }