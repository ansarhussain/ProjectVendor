namespace VendorProject.Common.DTOs
{
    /// <summary>
    /// Pagination and search query parameters
    /// </summary>
    public class PaginationQuery
    {
        private int _page = 1;
        private int _pageSize = 10;
        private const int MaxPageSize = 100;

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > MaxPageSize ? MaxPageSize : value);
        }

        /// <summary>
        /// Search term for name-based filtering
        /// </summary>
        public string? SearchName { get; set; }
    }
}
