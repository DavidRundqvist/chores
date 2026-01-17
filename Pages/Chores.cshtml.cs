using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chores.Models;
using Chores.Repositories;

namespace chores.Pages;

public class ChoresModel : PageModel
{
    private readonly FileRepository _repository;
    
    public List<Chore> Chores { get; set; } = [];

    public ChoresModel()
    {
        var choreFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "chores.json");
        _repository = new FileRepository(choreFilePath);
    }

    public async Task OnGetAsync()
    {
        var chores = await _repository.LoadAsync();
        Chores = chores.ToList();
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
            Name = newName.Trim(),
            Frequency = TimeSpan.FromDays(newDays)
        };

        var choresList = chores.ToList();
        choresList.Add(newChore);
        await _repository.SaveAsync(choresList.ToArray());

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int choreIndex)
    {
        var chores = await _repository.LoadAsync();
        
        if (choreIndex < 0 || choreIndex >= chores.Length)
        {
            ModelState.AddModelError("", "Invalid chore");
            await OnGetAsync();
            return Page();
        }

        var choresList = chores.ToList();
        choresList.RemoveAt(choreIndex);
        await _repository.SaveAsync(choresList.ToArray());

        return RedirectToPage();
    }
}

