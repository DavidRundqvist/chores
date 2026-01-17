using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chores.Models;
using Chores.Repositories;

namespace chores.Pages;

public class ChoreWithNextDue
{
    public Chore Chore { get; set; } = null!;
    public DateTime? LastPerformed { get; set; }
    public DateTime NextDueDate { get; set; }
    public int DaysUntilDue { get; set; }
    public bool IsOverdue { get; set; }
}

public class RecordsModel : PageModel
{
    private readonly ChoreRepository _choreRepository;
    private readonly RecordRepository _recordRepository;
    
    public List<Chore> Chores { get; set; } = [];
    public List<Record> AllRecords { get; set; } = [];
    public List<ChoreWithNextDue> UpcomingChores { get; set; } = [];

    public RecordsModel()
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
            var lastRecord = AllRecords
                .Where(r => r.ChoreId == chore.Id)
                .OrderByDescending(r => r.PerformedAt)
                .FirstOrDefault();

            DateTime nextDueDate;
            if (lastRecord == null)
            {
                // Never been performed - due today
                nextDueDate = today;
            }
            else
            {
                nextDueDate = lastRecord.PerformedAt.Date.Add(chore.Frequency);
            }

            // Move to next Saturday if not already on the due date
            int daysUntilSaturday = (int)(DayOfWeek.Saturday - nextDueDate.DayOfWeek);
            if (daysUntilSaturday < 0)
                daysUntilSaturday += 7;
            nextDueDate = nextDueDate.AddDays(daysUntilSaturday);

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

    public async Task<IActionResult> OnPostDeleteAsync(Guid recordId)
    {
        var records = await _recordRepository.LoadAsync();
        var record = records.FirstOrDefault(r => r.Id == recordId);

        if (record == null)
        {
            ModelState.AddModelError("", "Record not found");
            await OnGetAsync();
            return Page();
        }

        var recordsList = records.ToList();
        recordsList.Remove(record);
        
        await _recordRepository.SaveAsync(recordsList.ToArray());

        return RedirectToPage();
    }
}
