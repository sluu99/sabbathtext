using SabbathText.Core.Entities;
using System.Threading.Tasks;

namespace SabbathText.Core.Backend
{
    public interface IProcessor
    {
        Task<bool> ProcessMessage(Message message);
    }
}
