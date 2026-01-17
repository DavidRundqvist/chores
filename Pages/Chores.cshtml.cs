using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chores.Models;
using Chores.Repositories;

namespace chores.Pages;

public class ChoresModel : PageModel
{
    private readonly FileRepository _repository;
    
    public List<Chore> Chores { get; set; } = [];
    public Guid? EditingId { get; set; }

    public ChoresModel()
    {
        var choreFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "chores.json");
        _repository = new FileRepository(choreFilePath);
    }

    public async Task OnGetAsync(Guid? editingId = null)
    {
        var chores = await _repository.LoadAsync();
        Chores = chores.ToList();
        EditingId = editingId;
    }

    public async Task<IActionResult> OnPostAsync(string newName, int newDays)
    {
        if (string.IsNullOrWhiteSpace(newName) || newDays <= 0)
        {
            ModelState.AddModelError("", "Invalid chore name or frequency");
            await OnGetAsync();
            return Page();
        }

        var chores = await _repository.LoadAsync();
        var newChore = new Chore
        {
            Id = Guid.NewGuid(),
            Name = newName.Trim(),
            Frequency = TimeSpan.FromDays(newDays)
        };

        var choresList = chores.ToList();
        choresList.Add(newChore);
        await _repository.SaveAsync(choresList.ToArray());

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(Guid choreId, string editName, int editDays)
    {
        if (string.IsNullOrWhiteSpace(editName) || editDays <= 0)
        {
            ModelState.AddModelError("", "Invalid chore name or frequency");
            await OnGetAsync(choreId);
            return Page();
        }

        var chores = await _repository.LoadAsync();
        var chore = chores.FirstOrDefault(c => c.Id == choreId);
        
        if (chore == null)
        {
            ModelState.AddModelError("", "Chore not found");
            await OnGetAsync();
            return Page();
        }

        chore.Name = editName.Trim();
        chore.Frequency = TimeSpan.FromDays(editDays);
        
        await _repository.SaveAsync(chores);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid choreId)
    {
        var chores = await _repository.LoadAsync();
        var chore = chores.FirstOrDefault(c => c.Id == choreId);
        
        if (chore == null)
        {
            ModelState.AddModelError("", "Chore not found");
            await OnGetAsync();
            return Page();
        }

        var choresList = chores.ToList();
        choresList.Remove(chore);
        await _repository.SaveAsync(choresList.ToArray());

        return RedirectToPage();
    }
}



