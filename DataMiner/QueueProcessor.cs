using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace DataMiner;

// Generic base class for queue processing
public abstract class QueueProcessor<TInput, TProcessed> where TProcessed : class
{
    protected readonly ConcurrentQueue<TInput> _inputQueue;
    protected readonly List<TProcessed> _results = new();
    protected readonly CancellationTokenSource _cts = new();
    protected readonly object _lock = new();
    protected readonly GoldMiner _goldMiner;
    private readonly int _batchSize;
    private readonly string _outputFile;

    protected QueueProcessor(GoldMiner goldMiner, ConcurrentQueue<TInput> inputQueue, string outputFile, int batchSize = 100)
    {
        _goldMiner = goldMiner ?? throw new ArgumentNullException(nameof(goldMiner));
        _inputQueue = inputQueue ?? throw new ArgumentNullException(nameof(inputQueue));
        _outputFile = outputFile ?? throw new ArgumentNullException(nameof(outputFile));
        _batchSize = batchSize > 0 ? batchSize : 100;
        Task.Run(() => ProcessQueueAsync(_cts.Token));
    }

    public void Add(TInput input)
    {
        if (IsValidInput(input))
        {
            _inputQueue.Enqueue(input);
            Console.WriteLine($"Enqueued item: {input}");
        }
    }

    protected abstract bool IsValidInput(TInput input);
    protected abstract TProcessed ProcessInput(TInput input);
    protected abstract string GetCsvHeader();
    protected abstract string GetCsvLine(TProcessed result);

    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var batch = new List<TInput>();
                while (batch.Count < _batchSize && _inputQueue.TryDequeue(out var input) && IsValidInput(input))
                {
                    batch.Add(input);
                }

                if (batch.Count > 0)
                {
                    var processedBatch = batch.Select(ProcessInput).ToList();
                    lock (_lock)
                    {
                        _results.AddRange(processedBatch);
                        if (_results.Count >= _batchSize)
                        {
                            SaveToCsvAsync().GetAwaiter().GetResult(); // Synchronous for simplicity
                        }
                    }
                }
                else
                {
                    // Dynamic delay based on queue size
                    await Task.Delay(_inputQueue.Count > 1000 ? 500 : 1000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing batch: {ex.Message}");
            }
        }
    }

    private async Task SaveToCsvAsync()
    {
        try
        {
            lock (_lock)
            {
                var csvLines = new[] { GetCsvHeader() }
                    .Concat(_results.Select(GetCsvLine))
                    .ToList();
                File.WriteAllLinesAsync(_outputFile, csvLines).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to CSV: {ex.Message}");
        }
    }

    public IEnumerable<TProcessed> GetResults(Func<TProcessed, bool>? predicate = null)
    {
        lock (_lock)
        {
            return predicate == null
                ? _results.AsEnumerable()
                : _results.Where(predicate).AsEnumerable();
        }
    }

    public void Stop()
    {
        _cts.Cancel();
        try
        {
            _cts.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            lock (_lock)
            {
                SaveToCsvAsync().GetAwaiter().GetResult();
                var count = _results.Count;
                var latest = _results.OrderByDescending(r => GetTimestamp(r)).FirstOrDefault();

                Console.WriteLine($"Final results: {count} items processed");
                if (latest != null)
                {
                    Console.WriteLine($"Latest result: {GetSummary(latest)}");
                }
            }
        }
        finally
        {
            _cts.Dispose();
        }
    }

    protected abstract DateTime GetTimestamp(TProcessed result);
    protected abstract string GetSummary(TProcessed result);
}

public class GoldDustProcessor : QueueProcessor<Dust, GoldDustProcessor.ProcessedDust>
{
    public class ProcessedDust
    {
        public Dust Input { get; set; }
        public DateTime Timestamp { get; set; }
        public string Comment { get; set; }
        public bool Processed { get; set; }

        public ProcessedDust(Dust input, string comment)
        {
            Input = input;
            Timestamp = DateTime.UtcNow;
            Comment = comment;
            Processed = true;
        }
    }

    public GoldDustProcessor(GoldMiner goldMiner, ConcurrentQueue<Dust> inputQueue)
        : base(goldMiner, inputQueue, "processed_dust.csv")
    {
    }

    protected override bool IsValidInput(Dust input) => input != null && !input.Key.Equals(Guid.Empty);

    protected override ProcessedDust ProcessInput(Dust input)
    {
        string error = string.Empty;
        if (!_goldMiner.DustDictionary.TryAdd(input.Key, input))
        {
            error = $"Key {input.Key} already exists in DustDictionary";
        }
        return new ProcessedDust(input, error);
    }

    protected override string GetCsvHeader() => "Input,Timestamp,Comment,Processed";

    protected override string GetCsvLine(ProcessedDust result) =>
        $"{result.Input},{result.Timestamp:O},{result.Comment},{result.Processed}";

    protected override DateTime GetTimestamp(ProcessedDust result) => result.Timestamp;

    protected override string GetSummary(ProcessedDust result) =>
        $"Input={result.Input}, Timestamp={result.Timestamp:O}, Key={result.Input.Key}";
}

public class RegressionNamesProcessor : QueueProcessor<string, RegressionNamesProcessor.ProcessedString>
{
    public class ProcessedString
    {
        public string Input { get; set; }
        public DateTime Timestamp { get; set; }
        public int Length { get; set; }
        public int CharCount { get; set; }
        public bool Processed { get; set; }

        public ProcessedString(string input)
        {
            Input = input;
            Timestamp = DateTime.UtcNow;
            Length = input.Length;
            CharCount = input.Count(char.IsLetter);
            Processed = true;
        }
    }

    public RegressionNamesProcessor(GoldMiner goldMiner, ConcurrentQueue<string> inputQueue)
        : base(goldMiner, inputQueue, "processed_strings.csv")
    {
    }

    protected override bool IsValidInput(string input) => !string.IsNullOrEmpty(input);

    protected override ProcessedString ProcessInput(string input)
    {
        if (_goldMiner.DustQueue.Count <= 40000)
        {
            foreach (var dust in _goldMiner.GoldDust(input))
            {
                _goldMiner.DustQueue.Enqueue(dust);
            }
        }
        return new ProcessedString(input);
    }

    protected override string GetCsvHeader() => "Input,Timestamp,Length,CharCount,Processed";

    protected override string GetCsvLine(ProcessedString result) =>
        $"{result.Input},{result.Timestamp:O},{result.Length},{result.CharCount},{result.Processed}";

    protected override DateTime GetTimestamp(ProcessedString result) => result.Timestamp;

    protected override string GetSummary(ProcessedString result) =>
        $"Input={result.Input}, Timestamp={result.Timestamp:O}, Length={result.Length}";
}

// Assuming GoldMiner class with DustsQueue
//public partial class GoldMiner
//{
//    public readonly ConcurrentDictionary<Guid, Dust> DustDictionary = new();
//    public readonly ConcurrentQueue<string> RegressionNameQueue = new();
//    public readonly ConcurrentQueue<Dust> DustQueue = new();

//    // Placeholder for GoldDust method
//    public IEnumerable<Dust> GoldDust(string input)
//    {
//        // Implement actual logic here
//        yield break;
//    }
//}