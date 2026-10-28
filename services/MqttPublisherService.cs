using meshbridge.configuration;
using meshbridge.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using System;
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
            try
            {
                byte[] raw = await _processor.Serialize(string.Empty, payload);
                if (raw.Length == 0) throw new Exception(string.Format("0 байт получено от процессора {0}", _processor.GetType().Name));

                await PublishAsync(raw, cancellationToken);
                _logger.LogDebug("{service}: отправлено {cnt} байт", GetType().Name, raw.Length);
            }
            catch (Exception e)
            {
                _logger.LogError("{service}: ошибка отправки '{err}'", GetType().Name, e.Message);
                throw;
            }
        }
    }
}