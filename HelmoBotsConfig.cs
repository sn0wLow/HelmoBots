using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace HelmoBots
{
    public class HelmoBotsConfig : BasePluginConfig
    {
        [JsonPropertyName("Enabled")]
        public bool IsEnabled { get; set; } = true;
    }
}
