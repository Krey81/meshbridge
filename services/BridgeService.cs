using meshbridge.classes;
using meshbridge.format;
using Meshtastic.Protobufs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace meshbridge.services
{
    /// <summary>
    /// Основной сервис приложения
    /// </summary>
    /// <remarks>Читает входящие сообщения из брокера на вход, выполняет преобразования, отправляет в брокер на выход. Периодически шлет NodeInfo</remarks>
    public class BridgeService : BackgroundService 
    {
        //TODO вынести в конфиг
        /// <summary>
        /// Задержка в случае проблем
        /// </summary>
        private readonly TimeSpan _cooldown = TimeSpan.FromSeconds(5);

        private readonly ILogger<BridgeService> _logger;

        private readonly ChannelReader<FormatSensorSpot> _reader;

        private readonly MqttPublisherService<ServiceEnvelope> _publisher;

        private readonly TimeSpan _nodeInfoSendPeriod = TimeSpan.FromMinutes(1); //10

        private ConcurrentDictionary<uint, NodeInfoSendingEntry> _sent = new();

        private CancellationToken _stoppingToken = CancellationToken.None;

        /// <summary>
        /// Конструктор по DI
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="reader"></param>
        /// <param name="publisher"></param>
        public BridgeService(
            ILogger<BridgeService> logger,
            ChannelReader<FormatSensorSpot> reader,
            MqttPublisherService<ServiceEnvelope> publisher
        )
        {
            _logger = logger;
            _reader = reader;
            _publisher = publisher;
        }

        ///<inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;

            var sendNodeInfoTask =  SendNodeInfoTaskAsync();
            var mainTask = MainLoop();
            await Task.WhenAll(sendNodeInfoTask, mainTask);
        }

        /// <summary>
        /// Фоновый таск для отсылки NodeInfo
        /// </summary>
        /// <returns></returns>
        private async Task SendNodeInfoTaskAsync()
        {
            using var timer = new PeriodicTimer(_nodeInfoSendPeriod);
            while (await timer.WaitForNextTickAsync(_stoppingToken))
            {
                DateTimeOffset now = DateTimeOffset.Now;

                var q =
                    from e in _sent.Values
                    where e.SentTime == null || e.SentTime < (now - _nodeInfoSendPeriod)
                    select e;

                foreach (var nodeInfoEntry in q) await SendNodeInfo(nodeInfoEntry);
            }
        }

        private async Task SendNodeInfo(NodeInfoSendingEntry sendingInfo)
        {
            try
            {
                await _publisher.Publish(EnvelopeConverter.NodeInfo(sendingInfo.NodeInfo), _stoppingToken);
                _logger.LogDebug(
                    "{service}: отправлен пакет NodeInfo для {user} ({nodeId})", 
                    nameof(BridgeService), 
                    sendingInfo.NodeInfo.LongName,
                    string.Format("{0:x4}", sendingInfo.NodeInfo.NodeId)
                );
            }
            finally
            {
                sendingInfo.SentTime = DateTimeOffset.Now;
            }
        }

        /// <summary>
        /// Рабочий цикл
        /// </summary>
        /// <returns></returns>
        private async Task MainLoop()
        {
            await foreach (var sensorSpot in _reader.ReadAllAsync(_stoppingToken))
            {
                ServiceEnvelope envelope;

                try
                {
                    envelope = await MapToEnvelope(sensorSpot);
                }
                catch (Exception e)
                {
                    _logger.LogError("{servicre}: ошибка преобразования в ServiceEnvelope: {err}", nameof(BridgeService), e.Message);
                    await Task.Delay(_cooldown, _stoppingToken);
                    continue;
                }

                try
                {
                    await _publisher.Publish(envelope, _stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError("{servicre}: не удалось отправить пакет, паблишер отвалился: {err}", nameof(BridgeService), e.Message);
                    await Task.Delay(_cooldown, _stoppingToken);
                    continue;
                }
            }
        }

        //TODO add generic helper
        //private async Task<ServiceEnvelope> TryMap()

        /// <summary>
        /// Конвертация форматов
        /// </summary>
        /// <param name="spot"></param>
        /// <returns></returns>
        private async Task<ServiceEnvelope> MapToEnvelope(FormatSensorSpot spot)
        {
            NodeInfoData nodeInfo = GetNodeInfo(spot);

            if (_sent.TryGetValue(nodeInfo.NodeId, out NodeInfoSendingEntry? entry)) entry.SeenTime = DateTime.Now;
            else
            {
                NodeInfoSendingEntry newentry = new NodeInfoSendingEntry(nodeInfo);
                _sent.TryAdd(nodeInfo.NodeId, newentry);
                await SendNodeInfo(newentry);
            }

            PositionData position = new PositionData()
            {
                NodeId = nodeInfo.NodeId,
                Lat = spot.Latitude,
                Lon = spot.Longitude
            };

            return EnvelopeConverter.Position(position);
        }

        /// <summary>
        /// Формирование NodeInfo
        /// </summary>
        /// <param name="spot"></param>
        /// <returns></returns>
        private NodeInfoData GetNodeInfo(FormatSensorSpot spot)
        {
            return new NodeInfoData()
            {
                NodeId = GetStableHash(spot.UserName),
                ShortName = spot.UserName.Substring(0, 3),
                LongName = spot.UserName
            };
        }

        /// <summary>
        /// простой стабильный алгоритм хэша
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static uint GetStableHash(string s)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in s) hash = (hash ^ c) * 16777619;
                return hash;
            }
        }
    }
}
