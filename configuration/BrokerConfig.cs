namespace meshbridge.configuration
{
    public record BrokerConfig
    {
        public string Host { get; set; }  = "localhost";

        public int Port { get; set; } = 1883;

        public string User { get; set; } = "anonymous";

        public string Password { get; set; } = string.Empty;

        public string Protocol { get; set; } = "V311";
    }
}
