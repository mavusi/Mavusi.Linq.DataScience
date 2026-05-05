using Microsoft.AspNetCore.Mvc;
using Mavusi.Linq.DataScience.GpuBound;

namespace TestApi.Controllers
{
    [ApiController]
    [Route("/")]
    public class TestsController : ControllerBase
    {
        private readonly ILogger<TestsController> _logger;

        public TestsController(ILogger<TestsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var x = Enumerable.Range(0, 100000).Select(i => (double)i).ToArray();
            var y = Enumerable.Range(500, 100000).Select(i => (double)i * 2 + 5).ToArray();

            // GPU-accelerated correlation
            var correlation = x.CorrelationGpu(y); // Returns ~1.0

            // GPU-accelerated covariance
            var covariance = x.CovarianceGpu(y);
            return Ok(new { correlation, covariance });
        }
    }
}
