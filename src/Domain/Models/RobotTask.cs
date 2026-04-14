/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
- This creates the RobotTask object that is stored in the database

*/
using Greenhouse.Domain.Enums;

namespace Greenhouse.Domain.Models;

public class RobotTask
{
    //C# uses pascal case
    public int Id {get; set;}
    public TaskType Type {get; set;}
    //Nullable --> '?'
    public int? RobotId { get; set; }
    public int? CropId { get; set; }    
    public TaskState Status { get; set; }
    public double Progress { get; set; } = 0;
    public double Duration { get; set; } = 5;
    //Harvest = 40, Irrigate = 70, Charge = 100
    public int Priority {get; set;}
}
