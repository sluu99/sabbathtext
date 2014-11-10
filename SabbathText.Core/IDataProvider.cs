using SabbathText.Core.Entities;
using System;
using System.Threading.Tasks;

namespace SabbathText.Core
{
    public interface IDataProvider
    {
        Task<Account> GetAccountByPhoneNumber(string phoneNumber);
        Task CreateAccountWithPhoneNumber(string phoneNumber);
        Task UpdateAccount(Account account);

        Task RecordMessage(string accountId, Message message);
        Task RecordPoisonMessage(string queueName, string message);
        Task<int> CountPoisonMessages();

        Task<Location> GetLocationByZipCode(string zipCode);
        Task<LocationTimeInfo> GetTimeInfoByZipCode(string zipCode, DateTime date);

        Task<KeyValue> GetKeyValue(string key);
        Task PutKeyValue(KeyValue keyValue);
    }
}
