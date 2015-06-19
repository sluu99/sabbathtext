﻿namespace SabbathText.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
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
        public virtual void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.telemetryClient.TrackEvent(eventName, properties, metrics);
        }

        /// <summary>
        /// Tracks an exception.
        /// </summary>
        /// <param name="exception">The exception to track.</param>
        /// <param name="properties">The optional event properties.</param>
        /// <param name="metrics">The optional event metrics.</param>
        public virtual void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.telemetryClient.TrackException(exception, properties, metrics);
        }

        /// <summary>
        /// Tracks a request
        /// </summary>
        /// <param name="name">The request name.</param>
        /// <param name="requestTime">The request time.</param>
        /// <param name="duration">The request duration.</param>
        /// <param name="responseCode">The request response code.</param>
        /// <param name="success">A value indicating if the request was successful.</param>
        /// <param name="properties">The optional request properties.</param>
        /// <param name="metrics">The optional request metrics.</param>
        public virtual void TrackRequest(
            string name, 
            DateTime requestTime, 
            TimeSpan duration, 
            string responseCode, 
            bool success,
            IDictionary<string, string> properties = null, 
            IDictionary<string, double> metrics = null)
        {
            RequestTelemetry requestTelemetry = new RequestTelemetry(name, new DateTimeOffset(requestTime), duration, responseCode, success);

            if (properties != null)
            {
                foreach (KeyValuePair<string, string> prop in properties)
                {
                    requestTelemetry.Properties.Add(prop);
                }
            }

            if (metrics != null)
            {
                foreach (KeyValuePair<string, double> metric in metrics)
                {
                    requestTelemetry.Metrics.Add(metric);
                }
            }

            this.telemetryClient.TrackRequest(requestTelemetry);
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
    }
}
