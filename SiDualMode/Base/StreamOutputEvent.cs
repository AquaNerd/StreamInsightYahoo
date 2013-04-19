using System;
using Microsoft.ComplexEventProcessing;

namespace SiDualMode.Base {
    public class StreamOutputEvent<TPayload> {

        public DateTimeOffset StartTime { get; private set; }
        public EventKind EventKind { get; private set; }
        public DateTimeOffset? EndTime { get; private set; }
        public EventShape EventShape { get; private set; }
        public EdgeType? EdgeType { get; private set; }
        public TPayload Payload { get; private set; }

        /// <summary>
        /// Creates and output event from a source event.
        /// </summary>
        /// <param name="sourceEvent">The source event.</param>
        /// <returns></returns>
        public static StreamOutputEvent<TPayload> Create(PointEvent<TPayload> sourceEvent) {
            var outputEvent = new StreamOutputEvent<TPayload>() {
                StartTime = sourceEvent.StartTime,
                EventKind = sourceEvent.EventKind,
                EventShape = EventShape.Point
            };

            if (sourceEvent.EventKind == EventKind.Insert) {
                outputEvent.Payload = sourceEvent.Payload;
            }
            return outputEvent;
        }

        /// <summary>
        /// Creates and output event from a source event.
        /// </summary>
        /// <param name="sourceEvent">The source event.</param>
        /// <returns></returns>
        public static StreamOutputEvent<TPayload> Create(IntervalEvent<TPayload> sourceEvent) {
            var outputEvent = new StreamOutputEvent<TPayload>() {
                StartTime = sourceEvent.StartTime,
                EventKind = sourceEvent.EventKind,
                EventShape = EventShape.Interval
            };

            if (sourceEvent.EventKind == EventKind.Insert) {
                outputEvent.EndTime = sourceEvent.EndTime;
                outputEvent.Payload = sourceEvent.Payload;
            }
            return outputEvent;
        }

        /// <summary>
        /// Creates and output event from a source event.
        /// </summary>
        /// <param name="sourceEvent">The source event.</param>
        /// <returns></returns>
        public static StreamOutputEvent<TPayload> Create(EdgeEvent<TPayload> sourceEvent) {
            var outputEvent = new StreamOutputEvent<TPayload>() {
                StartTime = sourceEvent.StartTime,
                EventKind = sourceEvent.EventKind,
                EventShape = EventShape.Edge
            };

            if (sourceEvent.EventKind == EventKind.Insert) {
                outputEvent.Payload = sourceEvent.Payload;
                outputEvent.EdgeType = sourceEvent.EdgeType;
                if (sourceEvent.EdgeType == Microsoft.ComplexEventProcessing.EdgeType.End) {
                    outputEvent.EndTime = sourceEvent.EndTime;
                }
            }
            return outputEvent;
        }
    }
}
