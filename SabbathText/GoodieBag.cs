namespace SabbathText
{
    using System;
    using KeyValueStorage;
    using QueueStorage;
    using SabbathText.Compensation.V1;
    using SabbathText.Entities;
    using SabbathText.V1;

    /// <summary>
    /// This class contains dependency goodies
    /// </summary>
    public class GoodieBag
    {
        private static KeyValueStore<AccountEntity> accountStore;
        private static KeyValueStore<MessageEntity> messageStore;
        private static KeyValueStore<Checkpoint> checkpointStore;
        private static EnvironmentSettings environmentSettings;
        private static CompensationClient compensationClient;
        private static QueueStore checkpointQueue;
        private static MessageClient messageClient;

        /// <summary>
        /// Hides the constructor
        /// </summary>
        protected GoodieBag()
        {
        }

        /// <summary>
        /// Gets or sets the function to override the <see cref="Create"/> method.
        /// </summary>
        public static Func<GoodieBag, GoodieBag> CreateFunc { get; set; }

        /// <summary>
        /// Gets or sets the account store.
        /// </summary>
        public KeyValueStore<AccountEntity> AccountStore { get; set; }

        /// <summary>
        /// Gets or sets the message store.
        /// </summary>
        public KeyValueStore<MessageEntity> MessageStore { get; set; }
        
        /// <summary>
        /// Gets or sets the checkpoint store.
        /// </summary>
        public KeyValueStore<Checkpoint> CheckpointStore { get; set; }

        /// <summary>
        /// Gets or sets the environment settings.
        /// </summary>
        public EnvironmentSettings Settings { get; set; }

        /// <summary>
        /// Gets or sets the compensation client.
        /// </summary>
        public CompensationClient CompensationClient { get; set; }

        /// <summary>
        /// Gets or sets the checkpoint queue.
        /// </summary>
        public QueueStore CheckpointQueue { get; set; }

        /// <summary>
        /// Gets or sets the message client.
        /// </summary>
        public MessageClient MessageClient { get; set; }

        /// <summary>
        /// Initializes the default goodie bag
        /// </summary>
        /// <param name="settings">The environment settings.</param>
        public static void Initialize(EnvironmentSettings settings)
        {
            environmentSettings = settings;
            accountStore = KeyValueStore<AccountEntity>.Create(settings.AccountStoreConfiguration);
            messageStore = KeyValueStore<MessageEntity>.Create(settings.MessageStoreConfiguration);
            checkpointStore = KeyValueStore<Checkpoint>.Create(settings.CheckpointStoreConfiguration);
            checkpointQueue = QueueStore.Create(settings.CheckpointQueueConfiguration);
            compensationClient = new CompensationClient(checkpointStore, checkpointQueue, settings.CheckpointVisibilityTimeout);
            messageClient = CreateMessageClient(settings);
        }

        /// <summary>
        /// Creates a new instance of the goodie bag.
        /// This method could be replaced by setting <see cref="CreateFunc"/>
        /// </summary>
        /// <returns>A new instance of the goodie bag.</returns>
        public static GoodieBag Create()
        {
            GoodieBag bag = new GoodieBag
            {
                AccountStore = accountStore,
                CheckpointQueue = checkpointQueue,
                CheckpointStore = checkpointStore,
                CompensationClient = compensationClient,
                Settings = environmentSettings,
                MessageClient = messageClient,
                MessageStore = messageStore,
            };

            if (CreateFunc == null)
            {
                return bag;
            }

            return CreateFunc(bag);
        }

        private static MessageClient CreateMessageClient(EnvironmentSettings settings)
        {
            switch (settings.MessageClientType)
            {
                case MessageClientType.Twilio:
                    MessageClient msgClient = new MessageClient();
                    msgClient.InitTwilio(settings.TwilioAccount, settings.TwilioToken, settings.ServicePhoneNumber);
                    return msgClient;
                default:
                    InMemoryMessageClient inMemoryClient = new InMemoryMessageClient();
                    inMemoryClient.InitMemory();
                    return inMemoryClient;
            }
        }
    }
}
