namespace meshbridge.configuration
{
    /// <summary>
    /// Элемент конфигурации для настройки подключения к брокерам
    /// </summary>
    public record BrokerConfig
    {
        /// <summary>
        /// Хост
        /// </summary>
        public string Host { get; set; }  = "localhost";

        /// <summary>
        /// Порт
        /// </summary>
        public int Port { get; set; } = 1883;

        /// <summary>
        /// Юзер
        /// </summary>
        public string User { get; set; } = "anonymous";

        //TODO подключить секреты
        /// <summary>
        /// Пароль
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Версия протокола mqtt (кто-то в цепочке не умеет v5)
        /// </summary>
        public string Protocol { get; set; } = "V311";
    }
}
