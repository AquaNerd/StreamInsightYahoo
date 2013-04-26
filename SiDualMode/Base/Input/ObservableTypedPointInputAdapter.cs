using System;
using System.Linq;
using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Adapters;

namespace SiDualMode.Base.Input {
    public class ObservableTypedPointInputAdapter<TPayloadType, TConfigType> : TypedPointInputAdapter<TPayloadType> {
        private IDisposable _subscription;
        private readonly IObservable<StreamInputEvent<TPayloadType>> _eventProducer;
        private object _lockObject = new object();
        private bool _canEnqueue = false;
        private DateTimeOffset _lastCti = DateTimeOffset.MinValue;

        public ObservableTypedPointInputAdapter(IObservable<StreamInputEvent<TPayloadType>> eventProducer) {
            this._eventProducer = eventProducer;
        }

        public override void Start() {
            _canEnqueue = true;
            _subscription = _eventProducer.Subscribe(OnNext,
                () => {
                    if (AdapterState == AdapterState.Running) {
                        Stop();
                    }
                });
        }

        public override void Resume() {
            _canEnqueue = true;
        }

        public override void Stop() {
            lock (_lockObject) {
                _subscription.Dispose();
                base.Stop();
                Stopped();
            }
        }

        protected override void Dispose(bool disposing) {
            //Stop();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="publishedEvent"></param>
        public void OnNext(StreamInputEvent<TPayloadType> publishedEvent) {
            if (AdapterState == AdapterState.Running) {
                EnqueueEvent(publishedEvent);
            }
        }

        protected virtual void EnqueueEvent(StreamInputEvent<TPayloadType> publishedEvent) {
            if (!_canEnqueue) {
                return;
            }

            if (publishedEvent.Start < _lastCti) {
                return;
            }

            lock (_lockObject) {
                if (this.AdapterState != AdapterState.Running) {
                    return;
                }
                var point = publishedEvent.GetPointEvent();
                try {
                    var enqueueResult = this.Enqueue(ref point);
                    if (enqueueResult == EnqueueOperationResult.Success && publishedEvent.EventKind == EventKind.Cti) {
                        _lastCti = point.StartTime;
                    }
                    if (enqueueResult == EnqueueOperationResult.Full) {
                        //Queue full!!! Pause enqueueing.
                        _canEnqueue = false;
                        ReleaseEvent(ref point);
                        //Let StreamInsight know we're ready to resume.
                        Ready();
                    }
                } catch {
                    ReleaseEvent(ref point);
                    throw;
                }
            }
        }
    }
}
