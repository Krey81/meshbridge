using System.Buffers;
using System.Threading.Tasks;

namespace meshbridge.interfaces
{
    public interface IMqttIngress<T>
    {
        Task<T> Deserialize(string topic, ReadOnlySequence<byte> payload);
    }
}
