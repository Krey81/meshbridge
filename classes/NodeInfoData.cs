namespace meshbridge.classes
{
    /// <summary>
    /// Информация о ноде
    /// </summary>
    public record NodeInfoData
    {
        /// <summary>
        /// Целочисленный идентификатор
        /// </summary>
        public required uint NodeId;

        /// <summary>
        /// Короткое имя
        /// </summary>
        public required string ShortName;

        /// <summary>
        /// Длинное имя
        /// </summary>
        public required string LongName;

        //TODO избавиться
        /// <summary>
        /// Конструктор без параметров
        /// </summary>
        public NodeInfoData() { }

        /// <summary>
        /// Конструктор без длинного имени
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="shortName"></param>
        public NodeInfoData(uint nodeId, string shortName) : this(nodeId, shortName, shortName) { }

        /// <summary>
        /// Полный конструктор
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="shortName"></param>
        /// <param name="longName"></param>
        public NodeInfoData(uint nodeId, string shortName, string longName)
        {
            NodeId = nodeId;
            ShortName = shortName;
            LongName = longName;
        }
    }
}
