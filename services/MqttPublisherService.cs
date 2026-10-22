using meshbridge.configuration;
using meshbridge.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using System.Threading;
using System.Threading.Tasks;

namespace meshbridge.services
{
    public sealed class MqttPublisherService<T> : MqttBaseService where T : class
    {
        protected override string GetSectionName() => Constants.BrokerOutSectionName;

        protected override string GetTopic() => $"msh/EU_868/2/e/LongFast";

        protected override bool NeedSubscription() => false;

        protected override Task OnReceive(MqttApplicationMessageReceivedEventArgs message) => Task.CompletedTask;

        private readonly IMqttEgress<T> _processor;

        public MqttPublisherService(
            ILogger<MqttPublisherService<T>> logger,
            IOptionsMonitor<BrokerConfig> config,
            IMqttEgress<T> processor
        ) : base(logger, config)
        {
            _processor = processor;
        }

        public async Task Publish(T payload, CancellationToken cancellationToken)
        {
            byte[] raw = await _processor.Serialize(string.Empty, payload);
            await PublishAsync(raw, cancellationToken);
        }
    }
}