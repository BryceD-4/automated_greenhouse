/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DESCRIPTION:
- Used to create an enum for TaskType to avoid typos or confusion

*/
namespace Greenhouse.Domain.Enums;

public enum TaskType
{
    CropIrrigation, 
    RobotCharging,
    CropHarvesting
}