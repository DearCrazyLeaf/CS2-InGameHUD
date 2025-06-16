using System.Text.Json.Serialization;

namespace InGameHUD.Models
{
    public class PositionConfig
    {
        [JsonPropertyName("x_offset")]
        public float XOffset { get; set; }

        [JsonPropertyName("y_offset")]
        public float YOffset { get; set; }

        [JsonPropertyName("z_distance")]
        public float ZDistance { get; set; }

        public PositionConfig()
        {
        }

        public PositionConfig(float x, float y, float z)
        {
            XOffset = x;
            YOffset = y;
            ZDistance = z;
        }
    }
}