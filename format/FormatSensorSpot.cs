using meshbridge.interfaces;

namespace meshbridge.format
{
    public sealed class FormatSensorSpot: IUserData
    {
        public string UserName { get; set; } = string.Empty;

        public required string Type { get; set; }

        public required ulong Time { get; set; }

        public required double Latitude { get; set; }

        public required double Longitude { get; set; }

        public double? Altitude { get; set; }

        public double? Bearing { get; set; }

        public double? Speed { get; set; }

        public double? Accuracy { get; set; }

        public bool HasVector => Bearing.HasValue && Speed.HasValue;

        public override string ToString()
        {
            string format = nameof(FormatSensorSpot).Replace("Format", string.Empty);
            if (HasVector) return string.Format("{0}, (Lat;Lon) {1};{2}, {3}° на скорости {4}", format, Latitude, Longitude, (int)Bearing.Value, Speed);
            else return string.Format("{0}, (Lat;Lon) {1};{2}", format, Latitude, Longitude);
        }
    }
}
