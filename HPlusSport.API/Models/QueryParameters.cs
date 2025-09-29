using Microsoft.EntityFrameworkCore.Query;

namespace HPlusSport.API.Models
{
    public class QueryParameters
    {
        const int _maxSize = 100;
        private int _size = 50;
        public int Page { get; set; } = 1;
        public int Size { get { return _size; } set { _size = Math.Min(_maxSize, value); } }

        public string SortBy { get; set; } = string.Empty;
        private string _sortOrder = "asc";

        public string SortOrder
        {
            get { return _sortOrder; }
            set { if (value == "asc" || value == "desc") { _sortOrder = value; } }

        }
    }

    public  class ProductQueryParameter : QueryParameters
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? SearchTerm { get; set; }
    }
}
