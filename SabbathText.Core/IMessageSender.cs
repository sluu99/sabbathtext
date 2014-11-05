using System.Threading.Tasks;

namespace SabbathText.Core
{
    public interface IMessageSender
    {
        Task Send(Entities.Message message);
    }
}
