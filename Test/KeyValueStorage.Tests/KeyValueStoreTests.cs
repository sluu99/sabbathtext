namespace KeyValueStorage.Tests
{
    using System;
    using KeyValueStorage.Tests.Fixtures;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// This class tests the KeyValueStore
    /// </summary>
    [TestClass]
    public class KeyValueStoreTests
    {
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
            this.ResetStore();
        }

        /// <summary>
        /// Tests insert
        /// </summary>
        [TestMethod]
        public void TestInsert()
        {
            Dog dog = new Dog
            {
                Birthday = DateTime.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs/buddy",
                Weight = 98.4f,
            };

            this.Store.Insert(dog).Wait();
        }

        /// <summary>
        /// Test insert when inserting objects with a duplicate keys
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(DuplicateKeyException), "Insert expects a duplicate key exception")]
        public void InsertShouldThrowDuplicateKeyException()
        {
            Dog dog = new Dog
            {
                Birthday = DateTime.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs/buddy",
                Weight = 98.4f,
            };

            this.Store.Insert(dog).Wait();

            Dog elf = new Dog
            {
                Birthday = DateTime.UtcNow.AddYears(-1),
                Breed = DogBreed.GermanShepherd,
                Name = "Elf",
                PartitionKey = "B",
                RowKey = "dogs/buddy",
                Weight = 60f,
            };

            this.Store.Insert(elf).Wait();
        }

        /// <summary>
        /// Test get and make sure the properties match
        /// </summary>
        [TestMethod]
        public void TestGet()
        {
            Dog dog = new Dog
            {
                Birthday = DateTime.UtcNow.AddYears(-2),
                Breed = DogBreed.Labrador,
                Name = "Buddy",
                PartitionKey = "B",
                RowKey = "dogs/buddy",
                Weight = 98.4f,
            };

            this.Store.Insert(dog).Wait();

            Dog buddy = this.Store.Get("B", "dogs/buddy").Result;

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
        /// Reset the store used for testing
        /// </summary>
        protected virtual void ResetStore()
        {
            this.Store = new KeyValueStore<Dog>();
        }
    }
}
