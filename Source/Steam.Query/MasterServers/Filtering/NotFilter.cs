namespace Steam.Query.MasterServers.Filtering
{
    public class NotFilter : IFilter
    {
        public ConstantFilter Filter { get; set; }

        public NotFilter(ConstantFilter filter)
        {
            Filter = filter;
        }

        public string GetFilterString()
        {
            return $"nor\\1\\{Filter.GetFilterString()}";
        }
    }
}