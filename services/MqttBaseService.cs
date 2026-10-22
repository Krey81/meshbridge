using meshbridge.configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Formatter;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace meshbridge.services
{
    public abstract class MqttBaseService : BackgroundService
    {
        protected abstract string GetSectionName();

        protected abstract string GetTopic();

        protected abstract bool NeedSubscription();

        protected readonly TimeSpan _cooldown = TimeSpan.FromSeconds(5);

        protected readonly ILogger _logger;

        private readonly MqttClientFactory _mqttFactory = new MqttClientFactory();

        private IMqttClient? _mqttClient;

        protected CancellationToken _cancellationToken = CancellationToken.None;

        protected readonly BrokerConfig _brokerConfig;

        public MqttBaseService(
            ILogger logger,
            IOptionsMonitor<BrokerConfig> config
        )
        {
            _logger = logger;
            _brokerConfig = config.Get(GetSectionName());
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{service}: стартуем...", GetType().Name);

            _cancellationToken = stoppingToken;
            _mqttClient = _mqttFactory.CreateMqttClient();
            _mqttClient.ApplicationMessageReceivedAsync += OnReceive;

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_brokerConfig.Host , _brokerConfig.Port)
                .WithCredentials(_brokerConfig.User, _brokerConfig.Password)
                .WithProtocolVersion(GetProtocol(_brokerConfig.Protocol))
                .WithClientId($"meshbridge-{Guid.NewGuid()}")
                .Build();

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    MqttClientConnectResult conResult = await _mqttClient.ConnectAsync(options, _cancellationToken);
                    if (conResult.ResultCode != MqttClientConnectResultCode.Success)
                    {
                        _logger.LogInformation("{service}: {reason}", GetType().Name, GetReason(conResult));
                        await Task.Delay(_cooldown, _cancellationToken);
                        continue;
                    }
                    else _logger.LogInformation("{service}: connected", GetType().Name);

                    if (NeedSubscription()) await Subscribe();
                    await Task.Delay(Timeout.Infinite, _cancellationToken);
                }
                catch (OperationCanceledException) { break; }
                catch (OutOfMemoryException) { throw; }
                catch (Exception e)
                {
                    _logger.LogError(e, "Ошибка MQTT, реконнект...");
                    await Task.Delay(_cooldown, _cancellationToken);
                }
            }
        }

        private MqttProtocolVersion GetProtocol(string protocol)
        {
            if (!Enum.IsDefined(typeof(MqttProtocolVersion), protocol)) throw new NotSupportedException($"некорректный протокол mqtt {protocol}");
            else return Enum.Parse<MqttProtocolVersion>(protocol);
        }

        private string GetReason(MqttClientConnectResult conResult)
        {
            if (!string.IsNullOrEmpty(conResult.ReasonString)) return conResult.ReasonString;
            else return string.Format("код ошибки {0}", conResult.ResultCode);
        }

        protected virtual async Task Subscribe() 
        {
            if (_mqttClient == null) throw new ArgumentNullException(nameof(_mqttClient));

            var subscribeOptions = 
                _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(GetTopic()))
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions, _cancellationToken);
            _logger.LogInformation("{service}: подписка оформлена, ожидаем пакеты...", GetType().Name);
        }

        protected abstract Task OnReceive(MqttApplicationMessageReceivedEventArgs message);

        protected async Task PublishAsync(byte[] payload, CancellationToken cancellationToken)
        {
            if (_mqttClient == null) throw new ArgumentNullException(nameof(_mqttClient), "mqtt client  is null");
            await _mqttClient.PublishBinaryAsync(GetTopic(), payload, cancellationToken: cancellationToken);
        }
    }
}
