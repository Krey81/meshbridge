using meshbridge.format;
using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(FormatSensorSpot))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
