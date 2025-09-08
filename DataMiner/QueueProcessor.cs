using System.Collections.Concurrent;

namespace DataMiner;

// Generic base class for queue processing
public abstract class QueueProcessor<TInput, TProcessed> where TProcessed : class
{
    protected readonly ConcurrentQueue<TInput> InputQueue;
    protected readonly List<TProcessed> Results = new();
    protected readonly CancellationTokenSource Cts = new();
    protected readonly object Lock = new();
    protected readonly GoldMiner _goldMiner;
    private readonly int _batchSize;
    private readonly string _outputFile;

    protected QueueProcessor(GoldMiner goldMiner, ConcurrentQueue<TInput> inputQueue, string outputFile, int batchSize = 100)
    {
        _goldMiner = goldMiner ?? throw new ArgumentNullException(nameof(goldMiner));
        InputQueue = inputQueue ?? throw new ArgumentNullException(nameof(inputQueue));
        _outputFile = outputFile ?? throw new ArgumentNullException(nameof(outputFile));
        _batchSize = batchSize > 0 ? batchSize : 100;
        Task.Run(() => ProcessQueueAsync(Cts.Token));
    }

    public void Add(TInput input)
    {
        if (IsValidInput(input))
        {
            InputQueue.Enqueue(input);
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
                while (batch.Count < _batchSize && InputQueue.TryDequeue(out var input) && IsValidInput(input))
                {
                    batch.Add(input);
                }

                if (batch.Count > 0)
                {
                    var processedBatch = batch.Select(ProcessInput);
                    lock (Lock)
                    {
                        Results.AddRange(processedBatch);
                        if (Results.Count >= _batchSize)
                        {
                            SaveToCsvAsync().GetAwaiter().GetResult(); // Synchronous for simplicity
                        }
                    }
                }
                else
                {
                    // Dynamic delay based on queue size
                    await Task.Delay(InputQueue.Count > 1000 ? 500 : 1000, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing batch: {ex.Message}");
            }
        }
    }

    private Task SaveToCsvAsync()
    {
        try
        {
            lock (Lock)
            {
                var csvLines = new[] { GetCsvHeader() }
                    .Concat(Results.Select(GetCsvLine));

                File.WriteAllLinesAsync(_outputFile, csvLines).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to CSV: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public IEnumerable<TProcessed> GetResults(Func<TProcessed, bool>? predicate = null)
    {
        lock (Lock)
        {
            return predicate == null
                ? Results.AsEnumerable()
                : Results.Where(predicate).AsEnumerable();
        }
    }

    public void Stop()
    {
        Cts.Cancel();
        try
        {
            Cts.Token.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            lock (Lock)
            {
                SaveToCsvAsync().GetAwaiter().GetResult();
                var count = Results.Count;
                var latest = Results.OrderByDescending(r => GetTimestamp(r)).FirstOrDefault();

                Console.WriteLine($"Final results: {count} items processed");
                if (latest != null)
                {
                    Console.WriteLine($"Latest result: {GetSummary(latest)}");
                }
            }
        }
        finally
        {
            Cts.Dispose();
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

        var error = string.Empty;
        if (input.IsInteresting && !_goldMiner.DustDictionary.TryAdd(input.Key, input))
        {
            error = $"Key {input.Key} already exists in DustDictionary";
        }
        return new ProcessedDust(input, input.IsInteresting ? error : $"Skipped p-value: {input.Regression.PValue}");
    }

    protected override string GetCsvHeader() => "Input,Timestamp,Comment,Processed";

    protected override string GetCsvLine(ProcessedDust result) =>
        $"{result.Input},{result.Timestamp:O},{result.Comment},{result.Processed}";

    protected override DateTime GetTimestamp(ProcessedDust result) => result.Timestamp;

    protected override string GetSummary(ProcessedDust result) =>
        $"Input={result.Input}, Timestamp={result.Timestamp:O}, Key={result.Input.Key}";
}

public class RegressionNamesProcessor(GoldMiner goldMiner, ConcurrentQueue<string> inputQueue)
    : QueueProcessor<string, RegressionNamesProcessor.ProcessedString>(goldMiner, inputQueue, "processed_strings.csv")
{
    public class ProcessedString(string input)
    {
        public string Input { get; set; } = input;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public int Length { get; set; } = input.Length;
        public int CharCount { get; set; } = input.Count(char.IsLetter);
        public bool Processed { get; set; } = true;
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