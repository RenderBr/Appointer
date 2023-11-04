using Auxiliary.Configuration;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Appointer
{
    public class Group : JsonAttribute
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("NextRank")]
        public string NextRank { get; set; }

        [JsonPropertyName("Cost")]
        public int Cost { get; set; }

        [JsonConstructor]
        public Group(string name, string nextRank, int cost)
        {
            Name = name;
            NextRank = nextRank;
            Cost = cost;
        }
    }

    public class AppointerSettings : ISettings
    {
        [JsonPropertyName("StartGroup")]
        public string StartGroup { get; set; } = "default";

        [JsonPropertyName("UseAFKSystem")]
        public bool UseAFKSystem { get; set; } = true;

        [JsonPropertyName("KickForAFK")]
        public bool KickForAFK { get; set; } = false;

        [JsonPropertyName("KickThreshold")]
        public int KickThreshold { get; set; } = 1000;

        [JsonPropertyName("Groups")]
        public List<Group> Groups { get; set; } = new List<Group>() { new Group("member", "vip", 1000) };

        [JsonPropertyName("UseMySQL")]
        public bool UseMySQL { get; set; } = false;
    }
}
