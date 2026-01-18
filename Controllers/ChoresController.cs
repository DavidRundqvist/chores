using Microsoft.AspNetCore.Mvc;
using Chores.Models;
using Chores.Repositories;
using Chores.Utilities;

namespace Chores.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChoresController : ControllerBase
{
    private readonly ChoreRepository _choreRepository;
    private readonly RecordRepository _recordRepository;

    public ChoresController()
    {
        var choreFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "chores.json");
        var recordFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "records.json");
        _choreRepository = new ChoreRepository(choreFilePath);
        _recordRepository = new RecordRepository(recordFilePath);
    }
[HttpGet("due-today")]
public async Task<ActionResult<object>> GetDueToday()
{
    var chores = await _choreRepository.LoadAsync();
    var records = await _recordRepository.LoadAsync();
    
    var today = DateTime.Now.Date;
    var dueChores = new List<DueChoreDto>();

    foreach (var chore in chores)
    {
        var nextDueDate = DueChoreCalculator.CalculateNextDueDate(chore, records, today);

        // Include if due today or earlier (overdue)
        if (nextDueDate <= today)
        {
            dueChores.Add(new DueChoreDto
            {
                Id = chore.Id,
                Name = chore.Name,
                NextDueDate = nextDueDate,
                IsOverdue = nextDueDate < today
            });
        }
    }

    // Wrap the array in an object for HA
    var response = new
    {
        chores = dueChores
    };

    return Ok(response);
}

}

public class DueChoreDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime NextDueDate { get; set; }
    public bool IsOverdue { get; set; }
}
