namespace SabbathText.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// A telemetry tracker that sends the telemetry to the diagnostic trace.
    /// </summary>
    public class TracingTelemetryTracker : TelemetryTracker
    {
        /// <summary>
        /// Tracks an event
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="properties">The optional event properties.</param>
        /// <param name="metrics">The optional event metrics.</param>
        public override void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Real clock: {0}. Virtual clock (UTC): {1}".InvariantFormat(DateTime.Now, Clock.UtcNow));
            sb.AppendLine("Event: {0}".InvariantFormat(eventName));

            if (properties != null)
            {
                sb.AppendLine(JsonConvert.SerializeObject(properties, Formatting.Indented));
            }

            if (metrics != null)
            {
                sb.AppendLine(JsonConvert.SerializeObject(properties, Formatting.Indented));
            }

            Trace.TraceInformation(sb.ToString());
        }
        
        /// <summary>
        /// Tracks an exception.
        /// </summary>
        /// <param name="exception">The exception to track.</param>
        /// <param name="properties">The optional event properties.</param>
        /// <param name="metrics">The optional event metrics.</param>
        public override void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Real clock: {0}. Virtual clock (UTC): {1}".InvariantFormat(DateTime.Now, Clock.UtcNow));
            sb.AppendLine("Exception: {0}".InvariantFormat(exception.Message));
            sb.AppendLine(exception.StackTrace);

            if (properties != null)
            {
                sb.AppendLine(JsonConvert.SerializeObject(properties, Formatting.Indented));
            }

            if (metrics != null)
            {
                sb.AppendLine(JsonConvert.SerializeObject(properties, Formatting.Indented));
            }

            Trace.TraceError(sb.ToString());
        }

        /// <summary>
        /// Tracks a request.
        /// </summary>
        /// <param name="name">The request name.</param>
        /// <param name="requestTime">The request time.</param>
        /// <param name="duration">The request duration.</param>
        /// <param name="responseCode">The request response code.</param>
        /// <param name="success">A value indicating if the request was successful.</param>
        /// <param name="properties">The optional request properties.</param>
        /// <param name="metrics">The optional request metrics.</param>
        public override void TrackRequest(
            string name,
            DateTime requestTime,
            TimeSpan duration,
            string responseCode,
            bool success,
            IDictionary<string, string> properties = null,
            IDictionary<string, double> metrics = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Real clock: {0}. Virtual clock (UTC): {1}".InvariantFormat(DateTime.Now, Clock.UtcNow));
            sb.AppendLine("Request: {0}".InvariantFormat(name));
            sb.AppendLine("Request time: {0}".InvariantFormat(requestTime));
            sb.AppendLine("Duration: {0}".InvariantFormat(duration));
            sb.AppendLine("Response code: {0}".InvariantFormat(responseCode));
            sb.AppendLine("Success: {0}".InvariantFormat(success));

            if (properties != null)
            {
                sb.AppendLine(JsonConvert.SerializeObject(properties, Formatting.Indented));
            }

            if (metrics != null)
            {
                sb.AppendLine(JsonConvert.SerializeObject(properties, Formatting.Indented));
            }

            Trace.TraceInformation(sb.ToString());
        }
    }
}
