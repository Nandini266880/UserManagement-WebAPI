namespace Users.Application.Models
{
    public class UserQuery
    {
        // --- Pagination ---
        private int _pageSize = 10;
        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > 100 ? 100 : value;
        }

        // --- Sorting ---
        public string SortBy { get; set; } = "FullName"; 
        public bool IsDescending { get; set; } = false;

        // Allowed sort columns — prevents SQL injection via dynamic sort
        public static readonly IReadOnlyList<string> AllowedSortColumns =
            ["FullName", "Email", "CreatedAt"];
    }
}
