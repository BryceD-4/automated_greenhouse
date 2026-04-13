
using Greenhouse.Domain.DTOs;
using Greenhouse.Domain.Models;
using Greenhouse.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Greenhouse.PresentationAPI.EndPoints;

public static class CropEndPoints
{
    public static void MapCropEndpoints(this WebApplication app)
    {
        app.MapPost("/crops",
            async Task<Results<BadRequest<string>, Created<Crop>>> (CreateCropDto dto, CropService service) =>
            {
                //Simple validation
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return TypedResults.BadRequest("NAME Required*");
                }

                var newCrop = await service.Add(dto);
                return TypedResults.Created($"/crops/{newCrop.Id}", newCrop);
            });

        app.MapGet("/crops", async (CropService service) =>
        {
           return await service.GetAll(); 
        });
    }
}
