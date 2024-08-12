namespace AllUp.Interfaces
{
    public interface IPaginationVM
    {
        int CurrentPage { get; }
        int End { get; }
        bool HasNext { get; }
        bool HasPrevious { get; }
        int PageCount { get; }
        int Start { get; }
    }
}