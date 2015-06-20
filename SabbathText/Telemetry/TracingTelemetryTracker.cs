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
        protected override void TrackEvent(string eventName, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Real clock (UTC): {0}. Virtual clock (UTC): {1}".InvariantFormat(DateTime.Now.ToUniversalTime(), Clock.UtcNow));
            sb.AppendLine("Event: {0}".InvariantFormat(eventName));

            if (properties != null)
            {
                sb.AppendLine(JsonConvert.SerializeObject(properties, Formatting.Indented));
            }

            if (metrics != null)
            {
                sb.AppendLine(JsonConvert.SerializeObject(metrics, Formatting.Indented));
            }

            Trace.TraceInformation(sb.ToString());
        }
        
        /// <summary>
        /// Tracks an exception.
        /// </summary>
        /// <param name="exception">The exception to track.</param>
        /// <param name="properties">The optional event properties.</param>
        /// <param name="metrics">The optional event metrics.</param>
        protected override void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Real clock: {0}. Virtual clock (UTC): {1}".InvariantFormat(DateTime.Now.ToUniversalTime(), Clock.UtcNow));
            sb.AppendLine("Exception: {0}".InvariantFormat(exception.Message));
            sb.AppendLine(exception.StackTrace);

            if (properties != null)
            {
                sb.AppendLine(JsonConvert.SerializeObject(properties, Formatting.Indented));
            }

            if (metrics != null)
            {
                sb.AppendLine(JsonConvert.SerializeObject(metrics, Formatting.Indented));
            }

            Trace.TraceError(sb.ToString());
        }
    }
}
