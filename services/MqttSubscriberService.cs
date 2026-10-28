using meshbridge.configuration;
using meshbridge.interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace meshbridge.services
{
    /// <summary>
    /// Сервис подписки mqtt
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class MqttSubscriberService<T> : MqttBaseService where T : class, IUserData
    {
        ///<inheritdoc/>
        protected override string GetSectionName() => Constants.BrokerInSectionName;

        ///<inheritdoc/>
        protected override string GetTopic() => "android/sensor/+/gps";

        ///<inheritdoc/>
        protected override bool NeedSubscription() => true;

        private readonly IMqttIngress<T> _processor;

        private readonly ChannelWriter<T> _writer;

        /// <summary>
        /// Конструктор по DI
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="processor"></param>
        /// <param name="writer"></param>
        public MqttSubscriberService(
            ILogger<MqttSubscriberService<T>> logger,
            IOptionsMonitor<BrokerConfig> config,
            IMqttIngress<T> processor,
            ChannelWriter<T> writer
        ) : base(logger, config)
        {
            _processor = processor;
            _writer = writer;
        }

        ///<inheritdoc/>
        protected override async Task OnReceive(MqttApplicationMessageReceivedEventArgs message)
        {
            try
            {
                string topic = message.ApplicationMessage.Topic;

                string user;
                if (!CheckTopic(message.ApplicationMessage.Topic, out user)) return;

                T dataIn = await _processor.Deserialize(topic, message.ApplicationMessage.Payload);

                dataIn.UserName = user;
                _logger.LogDebug("{service}: получен пакет от {user} '{descr}'", GetType().Name, user, dataIn.ToString());
                await _writer.WriteAsync(dataIn);
            }
            catch (Exception e)
            {
                _logger.LogError("{setvice}: Ошибка обработки нагрузки: {err}", nameof(MqttSubscriberService<>), e.Message);
                await Task.Delay(_cooldown, _cancellationToken);
            }
        }

        private bool CheckTopic(string topic, out string user)
        {
            string[] parts = topic.Split('/');
            if (parts.Length == 4)
            {
                if (parts[0].Equals("android") && parts[1].Equals("sensor") && parts[3].Equals("gps"))
                {
                    user = parts[2];
                    return true;
                }
            }
            user = string.Empty;
            return false;
        }
    }
}
