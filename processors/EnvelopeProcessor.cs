using Google.Protobuf;
using meshbridge.interfaces;
using Meshtastic.Protobufs;
using System;
using System.Threading.Tasks;

namespace meshbridge.processors
{
    public class EnvelopeProcessor : IMqttEgress<ServiceEnvelope>
    {
        public Task<byte[]> Serialize(string topic, ServiceEnvelope input) => Task.FromResult(input.ToByteArray());
    }
}
