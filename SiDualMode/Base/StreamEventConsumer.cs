using System;
using Microsoft.ComplexEventProcessing;

namespace SiDualMode.Base {
    public abstract class StreamEventConsumer<TPayloadType, TConfigType> :
    IObserver<PointEvent<TPayloadType>>, IObserver<EdgeEvent<TPayloadType>>, IObserver<IntervalEvent<TPayloadType>> {
        public TConfigType Configuration { get; private set; }

        public abstract void Completed();

        public abstract void Error(Exception error);

        public abstract void EventReceived(StreamOutputEvent<TPayloadType> outputEvent);

        protected StreamEventConsumer(TConfigType configuration) {
            this.Configuration = configuration;
        }

        public void OnNext(PointEvent<TPayloadType> value) {
            EventReceived(StreamOutputEvent<TPayloadType>.Create((PointEvent<TPayloadType>)value));
        }

        public void OnNext(IntervalEvent<TPayloadType> value) {
            EventReceived(StreamOutputEvent<TPayloadType>.Create((IntervalEvent<TPayloadType>)value));
        }

        public void OnNext(EdgeEvent<TPayloadType> value) {
            EventReceived(StreamOutputEvent<TPayloadType>.Create((EdgeEvent<TPayloadType>)value));
        }

        public void OnCompleted() {
            Completed();
        }

        public void OnError(Exception error) {
            Error(error);
        }
    }
}
