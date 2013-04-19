using System;
using System.Linq;
using System.Threading;

using SiDualMode.Base;

namespace SiDualMode.InputAdapter.TestDataAdapter {
    public class TestDataProducer : StreamEventProducer<TestDataEvent, TestDataInputConfig> {
        private DateTimeOffset _nextStartTime;
        private Timer _enqueueTimer;
        private int _runNumber = 0;

        public TestDataProducer(TestDataInputConfig config)
            : base(config) {
            _enqueueTimer = new Timer(ProduceEvents, null, Timeout.Infinite, Timeout.Infinite);
            this._nextStartTime = config.StartDateTime;
        }

        protected override void Start() {
            //Change the timer to start it.
            _enqueueTimer.Change(TimeSpan.Zero, Configuration.RefreshInterval);
        }

        /// <summary>
        /// Main driver to read events from source and enqueue them.
        /// </summary>
        /// <param name="state"></param>
        private void ProduceEvents(object state) {
            _runNumber++;

            var newEvents = TestDataEvent.CreateNext(Configuration, _runNumber);

            var eventTimestamp = _nextStartTime;

            var publishEvents = (from e in newEvents
                                 select new StreamInputEvent<TestDataEvent>(e) {
                                     Start = eventTimestamp
                                 }).ToList();

            foreach (var publishedEvent in publishEvents) {
                this.PublishEvent(publishedEvent);
            }

            this.PublishEvent(new StreamInputEvent<TestDataEvent>(eventTimestamp.AddTicks(1)));
            _nextStartTime += Configuration.TimestampIncrement;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (_enqueueTimer != null) {
                    _enqueueTimer.Dispose();
                    _enqueueTimer = null;
                }
            }
            base.Dispose(disposing);
        }
    }
}
