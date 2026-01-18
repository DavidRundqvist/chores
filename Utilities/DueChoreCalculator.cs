using Chores.Models;
using Chores.Repositories;

namespace Chores.Utilities;

public static class DueChoreCalculator
{
    public static DateTime CalculateNextDueDate(Chore chore, IEnumerable<Record> records, DateTime today)
    {
        var lastRecord = records
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

        return nextDueDate;
    }
}
