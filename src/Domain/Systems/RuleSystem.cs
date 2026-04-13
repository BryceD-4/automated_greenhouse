
using Greenhouse.Domain.Models;
using Greenhouse.Application.Services;

namespace Greenhouse.Domain.Systems;

public class RuleSystem
{
    //Called by the simulation service parallel calls
    public async Task EvaluateCrop(Crop crop, TaskService taskService, double deltaTime)
    {
        //determines when to water the crops
        if (crop.MoistureLevel < 60)
        {
            await taskService.CreateWaterTask(crop.Id);
        }
        //Keep the values from looking off in table (no negatives)
        if(crop.MoistureLevel < 0)
        {
            crop.MoistureLevel = 0;
        }

        if(crop.GrowthLevel > 90)
        {
            await taskService.CreateHarvestTask(crop.Id);

            //Crop needs to get harvested once it is ready 
            // or else it starts to go bad
            crop.HealthLevel -= 1 *deltaTime;
        }
        
        if(crop.GrowthLevel > 100)
        {
            crop.GrowthLevel = 100;
        }
    }

    public async Task EvaluateRobot(Robot robot, TaskService taskService)
    {
        //Keep display from going out of range
        if(robot.BatteryLevel < 0)
        {
            robot.BatteryLevel = 0;
        }
        if(robot.BatteryLevel > 100)
        {
            robot.BatteryLevel = 100;
        }

        //Add a charging battery task
        if(robot.BatteryLevel < 25)
        {
            await taskService.CreateChargeBatteryTask(robot.Id);
        }
    }
}