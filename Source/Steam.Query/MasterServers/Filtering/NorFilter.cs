namespace Steam.Query.MasterServers.Filtering
{
    public class NorFilter : IFilter
    {
        public ConstantFilter[] Filters { get; set; }

        public NorFilter(params ConstantFilter[] filters)
        {
            Filters = filters;
        }

        public string GetFilterString()
        {
            return $"nor\\{Filters.Length}{Filters.GetFilterCollectionString()}";
        }
    }
}