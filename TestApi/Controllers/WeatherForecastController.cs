using Microsoft.AspNetCore.Mvc;
using Mavusi.Linq.DataScience.GpuBound;
using Mavusi.DataScience.Visualizations;

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
        [HttpGet("plot")]
        public async Task<IActionResult> Plot()
        {
            var data = new List<ChartData>();
            foreach (var country in new List<string>() { "Argentina", "Brazil", "Austria", "Japan", "Canada" })
            {
                var x = Enumerable.Range(2016, 10).Select(i => i.ToString()).ToArray();
                //generate 10 random y values between 2 and 12
                var random = new Random();
                var y = Enumerable.Range(0, 10).Select(_ => random.NextDouble() * 10 + 2).ToArray();

                data.Add(new ChartData
                {
                    DataLabel = country,
                    xLabels = x.ToList(),
                    yValues = y.ToList()
                });
            }


            var html = QuickCharts.GenerateScatterPlot(data, width:800, height:450, darkMode:true);
                return Content(html, "text/html");
            //return Ok();
        }
    }
}
