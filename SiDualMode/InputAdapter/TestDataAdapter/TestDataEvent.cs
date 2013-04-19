using System;
using System.Collections.Generic;

namespace SiDualMode.InputAdapter.TestDataAdapter {
    /// <summary>
    /// Payload type for the generator adapters.
    /// </summary>
    public class TestDataEvent {
        public string ItemId { get; set; }
        public int RunNumber { get; set; }
        public DateTime EnqueueTimestamp { get; set; }
        public double Value { get; set; }

        private static Random rdm = new Random();

        public static List<TestDataEvent> CreateNext(TestDataInputConfig config, int runNumber) {
            List<TestDataEvent> newReferenceData = new List<TestDataEvent>(config.NumberOfItems);

            for (int i = 0; i < config.NumberOfItems; i++) {
                newReferenceData.Add(new TestDataEvent() {
                    ItemId = "Item" + i.ToString(),
                    RunNumber = runNumber,
                    EnqueueTimestamp = DateTime.Now,
                    Value = rdm.NextDouble() * 10
                });
            }

            return newReferenceData;
        }
    }
}
