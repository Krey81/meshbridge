using System.Buffers;
using System.Threading.Tasks;

namespace meshbridge.interfaces
{
    /// <summary>
    /// Интерфейс для входящих mqtt
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMqttIngress<T>
    {
        /// <summary>
        /// Десериализация
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        Task<T> Deserialize(string topic, ReadOnlySequence<byte> payload);
    }
}
