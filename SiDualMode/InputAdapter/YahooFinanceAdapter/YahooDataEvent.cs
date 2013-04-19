using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SiDualMode.InputAdapter.YahooFinanceAdapter {
    /// <summary>
    /// Payload type for the generator adapters.
    /// </summary>
    public class YahooDataEvent {
        private const string BASE_URL = "http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20in%20({0})&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";

        //Required
        public DateTime LastUpdateTime { get; set; }
        public string Name { get; set; }
        public string StockExchange { get; set; }
        public string Symbol { get; set; }

        //Optional
        public decimal? Ask { get; set; }
        public decimal? AverageDailyVolume { get; set; }
        public decimal? Bid { get; set; }
        public decimal? BookValue { get; set; }
        public decimal? Change { get; set; }
        public decimal? ChangeFromTwoHundredDayMovingAverage { get; set; }
        public decimal? ChangeFromYearHigh { get; set; }
        public decimal? ChangeFromYearLow { get; set; }
        public decimal? ChangeInPercent { get; set; }
        public decimal? ChangePercent { get; set; }
        public decimal? DailyHigh { get; set; }
        public decimal? DailyLow { get; set; }
        public DateTime? DividendPayDate { get; set; }
        public decimal? DividendShare { get; set; }
        public decimal? DividendYield { get; set; }
        public decimal? EarningsShare { get; set; }
        public decimal? Ebitda { get; set; }
        public decimal? EpsEstimateNextQuarter { get; set; }
        public decimal? EpsEstimateNextYear { get; set; }
        public decimal? EpsEstimateCurrentYear { get; set; }
        public DateTime? ExDividendDate { get; set; }
        public decimal? FiftyDayMovingAverage { get; set; }
        public DateTime? LastTradeDate { get; set; }
        public decimal? LastTradePrice { get; set; }
        public decimal? MarketCapitalization { get; set; }
        public decimal? OneYearPriceTarget { get; set; }
        public decimal? Open { get; set; }
        public decimal? PegRatio { get; set; }
        public decimal? PeRatio { get; set; }
        public decimal? PercentChangeFromFiftyDayMovingAverage { get; set; }
        public decimal? PercentChangeFromTwoHundredDayMovingAverage { get; set; }
        public decimal? PercentChangeFromYearHigh { get; set; }
        public decimal? PercentChangeFromYearLow { get; set; }
        public decimal? PreviousClose { get; set; }
        public decimal? PriceBook { get; set; }
        public decimal? PriceEpsEstimateCurrentYear { get; set; }
        public decimal? PriceEpsEstimateNextYear { get; set; }
        public decimal? PriceSales { get; set; }
        public decimal? ShortRatio { get; set; }
        public decimal? TwoHunderedDayMovingAverage { get; set; }
        public decimal? Volume { get; set; }
        public decimal? YearlyHigh { get; set; }
        public decimal? YearlyLow { get; set; }
        
        public override string ToString() {
            return string.Format("{0} - {1} - {2}", Name, LastTradePrice, LastUpdateTime.ToString("MM/dd/yyyy HH:mm:ss.fff"));
        }

        public static List<YahooDataEvent> CreateNext(YahooDataInputConfig config, int runNumber) {
            List<YahooDataEvent> newReferenceData = new List<YahooDataEvent>();
            string[] Symbols = new string[] { "AAPL", "DELL", "MSFT" };

            //TODO: Add Requestor to get data from YahooFinance
            string symbolList = "";
            if (Symbols.Count() > 1) {
                symbolList = String.Join("%2C", Symbols.Select(w => "%22" + w + "%22").ToArray());
            } else if (Symbols.Count() == 1) {
                symbolList = String.Join("%2C", "%22" + Symbols.First() + "%22");
            } else {
                //throw exception.
                //need symbols
                throw new ArgumentException("No symbols requested.  Need to request at least 1 symbol.");
            }

            string url = string.Format(BASE_URL, symbolList);
            try {
                XDocument doc = XDocument.Load(url);
                while (doc.Root.Element("results").IsEmpty) {
                    doc = XDocument.Load(url);
                }
                XElement results = doc.Root.Element("results");
                foreach (string symbol in Symbols) {
                    newReferenceData.Add(ParseXmlRequest(symbol, results));
                }
            } catch (Exception ex) {
                Console.WriteLine("RequestDataException: {0}", ex.Message);
            }

            return newReferenceData;
        }

        private static YahooDataEvent ParseXmlRequest(string symbol, XElement results) {
            YahooDataEvent newEvent = new YahooDataEvent();

            XElement element = results.Elements("quote").First(w => w.Attribute("symbol").Value == symbol);

            newEvent.Symbol = element.Element("Symbol").Value;
            newEvent.LastTradePrice = GetDecimal(element.Element("LastTradePriceOnly").Value);
            newEvent.LastUpdateTime = DateTime.Now;

            newEvent.Ask = GetDecimal(element.Element("Ask").Value);
            newEvent.Bid = GetDecimal(element.Element("Bid").Value);
            newEvent.AverageDailyVolume = GetDecimal(element.Element("AverageDailyVolume").Value);
            newEvent.BookValue = GetDecimal(element.Element("BookValue").Value);
            newEvent.Change = GetDecimal(element.Element("Change").Value);
            newEvent.DividendShare = GetDecimal(element.Element("DividendShare").Value);
            newEvent.LastTradeDate = GetDateTime(element.Element("LastTradeDate").Value + " " + element.Element("LastTradeTime").Value);
            newEvent.EarningsShare = GetDecimal(element.Element("EarningsShare").Value);
            newEvent.EpsEstimateCurrentYear = GetDecimal(element.Element("EPSEstimateCurrentYear").Value);
            newEvent.EpsEstimateNextYear = GetDecimal(element.Element("EPSEstimateNextYear").Value);
            newEvent.EpsEstimateNextQuarter = GetDecimal(element.Element("EPSEstimateNextQuarter").Value);
            newEvent.DailyLow = GetDecimal(element.Element("DaysLow").Value);
            newEvent.DailyHigh = GetDecimal(element.Element("DaysHigh").Value);
            newEvent.YearlyLow = GetDecimal(element.Element("YearLow").Value);
            newEvent.YearlyHigh = GetDecimal(element.Element("YearHigh").Value);
            newEvent.MarketCapitalization = GetDecimal(element.Element("MarketCapitalization").Value);
            newEvent.Ebitda = GetDecimal(element.Element("EBITDA").Value);
            newEvent.ChangeFromYearLow = GetDecimal(element.Element("ChangeFromYearLow").Value);
            newEvent.PercentChangeFromYearLow = GetDecimal(element.Element("PercentChangeFromYearLow").Value);
            newEvent.ChangeFromYearHigh = GetDecimal(element.Element("ChangeFromYearHigh").Value);
            newEvent.PercentChangeFromYearHigh = GetDecimal(element.Element("PercebtChangeFromYearHigh").Value); //missspelling in yahoo for field name
            newEvent.FiftyDayMovingAverage = GetDecimal(element.Element("FiftydayMovingAverage").Value);
            newEvent.TwoHunderedDayMovingAverage = GetDecimal(element.Element("TwoHundreddayMovingAverage").Value);
            newEvent.ChangeFromTwoHundredDayMovingAverage = GetDecimal(element.Element("ChangeFromTwoHundreddayMovingAverage").Value);
            newEvent.PercentChangeFromTwoHundredDayMovingAverage = GetDecimal(element.Element("PercentChangeFromTwoHundreddayMovingAverage").Value);
            newEvent.PercentChangeFromFiftyDayMovingAverage = GetDecimal(element.Element("PercentChangeFromFiftydayMovingAverage").Value);
            newEvent.Name = element.Element("Name").Value;
            newEvent.Open = GetDecimal(element.Element("Open").Value);
            newEvent.PreviousClose = GetDecimal(element.Element("PreviousClose").Value);
            newEvent.ChangeInPercent = GetDecimal(element.Element("ChangeinPercent").Value);
            newEvent.PriceSales = GetDecimal(element.Element("PriceSales").Value);
            newEvent.PriceBook = GetDecimal(element.Element("PriceBook").Value);
            newEvent.ExDividendDate = GetDateTime(element.Element("ExDividendDate").Value);
            newEvent.PeRatio = GetDecimal(element.Element("PERatio").Value);
            newEvent.DividendPayDate = GetDateTime(element.Element("DividendPayDate").Value);
            newEvent.PegRatio = GetDecimal(element.Element("PEGRatio").Value);
            newEvent.PriceEpsEstimateCurrentYear = GetDecimal(element.Element("PriceEPSEstimateCurrentYear").Value);
            newEvent.PriceEpsEstimateNextYear = GetDecimal(element.Element("PriceEPSEstimateNextYear").Value);
            newEvent.ShortRatio = GetDecimal(element.Element("ShortRatio").Value);
            newEvent.OneYearPriceTarget = GetDecimal(element.Element("OneyrTargetPrice").Value);
            newEvent.Volume = GetDecimal(element.Element("Volume").Value);
            newEvent.StockExchange = element.Element("StockExchange").Value;

            return newEvent;
        }

        private static decimal? GetDecimal(String input) {
            if (input == null) {
                return null;
            }

            input = input.Replace("%", "");

            decimal value;

            if (Decimal.TryParse(input, out value)) {
                return value;
            }

            return null;
        }

        private static DateTime? GetDateTime(string input) {
            if (input == null) {
                return null;
            }

            DateTime value;

            if (DateTime.TryParse(input, out value)) {
                return value;
            }

            return null;
        }
    }
}
