/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DATE: April 2026
DESCRIPTION:
- This calls the CropService to create a crop or return all crops when this is requested
by the specified http
- All http requests locally run from 'TestQueries.http'

*/
using Greenhouse.Domain.DTOs;
using Greenhouse.Domain.Models;
using Greenhouse.Application.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Greenhouse.PresentationAPI.EndPoints;

public static class CropEndPoints
{
    public static void MapCropEndpoints(this WebApplication app)
    {
        //Responds to a POST to the local host ending with '/crops'
        app.MapPost("/crops",
            async Task<Results<BadRequest<string>, Created<Crop>>> (CreateCropDto dto, CropService service) =>
            {
                //Simple validation if the name is not entered
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    return TypedResults.BadRequest("NAME Required*");
                }
                //Add the crop to the database
                var newCrop = await service.Add(dto);
                return TypedResults.Created($"/crops/{newCrop.Id}", newCrop);
            });
        //responds to GET to '/crops' url to get all crops from the DB
        app.MapGet("/crops", async (CropService service) =>
        {
           return await service.GetAll(); 
        });
    }
}
