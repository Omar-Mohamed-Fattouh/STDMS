namespace STDMS.Models
{
    public class AttendancePageResult
    {
        public IReadOnlyList<Attendance> Records { get; init; } = [];
        public int TotalRecords { get; init; }
        public int PresentRecords { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }

        public int AbsentRecords => TotalRecords - PresentRecords;
        public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
        public int FirstVisiblePage => Math.Max(1, Page - 2);
        public int LastVisiblePage => Math.Min(TotalPages, Page + 2);
        public int PresentPercentage => TotalRecords == 0
            ? 0
            : (int)Math.Round(PresentRecords * 100.0 / TotalRecords);
    }
}
