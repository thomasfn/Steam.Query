namespace Steam.Query.MasterServers.Filtering
{
    public class ConstantFilter : IFilter
    {
        public ConstantFilter(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }

        public string GetFilterString()
        {
            return $"{Key}\\{Value}";
        }
    }
}