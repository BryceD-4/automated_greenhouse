//This updates crops, changes moisture level, marks ready for harvest
using Greenhouse.Infrastructure.Data;
using Greenhouse.Domain.DTOs;
using Greenhouse.Domain.Models;
using Greenhouse.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Application.Services;

public class CropService
{
    private readonly GreenhouseDbContext _db; 

    public CropService(GreenhouseDbContext db)
   {
      _db = db;
   }

    public async Task<List<Crop>> GetAll()
    {
        return await _db.Crops.ToListAsync();
    }

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

        _db.Crops.Add(newCrop);
        await _db.SaveChangesAsync();

        return newCrop;
    }  
}