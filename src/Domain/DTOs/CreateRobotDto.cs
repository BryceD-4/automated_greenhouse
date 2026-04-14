/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DESCRIPTION:
- DTO created to limit user control during robot creation
- Used by RobotEndPoints.cs

*/

namespace Greenhouse.Domain.DTOs;

//DTO = limits what client can control
//Clinet should control Type/Name, 
//but not waterlevel, Growth, health etc. The system should control this.

public record CreateRobotDto(string Name);