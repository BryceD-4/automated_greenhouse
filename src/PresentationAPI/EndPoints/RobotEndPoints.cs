/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
- This calls the CropService to create or delete a Robot or return all robots when this is requested
by the specified http
- All http requests locally run from 'TestQueries.http'

*/
using Greenhouse.Application.Services;
using Greenhouse.Domain.Models;
using Greenhouse.Domain.DTOs;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Greenhouse.PresentationAPI.EndPoints;

public static class RobotEndPoints
{
    public static void MapRobotEndpoints(this WebApplication app)
    {
        //URL to the POST request signals creation
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
        //URL GET request signal get all
        app.MapGet("/robots", async (RobotService service) =>
        {
           return await service.GetAll(); 
        });

        //URL DELETE request 
        app.MapDelete("/robots/{id}", async Task<Results<NoContent, NotFound>> (int id, RobotService service) =>
        {
            var success = await service.Delete(id);
            //If successful, returns no content, if unsucessful (0) returns not found
            return success
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
    }
}
