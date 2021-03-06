﻿namespace SabbathText.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Newtonsoft.Json;
    using SabbathText.Entities;

    /// <summary>
    /// A class used for tracking telemetry
    /// </summary>
    public class TelemetryTracker
    {
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Hide the constructor
        /// </summary>
        protected TelemetryTracker()
        {
        }

        /// <summary>
        /// Creates a new telemetry tracker.
        /// </summary>
        /// <param name="config">The tracker configuration.</param>
        /// <returns>A new telemetry tracker.</returns>
        public static TelemetryTracker Create(TelemetryTrackerConfiguration config)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.AzureAppInsightsInstrumentationKey))
            {
                return new TracingTelemetryTracker();
            }

            TelemetryTracker tracker = new TelemetryTracker();
            tracker.InitAzureAppInsights(config.AzureAppInsightsInstrumentationKey);

            return tracker;
        }

        /// <summary>
        /// Initializes the telemetry tracker for Azure Application Insights
        /// </summary>
        /// <param name="instrumentationKey">The instrumentation key.</param>
        public void InitAzureAppInsights(string instrumentationKey)
        {
            this.telemetryClient = new TelemetryClient();
            this.telemetryClient.InstrumentationKey = instrumentationKey;
        }

        /// <summary>
        /// Tracks an event
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">The optional event properties.</param>
        /// <param name="metrics">The optional event metrics.</param>
        protected virtual void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.telemetryClient.TrackEvent(eventName, properties, metrics);
        }

        /// <summary>
        /// Tracks an exception.
        /// </summary>
        /// <param name="exception">The exception to track.</param>
        /// <param name="properties">The optional event properties.</param>
        /// <param name="metrics">The optional event metrics.</param>
        protected virtual void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.telemetryClient.TrackException(exception, properties, metrics);
        }

        /// <summary>
        /// Tracks that a message is sent.
        /// </summary>
        /// <param name="template">The message template.</param>
        /// <param name="content">The message content</param>
        /// <param name="trackingId">The tracking ID.</param>
        public void MessageSent(MessageTemplate template, string content, string trackingId)
        {
            this.TrackEvent(
                "MessageSent",
                new Dictionary<string, string>
                {
                    { "MessageTemplate", template.ToString() },
                    { "Content", content },
                    { "TrackingId", trackingId },
                });
        }

        /// <summary>
        /// Track that a message is skipped.
        /// </summary>
        /// <param name="template">The message template.</param>
        /// <param name="trackingId">The tracking ID.</param>
        public void MessageSkipped(MessageTemplate template, string trackingId)
        {
            this.TrackEvent(
                "MessageSkipped",
                new Dictionary<string, string>
                {
                    { "MessageTemplate", template.ToString() },
                    { "TrackingId", trackingId },
                });
        }

        /// <summary>
        /// Tracks that a checkpoint is being processed.
        /// </summary>
        /// <param name="partitionKey">The checkpoint partition key.</param>
        /// <param name="rowKey">The checkpoint row key.</param>
        /// <param name="checkpointType">The checkpoint type.</param>
        public void ProcessingCheckpoint(string partitionKey, string rowKey, string checkpointType)
        {
            this.TrackEvent(
                "ProcessingCheckpoint",
                new Dictionary<string, string>
                {
                    { "PartitionKey", partitionKey },
                    { "RowKey", rowKey },
                    { "CheckpointType", checkpointType },
                });
        }

        /// <summary>
        /// Tracks that a checkpoint is processed.
        /// </summary>
        /// <param name="partitionKey">The checkpoint partition key.</param>
        /// <param name="rowKey">The checkpoint row key.</param>
        /// <param name="checkpointType">The checkpoint type.</param>
        /// <param name="duration">The time it took to complete the checkpoint.</param>
        public void CompletedCheckpoint(string partitionKey, string rowKey, string checkpointType, TimeSpan duration)
        {
            this.TrackEvent(
                "CompletedCheckpoint",
                new Dictionary<string, string>
                {
                    { "PartitionKey", partitionKey },
                    { "RowKey", rowKey },
                    { "CheckpointType", checkpointType },
                },
                new Dictionary<string, double>
                {
                    { "Duration", duration.TotalMilliseconds },
                });
        }

        /// <summary>
        /// Tracks that a checkpoint is being marked as cancelling.
        /// </summary>
        /// <param name="partitionKey">The checkpoint partition key.</param>
        /// <param name="rowKey">The checkpoint row key.</param>
        /// <param name="checkpointType">The checkpoint type.</param>
        public void CancellingCheckpoint(string partitionKey, string rowKey, string checkpointType)
        {
            this.TrackEvent(
                "CancellingCheckpoint",
                new Dictionary<string, string>
                {
                    { "PartitionKey", partitionKey },
                    { "RowKey", rowKey },
                    { "CheckpointType", checkpointType },
                });
        }

        /// <summary>
        /// Tracks that a checkpoint is being extended to process at a later time.
        /// </summary>
        /// <param name="partitionKey">The checkpoint partition key.</param>
        /// <param name="rowKey">The checkpoint row key.</param>
        /// <param name="checkpointType">The checkpoint type.</param>
        /// <param name="processTime">The intended checkpoint process time.</param>
        public void ExtendingCheckpoint(string partitionKey, string rowKey, string checkpointType, DateTime processTime)
        {
            this.TrackEvent(
                "ExtendingCheckpoint",
                new Dictionary<string, string>
                {
                    { "PartitionKey", partitionKey },
                    { "RowKey", rowKey },
                    { "CheckpointType", checkpointType },
                    { "ProcessTime", processTime.ToString() },
                });
        }

        /// <summary>
        /// Tracks that some exception occurred when processing a checkpoint.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="partitionKey">The checkpoint partition key.</param>
        /// <param name="rowKey">The checkpoint row key.</param>
        /// <param name="checkpointType">The checkpoint type.</param>
        public void ProcessCheckpointException(Exception exception, string partitionKey, string rowKey, string checkpointType)
        {
            this.TrackException(
                exception,
                new Dictionary<string, string>
                {
                    { "PartitionKey", partitionKey },
                    { "RowKey", rowKey },
                    { "CheckpointType", checkpointType },
                });
        }

        /// <summary>
        /// Tracks that a Bible verse has been reserved.
        /// </summary>
        /// <param name="bibleVerse">The reserved Bible verse.</param>
        public void BibleVerseReserved(string bibleVerse)
        {
            this.TrackEvent(
                "BibleVerseReserved",
                new Dictionary<string, string>
                {
                    { "BibleVerse", bibleVerse },
                });
        }

        /// <summary>
        /// Tracks that a Sabbath text was sent
        /// </summary>
        /// <param name="bibleVerse">The reserved Bible verse.</param>
        /// <param name="zipCode">The ZIP code of the recipient.</param>
        public void SabbathTextSent(string bibleVerse, string zipCode)
        {
            this.TrackEvent(
                "SabbathTextSent",
                new Dictionary<string, string>
                {
                    { "BibleVerse", bibleVerse },
                    { "ZipCode", zipCode },
                });
        }

        /// <summary>
        /// Tracks that a message is processed.
        /// </summary>
        /// <param name="sender">The message sender.</param>
        /// <param name="content">The message content.</param>
        /// <param name="duration">The time it took to process the message.</param>
        public void MessageProcessed(string sender, string content, TimeSpan duration)
        {
            this.TrackEvent(
                "MessageProcessed",
                new Dictionary<string, string>
                {
                    { "Sender", sender },
                    { "Content", content },
                },
                new Dictionary<string, double>
                {
                    { "Duration", duration.TotalMilliseconds },
                });
        }
        
        /// <summary>
        /// Tracks that an exception happened while processing a message.
        /// </summary>
        /// <param name="exception">The exception to track.</param>
        /// <param name="sender">The message sender.</param>
        /// <param name="content">The message content.</param>
        /// <param name="duration">The time it took before the exception was thrown.</param>
        public void ProcessMessageException(Exception exception, string sender, string content, TimeSpan duration)
        {
            this.TrackException(
                exception,
                new Dictionary<string, string>
                {
                    { "Sender", sender },
                    { "Content", content },
                },
                new Dictionary<string, double>
                {
                    { "Duration", duration.TotalMilliseconds },
                });
        }
    }
}
