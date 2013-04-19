using System;
using Microsoft.ComplexEventProcessing;

namespace SiDualMode.Base {
    public class StreamInputEvent<TPayloadType> {
        public TPayloadType Payload { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public EdgeType EdgeType { get; set; }
        public EventKind EventKind { get; set; }

        public PointEvent<TPayloadType> GetPointEvent() {
            if (this.EventKind == EventKind.Insert) {
                return PointEvent<TPayloadType>.CreateInsert(this.Start, Payload);
            }
            return PointEvent<TPayloadType>.CreateCti(Start);
        }

        public IntervalEvent<TPayloadType> GetIntervalEvent() {
            if (this.EventKind == EventKind.Insert) {
                return IntervalEvent<TPayloadType>.CreateInsert(this.Start, this.End, Payload);
            }
            return IntervalEvent<TPayloadType>.CreateCti(Start);
        }

        public EdgeEvent<TPayloadType> GetEdgeEvent() {
            if (this.EventKind == EventKind.Insert) {
                return EdgeEvent<TPayloadType>.CreateCti(this.Start);
            }

            if (this.EdgeType == EdgeType.Start) {
                return EdgeEvent.CreateStart(this.Start, this.Payload);
            }
            return EdgeEvent.CreateEnd(this.Start, this.End, this.Payload);
        }

        public StreamInputEvent(DateTimeOffset ctiDateTime) {
            EventKind = EventKind.Cti;
            Start = ctiDateTime;
            EdgeType = EdgeType.Start;
            End = DateTimeOffset.MaxValue;
            Payload = default(TPayloadType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamInputEvent&lt;TEventType&gt;"/> struct.
        /// </summary>
        /// <param name="payload">The Payload.</param>
        /// <remarks>
        ///     This sets the start to <see cref="DateTimeOffset">DateTimeOffset.MinValue</see>,
        ///     and the edge type to the default value. <br/>
        ///     The subscriber is responsible for explicitly setting these parameters.
        /// </remarks>
        public StreamInputEvent(TPayloadType payload) {
            Start = DateTimeOffset.MinValue;
            End = DateTimeOffset.MaxValue;
            EdgeType = default(EdgeType);
            Payload = payload;
            EventKind = EventKind.Insert;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamInputEvent&lt;TEventType&gt;"/> struct.
        /// </summary>
        /// <param name="payload">The event payload.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <param name="edgeType">Type of the edge.</param>
        /// <remarks>
        ///     Use when the event publisher "knows" the appropriate start time, end time and/or the edge type.
        ///     <br/>
        ///     This is useful when publishing Payload that carry this information.
        /// </remarks>
        public StreamInputEvent(TPayloadType payload, DateTimeOffset startTime, DateTimeOffset endTime, EdgeType edgeType) {
            Start = startTime;
            End = endTime;
            EdgeType = edgeType;
            Payload = payload;
            EventKind = EventKind.Insert;
        }
    }
}
