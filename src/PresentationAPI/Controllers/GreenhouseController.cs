//Viewable at: http://localhost:5242/api/status
//This outputs the database information for ease of tracking. 

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
        // Use .OrderBy(x => x.Id) to ensure consistent ordering
        var crops = _dbContext.Crops.OrderBy(c => c.Id).ToList();
        var robots = _dbContext.Robots.OrderBy(r => r.Id).ToList();
        
        // You can also order tasks, perhaps by Id or their associated RobotId
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