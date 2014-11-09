using System.Threading.Tasks;

namespace SabbathText.Core
{
    public interface IMessageSender
    {
        Task<string> Send(Entities.Message message);
    }
}
