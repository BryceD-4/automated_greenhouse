/*
PROJECT: Automated Greenhouse
AUTHOR: Bryce Dixon
DESCRIPTION:
- Used to create an enum for TaskState to avoid typos or confusion

*/

namespace Greenhouse.Domain.Enums;

public enum TaskState
{
    Pending, 
    InProgress,
    Completed
}