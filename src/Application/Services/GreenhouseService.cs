/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DESCRIPTION:
- This class was used for testing, when an http call is sent to mimick an action between a robot and crop
this class is called from GreenhouseEndPoints.cs

*/

using Microsoft.EntityFrameworkCore;
using Greenhouse.Infrastructure.Data;
using Greenhouse.Domain.Models;


namespace Greenhouse.Application.Services;

public class GreenhouseService
{
    private readonly GreenhouseDbContext _db;

    public GreenhouseService(GreenhouseDbContext db)
    {
        _db = db;
    }
    //Send a robot to water a crop
    public async Task<bool> WaterCrop(int robotId, int cropId)
    {
        var robot = await _db.Robots.FindAsync(robotId);
        var crop = await _db.Crops.FindAsync(cropId);

        if(robot == null || crop == null)
        {
            return false;
        }

        if(robot.BatteryLevel < 10)
        {
            return false;
        }

        crop.MoistureLevel += 10;
        robot.BatteryLevel -= 5;

        await _db.SaveChangesAsync();

        return true;
    }
}
