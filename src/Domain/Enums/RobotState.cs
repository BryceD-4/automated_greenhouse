/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DESCRIPTION:
- Used to create an enum for RobotState to avoid typos or confusion

*/
namespace Greenhouse.Domain.Enums;

public enum RobotState
{
    Available, 
    Irrigating, 
    Harvesting, 
    Charging
}
