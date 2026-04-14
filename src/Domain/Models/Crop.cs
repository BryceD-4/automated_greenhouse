/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
- This creates the crop object that is stored in the database

*/

using Greenhouse.Domain.Enums;

namespace Greenhouse.Domain.Models;

public class Crop
{
    public int Id {get; set;}
    public string Name {get; set;} = "";
    public double MoistureLevel{get; set;} = 100;

    public double GrowthLevel {get; set;} = 0;

    public double HealthLevel{ get; set;} = 100;
    public CropState State {get; set;} 
}