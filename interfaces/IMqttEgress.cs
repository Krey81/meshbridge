using System.Threading.Tasks;

namespace meshbridge.interfaces
{
    public interface IMqttEgress<T>
    {
        Task<byte[]> Serialize(string topic, T input);
    }
}
