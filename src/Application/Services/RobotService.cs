/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DESCRIPTION:
- This class handles robot creation, obtaining, and deletion from the database
- only used by RobotEndPoints when calls are made via http for robot queries. 

*/

using Greenhouse.Infrastructure.Data;
using Greenhouse.Domain.DTOs;
using Greenhouse.Domain.Models;
using Greenhouse.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Application.Services;

public class RobotService
{
    private readonly GreenhouseDbContext _db;

    public RobotService(GreenhouseDbContext db)
    {
        _db = db;
    }

    public async Task<List<Robot>> GetAll()
    {
        return await _db.Robots.ToListAsync();
    }

    public async Task<Robot> Add(CreateRobotDto dto)
    {
        var newRobot = new Robot
        {
            Name = dto.Name,
            BatteryLevel = 100,
            State = RobotState.Available
        };
        _db.Robots.Add(newRobot);
        await _db.SaveChangesAsync();

        return newRobot;
    }

    public async Task<bool> Delete(int id)
    {
        var robotDelete = await _db.Robots.FindAsync(id);

        if(robotDelete is null)
        {
            return false;
        } 

        _db.Robots.Remove(robotDelete);
        await _db.SaveChangesAsync();

        return true;
    }
}