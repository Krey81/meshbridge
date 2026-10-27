using meshbridge.configuration;
using meshbridge.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using System.Threading;
using System.Threading.Tasks;

namespace meshbridge.services
{
    /// <summary>
    /// Сервис публикации в mqtt
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class MqttPublisherService<T> : MqttBaseService where T : class
    {
        ///<inheritdoc/>
        protected override string GetSectionName() => Constants.BrokerOutSectionName;

        ///<inheritdoc/>
        protected override string GetTopic() => $"msh/EU_868/2/e/LongFast";

        ///<inheritdoc/>
        protected override bool NeedSubscription() => false;

        ///<inheritdoc/>
        protected override Task OnReceive(MqttApplicationMessageReceivedEventArgs message) => Task.CompletedTask;

        private readonly IMqttEgress<T> _processor;

        /// <summary>
        /// Конструктор по DI
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="processor"></param>
        public MqttPublisherService(
            ILogger<MqttPublisherService<T>> logger,
            IOptionsMonitor<BrokerConfig> config,
            IMqttEgress<T> processor
        ) : base(logger, config)
        {
            _processor = processor;
        }

        /// <summary>
        /// Метод публикации
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Publish(T payload, CancellationToken cancellationToken)
        {
            byte[] raw = await _processor.Serialize(string.Empty, payload);
            await PublishAsync(raw, cancellationToken);
        }
    }
}