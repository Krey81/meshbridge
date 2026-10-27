namespace meshbridge.classes
{
    //TODO добавить точность
    /// <summary>
    /// Информация о позиции GNSS
    /// </summary>
    public record PositionData
    {
        /// <summary>
        /// Идентификатор ноды (целочисленный)
        /// </summary>
        public required uint NodeId;

        /// <summary>
        /// Широта
        /// </summary>
        public required double Lat;

        /// <summary>
        /// Долгота
        /// </summary>
        public required double Lon;
    }
}
