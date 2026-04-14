/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
- this class uses GReenhouseService to signal a watering action from a robot to a crop
- was used for testing in really stage
- kept for author's learning purposes
- All http requests locally run from 'TestQueries.http'

*/
using Greenhouse.Application.Services;

namespace Greenhouse.PresentationAPI.EndPoints;

public static class GreenhouseEndPoints
{
    public static void MapGreenhouseEndpoints(this WebApplication app)
    {
        //URL POST request signals this
        app.MapPost("/robots/{robotId}/water/{cropId}",
            async (int robotId, int cropId, GreenhouseService service) =>
            {
                var success = await service.WaterCrop(robotId, cropId);

                return success 
                    ? Results.Ok("Watered successfully")
                    : Results.BadRequest("Failed to water crop");
            });
    }
}
