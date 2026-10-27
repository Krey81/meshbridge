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
    //TODO сменить патерн на SoftWires
    /// <summary>
    /// Базовый класс для работы с mqtt
    /// </summary>
    public abstract class MqttBaseService : BackgroundService
    {
        /// <summary>
        /// Возвращает наименование секции конфигурации для настройки брокера
        /// </summary>
        /// <returns></returns>
        protected abstract string GetSectionName();

        /// <summary>
        /// Возвращает наименование топика mqtt
        /// </summary>
        /// <returns></returns>
        protected abstract string GetTopic();

        /// <summary>
        /// Определяет должен ли потомок подписываться на указанный топик
        /// </summary>
        /// <returns></returns>
        protected abstract bool NeedSubscription();

        //TODO вынести в конфиг
        /// <summary>
        /// Задержка в случае проблем
        /// </summary>
        protected readonly TimeSpan _cooldown = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Логер
        /// </summary>
        protected readonly ILogger _logger;

        private readonly MqttClientFactory _mqttFactory = new MqttClientFactory();

        private IMqttClient? _mqttClient;

        //TODO сменить на токенсорс
        /// <summary>
        /// Токен отмены сервиса
        /// </summary>
        protected CancellationToken _cancellationToken = CancellationToken.None;

        /// <summary>
        /// Итоговые настройки брокера
        /// </summary>
        protected readonly BrokerConfig _brokerConfig;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        public MqttBaseService(
            ILogger logger,
            IOptionsMonitor<BrokerConfig> config
        )
        {
            _logger = logger;
            _brokerConfig = config.Get(GetSectionName());
        }

        ///<inheritdoc/>
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
                    else _logger.LogInformation("{service}: connected to {host}:{port}", GetType().Name, _brokerConfig.Host, _brokerConfig.Port);

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

        /// <summary>
        /// Метод подписки
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected virtual async Task Subscribe() 
        {
            if (_mqttClient == null) throw new ArgumentNullException(nameof(_mqttClient));
            string topic = GetTopic();

            var subscribeOptions = 
                _mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(topic))
                .Build();

            await _mqttClient.SubscribeAsync(subscribeOptions, _cancellationToken);
            _logger.LogInformation("{service}: подписка на {topic} оформлена , ожидаем пакеты...", GetType().Name, topic);
        }

        /// <summary>
        /// Метод обработки полученных данных
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        protected abstract Task OnReceive(MqttApplicationMessageReceivedEventArgs message);

        /// <summary>
        /// Метод публикации
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected async Task PublishAsync(byte[] payload, CancellationToken cancellationToken)
        {
            if (_mqttClient == null) throw new ArgumentNullException(nameof(_mqttClient), "mqtt client  is null");
            await _mqttClient.PublishBinaryAsync(GetTopic(), payload, cancellationToken: cancellationToken);
        }
    }
}
