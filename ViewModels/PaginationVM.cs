using AllUp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AllUp.ViewModels;

public class PaginationVM<T> : List<T>, IPaginationVM
{
    public PaginationVM(IEnumerable<T> items, int page, int count)
    {
        CurrentPage = page;
        PageCount = count;
        Start = CurrentPage - 2 <= 0 ? 1 : CurrentPage + 2 >= PageCount ? PageCount - 4 : CurrentPage - 2;
        End = CurrentPage + 2 > PageCount ? PageCount : CurrentPage - 2 <= 0 ? 5 : CurrentPage + 2;
        AddRange(items);
    }

    public int CurrentPage { get; }
    public int PageCount { get; }
    public int Start { get; }
    public int End { get; }
    public bool HasNext => CurrentPage < PageCount;
    public bool HasPrevious => CurrentPage > 1;

    public static async Task<PaginationVM<T>> CreateAsync(IQueryable<T> query, int page, int take = 3)
    {
        int count = (int)Math.Ceiling((decimal)query.Count() / take);
        IEnumerable<T> items = await query.Skip((page - 1) * take).Take(take).ToListAsync();
        return new PaginationVM<T>(items, page, count);
    }
}
