using System;
using System.Threading;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace SiDualMode.Base.Output {
    public class ObserverTypedPointOutputAdapter<TPayloadType> : TypedPointOutputAdapter<TPayloadType> {
        private readonly IObserver<PointEvent<TPayloadType>> _sinkObserver;
        private object _monitorObject = new object();

        public ObserverTypedPointOutputAdapter(IObserver<PointEvent<TPayloadType>> sinkObserver) {
            _sinkObserver = sinkObserver;
        }

        public override void Stop() {
            try {
                Monitor.Enter(_monitorObject);
                //On last round to dequeue
                EmptyQueue();
                //Completed
                _sinkObserver.OnCompleted();
            } finally {
                Monitor.Exit(_monitorObject);
            }
            base.Stop();
            Stopped();
        }

        public override void Resume() {
            System.Threading.Thread thd = new Thread(DequeueEvents);
            thd.Start();
        }

        public override void Start() {
            System.Threading.Thread thd = new Thread(DequeueEvents);
            thd.Start();
        }

        private void DequeueEvents() {
            if (this.AdapterState != AdapterState.Running) {
                return;
            }
            //Ensures only 1 thread is dequeueing and no other threads are blocked.
            if (Monitor.TryEnter(_monitorObject)) {
                try {
                    EmptyQueue();
                } catch (Exception ex) {
                    _sinkObserver.OnError(ex);
                } finally {
                    Monitor.Exit(_monitorObject);
                    this.Ready();
                }
            }
        }

        private void EmptyQueue() {
            PointEvent<TPayloadType> dequeuedEvent;

            while (this.Dequeue(out dequeuedEvent) == DequeueOperationResult.Success) {
                _sinkObserver.OnNext(dequeuedEvent);
            }
        }
    }
}
