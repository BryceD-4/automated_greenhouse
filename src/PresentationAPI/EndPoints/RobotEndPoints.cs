
using Greenhouse.Application.Services;
using Greenhouse.Domain.Models;
using Greenhouse.Domain.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Greenhouse.PresentationAPI.EndPoints;

public static class RobotEndPoints
{
    public static void MapRobotEndpoints(this WebApplication app)
    {
        app.MapPost("/robots",
            async Task<Results<BadRequest<string>, Created<Robot>>> (CreateRobotDto dto, RobotService service) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return TypedResults.BadRequest("NAME REQuired");
            }    
            var newRobot = await service.Add(dto);
            return TypedResults.Created($"/crops/{newRobot.Id}", newRobot);
        });

        app.MapGet("/robots", async (RobotService service) =>
        {
           return await service.GetAll(); 
        });

        app.MapDelete("/robots/{id}", async Task<Results<NoContent, NotFound>> (int id, RobotService service) =>
        {
            var success = await service.Delete(id);
            return success
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}
