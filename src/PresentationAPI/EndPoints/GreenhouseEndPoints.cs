
using Greenhouse.Application.Services;

namespace Greenhouse.PresentationAPI.EndPoints;

public static class GreenhouseEndPoints
{
    public static void MapGreenhouseEndpoints(this WebApplication app)
    {
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
