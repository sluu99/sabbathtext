namespace KeyValueStorage.Tests.Fixtures
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

    /// <summary>
    /// Dog breeds
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1602:EnumerationItemsMustBeDocumented", Justification = "Reviewed")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DogBreed
    {
        GermanShepherd,
        GoldenRetriever,
        Labrador,
        SiberianHusky,
        Bulldog,
        Dobermann,
        GreatDane,
        Pug,
        Beagle,
        Boxer,
    }

    /// <summary>
    /// This is an entity class used for testing KeyValueStore
    /// </summary>
    public class Dog : KeyValueEntity
    {
        /// <summary>
        /// Gets or sets name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets birthday
        /// </summary>
        public DateTime Birthday { get; set; }

        /// <summary>
        /// Gets or sets breed
        /// </summary>
        public DogBreed Breed { get; set; }

        /// <summary>
        /// Gets or sets weight
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Gets or sets the partition key
        /// </summary>
        public string PK { get; set; }

        /// <summary>
        /// Gets or sets the row key
        /// </summary>
        public string RK { get; set; }

        /// <summary>
        /// Gets the partition key.
        /// </summary>
        public override string PartitionKey
        {
            get { return this.PK; }
        }

        /// <summary>
        /// Gets the row key.
        /// </summary>
        public override string RowKey
        {
            get { return this.RK; }
        }
    }
}
