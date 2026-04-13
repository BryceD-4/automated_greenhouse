
using Greenhouse.Domain.Models;
using Greenhouse.Domain.Enums;

namespace Greenhouse.Application.Interfaces;

public interface ITaskRepository 
{
    Task<RobotTask?> CheckForCropTask(int id, TaskType type);
    Task<RobotTask?> CheckForRobotTask(int id, TaskType type);

    Task<RobotTask?> GetNextRobotTask();
    Task<RobotTask?> GetCurrentRobotTask(Robot robot);

    Task<int> ConfirmTaskIsAvailable(int robotId, int taskId);
    Task<Crop?> GetTargetCrop(RobotTask task);

    void AddTask(RobotTask task);

    Task SaveChangesAsync();

}