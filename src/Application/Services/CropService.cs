/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DESCRIPTION:
- This file provides methods to GetAll or Add a crop to and from the database.  
- Called by CRopEndPoints.cs
*/
using Greenhouse.Infrastructure.Data;
using Greenhouse.Domain.DTOs;
using Greenhouse.Domain.Models;
using Greenhouse.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Application.Services;

public class CropService
{
    private readonly GreenhouseDbContext _db_context; 

    public CropService(GreenhouseDbContext db)
   {
      _db_context = db;
   }

    public async Task<List<Crop>> GetAll()
    {
        return await _db_context.Crops.ToListAsync();
    }

    //Create a new instance of a crop, only name is entered by the user
    //dto is used to abstract the user altering only the name
    public async Task<Crop> Add(CreateCropDto dto)
    {
        var newCrop = new Crop
        {
            Name = dto.Name,
            MoistureLevel = 50,
            GrowthLevel = 0,
            HealthLevel = 100,
            State = CropState.Growing
        };

        _db_context.Crops.Add(newCrop);
        await _db_context.SaveChangesAsync();

        return newCrop;
    }  
}