using System;
using System.Collections.Concurrent;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Reactive.Net.Sandbox.Samples.StockTraditionalEventHandlerVSReactiveSample
{
    // Example from https://livebook.manning.com/book/rx-dot-net-in-action/chapter-2/9
    // Scenario
    // - Stock update receive
    // - Calculate prevPrice change between current and previous prevPrice
    //   - if (price_change > 10%)  -> Notify user
    //     else -> Wait for next update and notify that price_change less than 10%

    public class ReactiveStockMonitor
    {
        private const double MaxChangeRatio = 0.1;
        private static ConcurrentDictionary<string, StockTick> _stockInfos;

        public static void RunReactiveStockMonitor()
        {
            _stockInfos = CommonStockDataGenerator.PrepareStockInfos();

            StockTickObservable()
                .DistinctUntilChanged()
                .Subscribe(SendUserNotificationIfChangeRatioGreaterThanMaxChangeRatio);
        }

        private static void SendUserNotificationIfChangeRatioGreaterThanMaxChangeRatio(StockTick stockTick)
        {
            StockTick? stockInfo = null;
            var quoteSymbol = stockTick.QuoteSymbol;
            var stockInfoExist = _stockInfos.TryGetValue(quoteSymbol, out stockInfo);

            if (stockInfoExist)
            {
                var priceDiff = stockTick.Price - stockInfo.Price;
                var changeRatio = Math.Abs(priceDiff / stockInfo.Price);

                if (changeRatio > MaxChangeRatio)
                {
                    //Notify user
                    Console.WriteLine(
                        $"Stock, {quoteSymbol} has changed with {changeRatio} ratio -> Old prevPrice: {stockInfo.Price}, New Price: {stockTick.Price}");
                }
                else
                {
                    Console.WriteLine($"--> Change ratio less than {MaxChangeRatio}...");
                }
            }
        }

        private static IObservable<StockTick> StockTickObservable() =>
            Observable.Create<StockTick>(observer =>
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.ForegroundColor = ConsoleColor.White;

                while (true)
                {
                    var symbol = CommonStockDataGenerator.GetRandomQuoteSymbol();
                    var newRandomPriceForSymbol = CommonStockDataGenerator.GetNewRandomPrice(symbol, _stockInfos);

                    observer.OnNext(new StockTick(symbol, newRandomPriceForSymbol));
                }

                observer.OnCompleted();
                return Disposable.Empty;
            });
    }
}