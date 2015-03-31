namespace KeyValueStorage.Tests
{
    using System;
    using KeyValueStorage.Tests.Fixtures;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// This class tests the KeyValueStore
    /// </summary>
    [TestClass]
    public class KeyValueStoreTests
    {
        /// <summary>
        /// The connection string
        /// </summary>
        private const string ConnectionString = "UseDevelopmentStorage=true";

        /// <summary>
        /// The table name
        /// </summary>
        private string tableName = "test";

        /// <summary>
        /// Gets or sets the store used for testing
        /// </summary>
        protected KeyValueStore<Dog> Store { get; set; }

        /// <summary>
        /// This method will be called before every test run
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            this.InitStore();
        }

        /// <summary>
        /// This method will be called after every test run
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            this.CleanUpStore();
        }

        /// <summary>
        /// Tests insert
        /// </summary>
        [TestMethod]
        public void TestInsert()
        {
            Dog dog = new Dog
            {
                Birthday = Clock.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs:buddy",
                Weight = 98.4f,
            };

            this.Store.Insert(dog).Wait();
            Assert.IsNotNull(dog.ETag, "ETag should be populated after insertion");
        }

        /// <summary>
        /// Tests insert when inserting objects with a duplicate keys
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DuplicateKeyException), "Insert expects a duplicate key exception")]
        public void InsertShouldThrowDuplicateKeyException()
        {
            Dog dog = new Dog
            {
                Birthday = Clock.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs:buddy",
                Weight = 98.4f,
            };

            this.Store.Insert(dog).Wait();

            Dog elf = new Dog
            {
                Birthday = Clock.UtcNow.AddYears(-1),
                Breed = DogBreed.GermanShepherd,
                Name = "Elf",
                PartitionKey = "B",
                RowKey = "dogs:buddy",
                Weight = 60f,
            };

            try
            {
                this.Store.Insert(elf).Wait();
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException != null)
                {
                    throw ae.InnerException;
                }

                throw;
            }
        }

        /// <summary>
        /// Tests get and make sure the properties match
        /// </summary>
        [TestMethod]
        public void TestGet()
        {
            Dog dog = new Dog
            {
                Birthday = Clock.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs:buddy",
                Weight = 98.4f,
            };

            this.Store.Insert(dog).Wait();

            Dog buddy = this.Store.Get("B", "dogs:buddy").Result;

            Assert.IsNotNull(buddy, "Get should not return null");
            Assert.AreEqual(dog.Birthday, buddy.Birthday);
            Assert.AreEqual(dog.Breed, buddy.Breed);
            Assert.AreEqual(dog.Name, buddy.Name);
            Assert.AreEqual(dog.PartitionKey, buddy.PartitionKey);
            Assert.AreEqual(dog.RowKey, buddy.RowKey);
            Assert.AreEqual(dog.Weight, buddy.Weight);
            Assert.AreEqual(dog.ETag, buddy.ETag);
            Assert.AreEqual(dog.Timestamp, buddy.Timestamp);
        }

        /// <summary>
        /// Tests that Get returns null when the entity does not exist
        /// </summary>
        [TestMethod]
        public void GetShouldReturnNullWhenEntityDoesNotExist()
        {
            Assert.IsNull(this.Store.Get("partitionKey", "rowKey").Result, "Get should return null when entity does not exist");
        }

        /// <summary>
        /// Tests update
        /// </summary>
        [TestMethod]
        public void TestUpdate()
        {
            Dog dog = new Dog
            {
                Birthday = Clock.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs:buddy",
                Weight = 98.4f,
            };

            this.Store.Insert(dog).Wait();

            Dog buddy = this.Store.Get("B", "dogs:buddy").Result;

            buddy.Weight += 5f;
            this.Store.Update(buddy).Wait();

            Dog newBuddyWithNewWeight = this.Store.Get("B", "dogs:buddy").Result;

            Assert.IsTrue(newBuddyWithNewWeight.Weight > dog.Weight, "Weight is expected to increase");
        }

        /// <summary>
        /// Tests update when a different process has modified the object
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ETagMismatchException), "An ETagMismatchException is expected in this test")]
        public void UpdateShouldThrowETagMismatchException()
        {
            Dog dog = new Dog
            {
                Birthday = Clock.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs:buddy",
                Weight = 98.4f,
            };

            this.Store.Insert(dog).Wait();

            Dog buddy = this.Store.Get("B", "dogs:buddy").Result;
            buddy.Weight += 5f;
            this.Store.Update(buddy).Wait();

            dog.Weight += 3f;

            try
            {
                this.Store.Update(dog).Wait();
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException != null)
                {
                    throw ae.InnerException;
                }

                throw;
            }
        }

        /// <summary>
        /// Tests update when the object does not exist
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EntityNotFoundException), "An EntityNotFoundException is expected in this test")]
        public void UpdateShouldThrowEntityNotFoundException()
        {
            Dog dog = new Dog
            {
                Birthday = Clock.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs:buddy",
                Weight = 98.4f,
                ETag = "*",
                Timestamp = Clock.UtcNow,
            };

            dog.Weight += 3f;

            try
            {
                this.Store.Update(dog).Wait();
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException != null)
                {
                    throw ae.InnerException;
                }

                throw;
            }
        }

        /// <summary>
        /// Tests delete
        /// </summary>
        [TestMethod]
        public void TestDelete()
        {
            Dog dog = new Dog
            {
                Birthday = Clock.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs:buddy",
                Weight = 98.4f,
            };

            this.Store.Insert(dog).Wait();
            Assert.IsNotNull(this.Store.Get(dog.PartitionKey, dog.RowKey).Result, "Expected to get an object after insertion");

            this.Store.Delete(dog).Wait();
            Assert.IsNull(this.Store.Get(dog.PartitionKey, dog.RowKey).Result, "Expected null after the entity had been deleted");
        }

        /// <summary>
        /// Tests that Delete throws ETagMismatchException if the entity is modified before deletion
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ETagMismatchException), "ETagMismatchException is expected if the entity is modified before deletion")]
        public void DeleteShouldThrowETagMismatchException()
        {
            Dog dog = new Dog
            {
                Birthday = Clock.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs:buddy",
                Weight = 98.4f,
            };

            this.Store.Insert(dog).Wait();

            Dog buddy = this.Store.Get(dog.PartitionKey, dog.RowKey).Result;
            
            this.Store.Update(dog).Wait(); // some other process modified the entity

            try
            {
                this.Store.Delete(buddy).Wait();
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException != null)
                {
                    throw ae.InnerException;
                }

                throw;
            }
        }

        /// <summary>
        /// Tests that Delete throws EntityNotFoundException if the entity does not exist
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(EntityNotFoundException), "EntityNotFoundException is expected if the entity does not exist")]
        public void DeleteShouldThrowEntityNotFoundException()
        {
            Dog dog = new Dog
            {
                Birthday = Clock.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs:buddy",
                Weight = 98.4f,
                ETag = "*",
                Timestamp = Clock.UtcNow,
            };
            
            try
            {
                this.Store.Delete(dog).Wait();
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException != null)
                {
                    throw ae.InnerException;
                }

                throw;
            }
        }
        
        /// <summary>
        /// Reset the Azure table store
        /// </summary>
        protected virtual void InitStore()
        {
            this.tableName = "test" + Guid.NewGuid().ToString("N").Substring(0, 8);

            KeyValueStore<Dog> azureTableStore = new KeyValueStore<Dog>();
            azureTableStore.InitAzureTable(ConnectionString, this.tableName);
            this.Store = azureTableStore;
        }

        /// <summary>
        /// Clean up the table after test run
        /// </summary>
        protected virtual void CleanUpStore()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(ConnectionString);
            CloudTableClient client = account.CreateCloudTableClient();
            CloudTable table = client.GetTableReference(this.tableName);
            table.DeleteIfExists();
        }
    }
}
