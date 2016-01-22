namespace Steam.Query.MasterServers.Filtering
{
    public class NandFilter : IFilter
    {
        public ConstantFilter[] Filters { get; set; }

        public NandFilter(params ConstantFilter[] filters)
        {
            Filters = filters;
        }

        public string GetFilterString()
        {
            return $"nand\\{Filters.Length}{Filters.GetFilterCollectionString()}";
        }
    }
}