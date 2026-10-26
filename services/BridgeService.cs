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
    public class BridgeService : BackgroundService 
    {
        private readonly ILogger<BridgeService> _logger;

        private readonly ChannelReader<FormatSensorSpot> _reader;

        private readonly MqttPublisherService<ServiceEnvelope> _publisher;

        private readonly TimeSpan _nodeInfoSendPeriod = TimeSpan.FromMinutes(1); //10

        private ConcurrentDictionary<uint, NodeInfoSendingEntry> _sent = new();

        private CancellationToken _stoppingToken = CancellationToken.None;

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _stoppingToken = stoppingToken;

            var sendNodeInfoTask =  SendNodeInfoTaskAsync();
            var mainTask = MainLoop();
            await Task.WhenAll(sendNodeInfoTask, mainTask);
        }

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
                _logger.LogDebug("{service}: node info for {user} sent", nameof(BridgeService), sendingInfo.NodeInfo.LongName);
            }
            finally
            {
                sendingInfo.SentTime = DateTimeOffset.Now;
            }
        }

        private async Task MainLoop()
        {
            await foreach (var sensorSpot in _reader.ReadAllAsync(_stoppingToken))
            {
                ServiceEnvelope envelope = await MapToEnvelope(sensorSpot);
                await _publisher.Publish(envelope, _stoppingToken);
            }
        }

        private async Task<ServiceEnvelope> MapToEnvelope(FormatSensorSpot spot)
        {

            NodeInfoData nodeInfo = GetNodeInfo(spot);
            ;
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

        private NodeInfoData GetNodeInfo(FormatSensorSpot spot)
        {
            return new NodeInfoData()
            {
                NodeId = Convert.ToUInt32(spot.UserName.GetHashCode()),
                ShortName = spot.UserName.Substring(0, 3),
                LongName = spot.UserName
            };
        }
    }
}
