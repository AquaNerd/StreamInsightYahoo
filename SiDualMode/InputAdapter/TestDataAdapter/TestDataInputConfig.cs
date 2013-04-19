using System;
using System.Runtime.Serialization;

namespace SiDualMode.InputAdapter.TestDataAdapter {
    /// <summary>
    /// This is the configuration type for the GeneratorFactory. Use instances of this class to configure data
    /// and frequency of a generator adapter.
    /// </summary>
    [DataContract()]
    public class TestDataInputConfig {
        [DataMember]
        public DateTimeOffset StartDateTime { get; set; }

        [DataMember]
        public TimeSpan RefreshInterval { get; set; }

        [DataMember]
        public TimeSpan TimestampIncrement { get; set; }

        [DataMember]
        public int NumberOfItems { get; set; }

        [DataMember]
        public bool AlwaysUseNow { get; set; }

        [DataMember]
        public bool EnqueueCtis { get; set; }

        public TestDataInputConfig() {
            RefreshInterval = TimeSpan.FromMilliseconds(500);
            NumberOfItems = 10;
            TimestampIncrement = TimeSpan.FromMinutes(5);
            StartDateTime = DateTimeOffset.Now.AddMonths(-5);
            AlwaysUseNow = false;
            EnqueueCtis = true;
        }
    }
}
