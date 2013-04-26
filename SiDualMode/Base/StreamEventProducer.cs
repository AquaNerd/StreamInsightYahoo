using System;
using System.Linq;

namespace SiDualMode.Base {
    /// <summary>
    /// Payload publisher base class.
    /// </summary>
    /// <typeparam name="TPayloadType">The type of the event type.</typeparam>
    /// <typeparam name="TConfigType">The type of config type.</typeparam>
    public abstract class StreamEventProducer<TPayloadType, TConfigType> : IObservable<StreamInputEvent<TPayloadType>>, IDisposable {
        public TConfigType Configuration { get; protected set; }
        private IObserver<StreamInputEvent<TPayloadType>> _observer;
        //This allows us to lock certain critical sections.
        //While the possibility of threading issues is low, it's still there
        protected object LockObject = new object();
        private bool _disposed;

        protected StreamEventProducer(TConfigType configuration) {
            this.Configuration = configuration;
        }

        protected abstract void Start();

        public virtual IDisposable Subscribe(IObserver<StreamInputEvent<TPayloadType>> observer) {
            CheckDisposed();
            lock (LockObject) {
                this._observer = observer;
            }
            Start();
            return this;
        }

        /// <summary>
        /// Publishes the exception.
        /// </summary>
        /// <param name="ex">The exception</param>
        protected void PublishException(Exception ex) {
            CheckDisposed();
            lock (LockObject) {
                if (_observer != null) {
                    _observer.OnError(ex);
                }
            }
        }

        /// <summary>
        /// Publishes the event.
        /// </summary>
        /// <param name="newEvent">The new event.</param>
        protected void PublishEvent(StreamInputEvent<TPayloadType> newEvent) {
            CheckDisposed();
            lock (LockObject) {
                if (_observer != null) {
                    _observer.OnNext(newEvent);
                }
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                lock (LockObject) {
                    _observer.OnCompleted();
                    _observer = null;
                }
            }
        }

        /// <summary>
        /// Checks if the class has been disposed.
        /// </summary>
        /// <remarks>Throws an <see cref="ObjectDisposedException"/>ObjectDisposedException</remarks>
        /// if the object has been disposed.
        protected void CheckDisposed() {
            if (_disposed) {
                throw new ObjectDisposedException("Object has been disposed");
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmangaged resources.
        /// </summary>
        public void Dispose() {
            //Make sure Dispose is only handled once!
            if (!_disposed) {
                Dispose(true);
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
