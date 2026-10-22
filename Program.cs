using Google.Protobuf;
using meshbridge.classes;
using Meshtastic.Protobufs;
using System;
namespace meshbridge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            NodeInfoData nodeInfo = new()
            {
                NodeId = 0xA1B2C3D4,
                ShortName = "krv",
                LongName = "Krey81-rover"
            };

            //смотровая
            PositionData position = new PositionData()
            {
                NodeId = nodeInfo.NodeId,
                Lat = 56.371675,
                Lon = 36.968860
            };

            Console.WriteLine(Convert.ToHexString(EnvelopeConverter.NodeInfo(nodeInfo).ToByteArray()));
            Console.WriteLine(Convert.ToHexString(EnvelopeConverter.Position(position).ToByteArray()));

            ;

        }
    }
}
