/*
This is used to reset the database between test trials
Used with contianerized DB integration testing. 
Ensures tests are tested in isolation. 
*/
using Greenhouse.Infrastructure.Data;

namespace Greenhouse.Tests.SharedFiles;
public static class DbCleaner
{
    public static async Task Clear(GreenhouseDbContext context)
    {
        context.RobotTasks.RemoveRange(context.RobotTasks);
        context.Robots.RemoveRange(context.Robots);
        context.Crops.RemoveRange(context.Crops);

        await context.SaveChangesAsync();
    }
}