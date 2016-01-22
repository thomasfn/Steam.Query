using System.Collections.Generic;
using System.Linq;

namespace Steam.Query.MasterServers.Filtering
{
    public static class Filter
    {
        /// <summary>
        /// Servers not matching the given filter
        /// </summary>
        public static NotFilter Not(ConstantFilter filter) => new NotFilter(filter);

        /// <summary>
        /// Servers not matching any given filter
        /// </summary>
        public static NorFilter Nor(params ConstantFilter[] filters) => new NorFilter(filters);

        /// <summary>
        /// Servers not matching all given filters at the same time
        /// </summary>
        public static NandFilter Nand(params ConstantFilter[] filters) => new NandFilter(filters);
        
        /// <summary>
        /// Servers using anti-cheat technology (VAC, but potentially others as well)
        /// </summary>
        public static ConstantFilter IsSecure { get; } = new ConstantFilter("secure", "1");
        
        /// <summary>
        /// Servers running dedicated
        /// </summary>
        public static ConstantFilter IsDedicated { get; } = new ConstantFilter("type", "d");
        
        /// <summary>
        /// Servers running the specified modification (ex. cstrike)
        /// </summary>
        public static ConstantFilter GamedirIs(string mod) => new ConstantFilter("gamedir", mod);

        /// <summary>
        /// Servers whose name matches the given string
        /// <param name="filter">The name to match - * is used as wildcard</param>
        /// </summary>
        public static ConstantFilter NameMatches(string filter) => new ConstantFilter("name_match", filter);
        
        /// <summary>
        /// Servers that are NOT running game [appid] (This was introduced to block Left 4 Dead games from the Steam Server Browser)
        /// </summary>
        public static ConstantFilter AppIdIsNot(string appId) => new ConstantFilter("napp", appId);
        
        /// <summary>
        /// Servers running the specified map (ex. cs_italy)
        /// Does NOT support wildcards
        /// </summary>
        public static ConstantFilter MapIs(string map) => new ConstantFilter("map", map);
        
        /// <summary>
        /// Servers running on a Linux platform
        /// </summary>
        public static ConstantFilter IsLinuxServer { get; } = new ConstantFilter("linux", "1");
        
        /// <summary>
        /// Servers that are not empty
        /// </summary>
        public static ConstantFilter IsNotEmpty { get; } = new ConstantFilter("empty", "1"); //sic
        
        /// <summary>
        /// Servers that are not full
        /// </summary>
        public static ConstantFilter IsNotFull { get; } = new ConstantFilter("full", "1");  //sic
        
        /// <summary>
        /// Servers that are spectator proxies
        /// </summary>
        public static ConstantFilter IsSpectatorProxy { get; } = new ConstantFilter("proxy", "1");
        
        /// <summary>
        /// Servers that are empty
        /// </summary>
        public static ConstantFilter IsEmpty { get; } = new ConstantFilter("noplayers", "1");
        
    }
}