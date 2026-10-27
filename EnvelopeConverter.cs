using Google.Protobuf;
using meshbridge.classes;
using Meshtastic.Protobufs;
using System;

namespace meshbridge
{
    /// <summary>
    /// Конвертер в ServiceEnvelope protobuf
    /// </summary>
    public static class EnvelopeConverter
    {
        /// <summary>
        /// Формирует ServiceEnvelope из NodeInfoData
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ServiceEnvelope NodeInfo(NodeInfoData args)
        {
            User user = new User()
            {
                Id = string.Format("!{0:x8}", args.NodeId),
                ShortName = args.ShortName,
                LongName = args.LongName
            };

            Data data = new()
            {
                Portnum = PortNum.NodeinfoApp,
                Payload = user.ToByteString()
            };

            MeshPacket packet = new()
            {
                From = args.NodeId,
                To = 0xFFFFFFFF,   // Broadcast
                Channel = 0,       // Индекс канала (должен совпадать с настройками MeshMonitor)
                Id = (uint)new Random().Next(1, int.MaxValue),
                // ВАЖНО: Если канал зашифрован, сюда нужно положить зашифрованные байты data.ToByteArray(), а не само data.
                Decoded = data
            };

            return Envelope(packet);
        }

        /// <summary>
        /// Формирует ServiceEnvelope из PositionData
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static ServiceEnvelope Position(PositionData args)
        {
            Position pos = new()
            {
                LatitudeI = (int)(args.Lat * 1e7),
                LongitudeI = (int)(args.Lon * 1e7),
                Time = (uint)DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            Data data = new()
            {
                Portnum = PortNum.PositionApp,
                Payload = pos.ToByteString()
            };

            MeshPacket packet = new()
            {
                From = args.NodeId,
                To = 0xFFFFFFFF,   // Broadcast
                Channel = 0,       // Индекс канала (должен совпадать с настройками MeshMonitor)
                Id = (uint)new Random().Next(1, int.MaxValue),
                // ВАЖНО: Если канал зашифрован, сюда нужно положить зашифрованные байты data.ToByteArray(), а не само data.
                Decoded = data
            };

            return Envelope(packet);
        }

        //TODO посмотреть что еще полезного можно впендюрить
        /// <summary>
        /// Добавление прочей инфы в ServiceEnvelope
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        private static ServiceEnvelope Envelope(MeshPacket packet)
        {
            return new()
            {
                Packet = packet,
                ChannelId = "LongFast",
                GatewayId = "MyPhoneGateway"
            };
        }
    }
}
