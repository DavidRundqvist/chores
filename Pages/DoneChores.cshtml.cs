using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Chores.Models;
using Chores.Repositories;
using Chores.Utilities;

namespace chores.Pages;


public class DoneChoresModel : PageModel
{
    private readonly ChoreRepository _choreRepository;
    private readonly RecordRepository _recordRepository;
    
    public List<Chore> Chores { get; set; } = [];
    public List<Record> AllRecords { get; set; } = [];

    public DoneChoresModel()
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
    }

}



