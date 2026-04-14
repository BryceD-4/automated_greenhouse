/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
- This outputs the database dashboard for ease of tracking 
task creation, crops, and robot activity. 
- Prints out as a JSON object only, see wwwroot/index.html for optimized html dashboard
- Viewable at: http://localhost:5242/api/status
*/



using Greenhouse.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Greenhouse.PresentationAPI.Controllers;

 //Call the controller and provide the controller route
[ApiController]
[Route("api/status")]
public class GreenhouseController : ControllerBase
{
    private readonly GreenhouseDbContext _dbContext;
    public GreenhouseController(GreenhouseDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult GetStatus()
    {   
        // Use .OrderBy(x => x.Id) to ensure consistent ordering in the tables when print out in index.html
        var crops = _dbContext.Crops.OrderBy(c => c.Id).ToList();
        var robots = _dbContext.Robots.OrderBy(r => r.Id).ToList();
        
        // Get tasks by priority
        var tasks = _dbContext.RobotTasks.OrderByDescending(t => t.Priority).ToList();
        
        //REturn the code 200 OK response with the data requested above
        return Ok(new
        {
            crops,
            robots,
            tasks
        });
    }
}