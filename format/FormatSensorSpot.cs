using meshbridge.interfaces;
using System.Diagnostics.CodeAnalysis;

namespace meshbridge.format
{
    /// <summary>
    /// Формат, в котором посылает GNSS данные приложение SensorSpot (Android)
    /// </summary>
    public sealed class FormatSensorSpot : IUserData
    {
        /// <summary>
        /// Метка юзера, к которому относится запись
        /// </summary>
        /// <remarks>Берется из топика</remarks>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Тип сенсора
        /// </summary>
        /// <remarks>должно быть android.gps для GNSS сенсора</remarks>
        public string Type { get; set; } = string.Empty;

        public ulong Time { get; set; }

        /// <summary>
        /// Широта
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Долгота
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Высота
        /// </summary>
        public double? Altitude { get; set; }

        /// <summary>
        /// Направление
        /// </summary>
        public double? Bearing { get; set; }

        /// <summary>
        /// Скорость
        /// </summary>
        public double? Speed { get; set; }

        /// <summary>
        /// Точность
        /// </summary>
        public double? Accuracy { get; set; }

        /// <summary>
        /// Можно понять вектор движения
        /// </summary>
        [MemberNotNullWhen(true, nameof(Bearing))]
        [MemberNotNullWhen(true, nameof(Speed))]
        public bool HasVector => Bearing.HasValue && Speed.HasValue;

        ///<inheritdoc/>
        public override string ToString()
        {
            string format = nameof(FormatSensorSpot).Replace("Format", string.Empty);
            if (HasVector) return string.Format("{0}, (Lat;Lon) {1};{2}, {3}° на скорости {4}", format, Latitude, Longitude, (int)Bearing, Speed);
            else return string.Format("{0}, (Lat;Lon) {1};{2}", format, Latitude, Longitude);
        }
    }
}
