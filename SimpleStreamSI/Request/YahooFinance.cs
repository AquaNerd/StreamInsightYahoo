using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Xml.Linq;

using Microsoft.ComplexEventProcessing;
using Microsoft.ComplexEventProcessing.Linq;

using SimpleStreamSI.Events.Input;

namespace SimpleStreamSI.Request {
    public class YahooFinance {
        private const string BASE_URL = "http://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20yahoo.finance.quotes%20where%20symbol%20in%20({0})&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
        public List<PointEvent<YahooQuote>> Quotes = new List<PointEvent<YahooQuote>>();

        public YahooFinance(string[] symbols) {
            //initial fill of List<PointEvent<YahooQuote>>
            Quotes = GetPointEvents(symbols);
        }

        //
        public static List<YahooQuote> GetQuotes(string[] symbols) {
            string symbolList = String.Join("%2C", symbols.Select(w => "%22" + w + "%22").ToArray());
            string url = string.Format(BASE_URL, symbolList);
            List<YahooQuote> Quotes = new List<YahooQuote>();

            try {
                XDocument doc = XDocument.Load(url);
                while (doc.Root.Element("results").IsEmpty) {
                    doc = XDocument.Load(url);
                }
                XElement results = doc.Root.Element("results");
                foreach (string symbol in symbols) {
                    YahooQuote quote = new YahooQuote(symbol);
                    quote = ParseQuote(symbol, results);
                    Quotes.Add(quote);
                }
            } catch (Exception e) {
                Console.WriteLine("GetQuotesException: {0}", e.Message);
            }
            return Quotes;
        }

        public static YahooQuote GetSingleQuote(string symbol) {
            string symbolList = String.Join("%2C", "%22" + symbol + "%22");
            string url = string.Format(BASE_URL, symbolList);
            YahooQuote quote = new YahooQuote(symbol);
            try {
                XDocument doc = XDocument.Load(url);
                while (doc.Root.Element("results").IsEmpty) {
                    doc = XDocument.Load(url);
                }
                XElement results = doc.Root.Element("results");
                quote = ParseQuote(symbol, results);
            } catch (Exception e) {
                Console.WriteLine("GetSingleQuoteException: {0}", e.Message);
            }
            return quote;
        }

        public static List<PointEvent<YahooQuote>> GetPointEvents(string[] symbols) {
            string symbolList = String.Join("%2C", symbols.Select(w => "%22" + w + "%22").ToArray());
            string url = string.Format(BASE_URL, symbolList);
            List<PointEvent<YahooQuote>> Quotes = new List<PointEvent<YahooQuote>>();

            try {
                XDocument doc = XDocument.Load(url);
                while (doc.Root.Element("results").IsEmpty) {
                    doc = XDocument.Load(url);
                }
                XElement results = doc.Root.Element("results");
                foreach (string symbol in symbols) {
                    YahooQuote quote = new YahooQuote(symbol);
                    quote = ParseQuote(symbol, results);
                    Quotes.Add(PointEvent<YahooQuote>.CreateInsert(quote.LastUpdate, quote));
                    Quotes.Add(PointEvent<YahooQuote>.CreateCti(quote.LastUpdate));
                }
            } catch (Exception e) {
                Console.WriteLine("GetPointEventException: {0}", e.Message);
            }

            return Quotes;
        }

        public void UpdatePointEvents(string[] symbols) {
            string symbolList = String.Join("%2C", symbols.Select(w => "%22" + w + "%22").ToArray());
            string url = string.Format(BASE_URL, symbolList);

            try {
                XDocument doc = XDocument.Load(url);
                while (doc.Root.Element("results").IsEmpty) {
                    doc = XDocument.Load(url);
                }
                XElement results = doc.Root.Element("results");
                foreach (string symbol in symbols) {
                    YahooQuote quote = new YahooQuote(symbol);
                    quote = ParseQuote(symbol, results);
                    Quotes.Add(PointEvent<YahooQuote>.CreateInsert(quote.LastUpdate, quote));
                    Quotes.Add(PointEvent<YahooQuote>.CreateCti(quote.LastUpdate));
                }
            } catch (Exception e) {
                Console.WriteLine("UpdatePointEventException: {0}", e.Message);
            }
        }

        private static YahooQuote ParseQuote(string symbol, XElement results) {
            YahooQuote quote = new YahooQuote(symbol);

            XElement q = results.Elements("quote").First(w => w.Attribute("symbol").Value == symbol);

            quote.Symbol = q.Element("Symbol").Value;
            quote.LastTradePrice = GetDecimal(q.Element("LastTradePriceOnly").Value);
            quote.LastUpdate = DateTime.Now;

            quote.Ask = GetDecimal(q.Element("Ask").Value);
            quote.Bid = GetDecimal(q.Element("Bid").Value);
            quote.AverageDailyVolume = GetDecimal(q.Element("AverageDailyVolume").Value);
            quote.BookValue = GetDecimal(q.Element("BookValue").Value);
            quote.Change = GetDecimal(q.Element("Change").Value);
            quote.DividendShare = GetDecimal(q.Element("DividendShare").Value);
            quote.LastTradeDate = GetDateTime(q.Element("LastTradeDate").Value + " " + q.Element("LastTradeTime").Value);
            quote.EarningsShare = GetDecimal(q.Element("EarningsShare").Value);
            quote.EpsEstimateCurrentYear = GetDecimal(q.Element("EPSEstimateCurrentYear").Value);
            quote.EpsEstimateNextYear = GetDecimal(q.Element("EPSEstimateNextYear").Value);
            quote.EpsEstimateNextQuarter = GetDecimal(q.Element("EPSEstimateNextQuarter").Value);
            quote.DailyLow = GetDecimal(q.Element("DaysLow").Value);
            quote.DailyHigh = GetDecimal(q.Element("DaysHigh").Value);
            quote.YearlyLow = GetDecimal(q.Element("YearLow").Value);
            quote.YearlyHigh = GetDecimal(q.Element("YearHigh").Value);
            quote.MarketCapitalization = GetDecimal(q.Element("MarketCapitalization").Value);
            quote.Ebitda = GetDecimal(q.Element("EBITDA").Value);
            quote.ChangeFromYearLow = GetDecimal(q.Element("ChangeFromYearLow").Value);
            quote.PercentChangeFromYearLow = GetDecimal(q.Element("PercentChangeFromYearLow").Value);
            quote.ChangeFromYearHigh = GetDecimal(q.Element("ChangeFromYearHigh").Value);
            quote.PercentChangeFromYearHigh = GetDecimal(q.Element("PercebtChangeFromYearHigh").Value); //missspelling in yahoo for field name
            quote.FiftyDayMovingAverage = GetDecimal(q.Element("FiftydayMovingAverage").Value);
            quote.TwoHunderedDayMovingAverage = GetDecimal(q.Element("TwoHundreddayMovingAverage").Value);
            quote.ChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("ChangeFromTwoHundreddayMovingAverage").Value);
            quote.PercentChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("PercentChangeFromTwoHundreddayMovingAverage").Value);
            quote.PercentChangeFromFiftyDayMovingAverage = GetDecimal(q.Element("PercentChangeFromFiftydayMovingAverage").Value);
            quote.Name = q.Element("Name").Value;
            quote.Open = GetDecimal(q.Element("Open").Value);
            quote.PreviousClose = GetDecimal(q.Element("PreviousClose").Value);
            quote.ChangeInPercent = GetDecimal(q.Element("ChangeinPercent").Value);
            quote.PriceSales = GetDecimal(q.Element("PriceSales").Value);
            quote.PriceBook = GetDecimal(q.Element("PriceBook").Value);
            quote.ExDividendDate = GetDateTime(q.Element("ExDividendDate").Value);
            quote.PeRatio = GetDecimal(q.Element("PERatio").Value);
            quote.DividendPayDate = GetDateTime(q.Element("DividendPayDate").Value);
            quote.PegRatio = GetDecimal(q.Element("PEGRatio").Value);
            quote.PriceEpsEstimateCurrentYear = GetDecimal(q.Element("PriceEPSEstimateCurrentYear").Value);
            quote.PriceEpsEstimateNextYear = GetDecimal(q.Element("PriceEPSEstimateNextYear").Value);
            quote.ShortRatio = GetDecimal(q.Element("ShortRatio").Value);
            quote.OneYearPriceTarget = GetDecimal(q.Element("OneyrTargetPrice").Value);
            quote.Volume = GetDecimal(q.Element("Volume").Value);
            quote.StockExchange = q.Element("StockExchange").Value;

            return quote;
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
