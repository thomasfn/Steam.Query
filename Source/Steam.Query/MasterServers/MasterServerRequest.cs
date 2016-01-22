using System;

namespace Steam.Query.MasterServers
{
    using Filtering;

    public class MasterServerRequest
    {
        public MasterServerRegion Region { get; set; } = MasterServerRegion.All;
        public IFilter[] Filters { get; set; } = new IFilter[0];

        public int? MaximumPackets { get; set; } = null;

        public TimeSpan? RequestTimeout { get; set; } = TimeSpan.FromSeconds(15);

        public MasterServerRequest()
        {

        }

        public MasterServerRequest(MasterServerRegion region, params IFilter[] filters)
        {
            Region = region;
            Filters = filters;
        }
    }
}