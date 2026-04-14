/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
- This creates the robot object that is stored in the database

*/
using Greenhouse.Domain.Enums;

namespace Greenhouse.Domain.Models;

public class Robot
{
    public int Id {get; set;}
    public string Name {get; set;} = "";
    public double BatteryLevel{get; set;} = 100;
    public int? CurrentTaskId {get; set;} = null;
    public RobotState State{get; set;}
}