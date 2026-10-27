using Google.Protobuf;
using meshbridge.interfaces;
using Meshtastic.Protobufs;
using System.Threading.Tasks;

namespace meshbridge.processors
{
    /// <summary>
    /// Процессор для ServiceEnvelope
    /// </summary>
    public class EnvelopeProcessor : IMqttEgress<ServiceEnvelope>
    {
        ///<inheritdoc/>
        public Task<byte[]> Serialize(string topic, ServiceEnvelope input) => Task.FromResult(input.ToByteArray());
    }
}
