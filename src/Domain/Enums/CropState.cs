/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DESCRIPTION:
- Used to create an enum for CropState to avoid typos or confusion

*/

namespace Greenhouse.Domain.Enums;

public enum CropState
{
    Growing, 
    ReadyToHarvest, 
    Dead
}
