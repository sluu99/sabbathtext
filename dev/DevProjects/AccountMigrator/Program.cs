using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using SabbathText;
using SabbathText.Entities;

namespace AccountMigrator
{
    class Program
    {
        static void Main(string[] args)
        {
            GoodieBag.Initialize(new MigrationEnvironmentSettings());
            GoodieBag bag = GoodieBag.Create();

            CloudStorageAccount oldCloudAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("OLD_ST_CONN_STR"));
            CloudTableClient oldTableClient = oldCloudAccount.CreateCloudTableClient();
            CloudTable oldTable = oldTableClient.GetTableReference("accounts");

            var tableQuery = oldTable.CreateQuery<DynamicTableEntity>();
            var result = oldTable.ExecuteQuery(tableQuery).ToList();


            if (result != null && result.Count > 0)
            {
                foreach (var entity in result)
                {
                    string phoneNumber = entity.Properties["PhoneNumber"].StringValue;
                    DateTime creationTime = new DateTime(entity.Properties["CreationTime"].DateTime.Value.Ticks, DateTimeKind.Utc);
                    DateTime lastSabbathTextTime = DateTime.MinValue;
                    if (entity.Properties["LastSabbathMessageTime"].DateTime != null)
                    {
                        lastSabbathTextTime = new DateTime(entity.Properties["LastSabbathMessageTime"].DateTime.Value.Ticks, DateTimeKind.Utc);
                    }
                    string recentVerses = null;
                    if (entity.Properties.ContainsKey("RecentlySentVerses"))
                    {
                        recentVerses = entity.Properties["RecentlySentVerses"].StringValue;
                    }
                    string status = entity.Properties["Status"].StringValue;
                    string zipCode = null;
                    if (entity.Properties.ContainsKey("ZipCode"))
                    {
                        zipCode = entity.Properties["ZipCode"].StringValue;
                    }

                    AccountEntity account = new AccountEntity
                    {
                        AccountId = AccountEntity.GetAccountIdByPhoneNumber(phoneNumber),
                        ConversationContext = ConversationContext.Unknown,
                        CreationTime = creationTime,
                        HasBeenGreeted = true,
                        LastSabbathTextTime = lastSabbathTextTime,
                        PhoneNumber = phoneNumber,
                        RecentMessages = new List<MessageEntity>(0),
                        RecentVerses = string.IsNullOrWhiteSpace(recentVerses) ? new List<string>(0) : new List<string>(recentVerses.Split(';')),
                        ZipCode = zipCode,
                    };

                    account.Status = AccountStatus.BrandNew;
                    if (status == "Subscribed")
                    {
                        account.Status = AccountStatus.Subscribed;
                    }
                    else if (status == "Unsubscribed")
                    {
                        account.Status = AccountStatus.Unsubscribed;
                    }

                    bag.AccountStore.InsertOrGet(account, CancellationToken.None).Wait();
                    Console.WriteLine("Migrated account " + account.AccountId);
                }
            }
        }
    }
}
