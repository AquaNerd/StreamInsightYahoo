using System;
using System.Runtime.Serialization;

namespace SiDualMode.InputAdapter.YahooFinanceAdapter {
    /// <summary>
    /// This is the configuration type for the GeneratorFactory. Use instance of this class to configure data
    /// and frequency of generator adapter.
    /// </summary>
    [DataContract()]
    public class YahooDataInputConfig {
        [DataMember]
        public DateTimeOffset StartDateTime { get; set; }

        [DataMember]
        public TimeSpan RefreshInterval { get; set; }

        [DataMember]
        public TimeSpan TimestampIncrement { get; set; }

        //[DataMember]
        //public string[] Symbols { get; set; }

        [DataMember]
        public int NumberOfItems { get; set; }

        [DataMember]
        public bool AlwaysUseNow { get; set; }

        [DataMember]
        public bool EnqueueCtis { get; set; }

        public YahooDataInputConfig() {
            RefreshInterval = TimeSpan.FromMilliseconds(500);
            //Symbols = new string[] {"AAPL", "DELL", "MSFT"};
            NumberOfItems = 10;
            TimestampIncrement = TimeSpan.FromMinutes(5);
            StartDateTime = DateTimeOffset.Now.AddMonths(-5);
            AlwaysUseNow = false;
            EnqueueCtis = true;
        }
    }
}
