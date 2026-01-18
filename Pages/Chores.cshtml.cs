using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chores.Models;
using Chores.Repositories;
using Chores.Utilities;

namespace chores.Pages;

public class ChoreWithNextDue
{
    public Chore Chore { get; set; } = null!;
    public DateTime? LastPerformed { get; set; }
    public DateTime NextDueDate { get; set; }
    public int DaysUntilDue { get; set; }
    public bool IsOverdue { get; set; }
}

public class ChoresModel : PageModel
{
    private readonly ChoreRepository _choreRepository;
    private readonly RecordRepository _recordRepository;
    
    public List<Chore> Chores { get; set; } = [];
    public List<Record> AllRecords { get; set; } = [];
    public List<ChoreWithNextDue> UpcomingChores { get; set; } = [];

    public ChoresModel()
    {
        var choreFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "chores.json");
        var recordFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "records.json");
        _choreRepository = new ChoreRepository(choreFilePath);
        _recordRepository = new RecordRepository(recordFilePath);
    }

    public async Task OnGetAsync()
    {
        var chores = await _choreRepository.LoadAsync();
        Chores = chores.ToList();
        
        var records = await _recordRepository.LoadAsync();
        AllRecords = records.ToList();

        CalculateUpcomingChores();
    }

    private void CalculateUpcomingChores()
    {
        var today = DateTime.Now.Date;
        
        UpcomingChores = Chores.Select(chore =>
        {
            var nextDueDate = DueChoreCalculator.CalculateNextDueDate(chore, AllRecords, today);

            var lastRecord = AllRecords
                .Where(r => r.ChoreId == chore.Id)
                .OrderByDescending(r => r.PerformedAt)
                .FirstOrDefault();

            var daysUntilDue = (int)(nextDueDate - today).TotalDays;
            var isOverdue = daysUntilDue < 0;

            return new ChoreWithNextDue
            {
                Chore = chore,
                LastPerformed = lastRecord?.PerformedAt,
                NextDueDate = nextDueDate,
                DaysUntilDue = daysUntilDue,
                IsOverdue = isOverdue
            };
        })
        .OrderBy(c => c.NextDueDate)
        .ToList();
    }

    public async Task<IActionResult> OnPostDoneAsync(Guid choreId)
    {
        var chores = await _choreRepository.LoadAsync();
        var chore = chores.FirstOrDefault(c => c.Id == choreId);

        if (chore == null)
        {
            ModelState.AddModelError("", "Chore not found");
            await OnGetAsync();
            return Page();
        }

        var record = new Record
        {
            Id = Guid.NewGuid(),
            ChoreId = choreId,
            PerformedAt = DateTime.Now
        };

        var records = await _recordRepository.LoadAsync();
        var recordsList = records.ToList();
        recordsList.Add(record);

        await _recordRepository.SaveAsync(recordsList.ToArray());

        return RedirectToPage();
    }
}



