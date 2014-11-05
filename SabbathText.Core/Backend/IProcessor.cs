using SabbathText.Core.Entities;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend
{
    public interface IProcessor
    {
        Task<TemplatedMessage> ProcessMessage(Message message);
    }
}
