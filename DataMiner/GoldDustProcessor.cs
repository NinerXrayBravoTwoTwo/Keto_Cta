using System.Collections.Concurrent;
using Keto_Cta;

namespace DataMiner;

// Assuming GoldMiner class with DustsQueue 

public class GoldDustProcessor
{
    private readonly ConcurrentQueue<Dust> _inputQueue; // Reference to GoldMiner.DustsQueue
    private readonly List<ProcessedDust> _results = [];
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly object _lock = new object();
    private readonly GoldMiner _goldMiner;

    public class ProcessedDust(Dust input)
    {
        public Dust Input { get; set; } = input;
        public DateTime Timestamp { get; set; }
        public Guid UniqueKey { get; set; }
        public SetName SetName { get; set; }
        public double Samples{ get; set; }
        public bool Processed { get; set; }
    }

    public GoldDustProcessor(GoldMiner goldMiner, ConcurrentQueue<Dust> inputQueue)
    {
        _goldMiner = goldMiner ?? throw new ArgumentNullException(nameof(goldMiner));
        _inputQueue = inputQueue ?? throw new ArgumentNullException(nameof(inputQueue));
        // Start background processing task
        Task.Run(() => ProcessStringsAsync(_cts.Token));
    }

    public void AddString(Dust input)
    {
        if (!input.UniqueKey.Equals( Guid.Empty))
        {
            _inputQueue.Enqueue(input);
            Console.WriteLine($"Added regression name: {input}");
        }
    }

    private async Task ProcessStringsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_inputQueue.TryDequeue(out Dust? input) && !input.UniqueKey.Equals(Guid.Empty))
                {
                    _ = _goldMiner.DustDictionary.TryAdd(input.UniqueKey, input);

                    // Create metadata using LINQ
                    var processed = new ProcessedDust(input)
                    {
                        Input = input,
                        Timestamp = DateTime.UtcNow,
                        SetName = input.SetName,
                        UniqueKey = input.UniqueKey,
                        Samples = input.Regression.N,
                        Processed = true
                    };

                    // Add to results
                    lock (_lock)
                    {
                        _results.Add(processed);

                        // Periodically save to CSV
                        if (_results.Count % 10 == 0)
                        {
                            SaveToCsv();
                        }
                    }
                }
                else
                {
                    // No items in queue, wait briefly
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing string: {ex.Message}");
            }
        }
    }

    private void SaveToCsv()
    {
        try
        {
            lock (_lock)
            {
                // Use LINQ to transform results into CSV format
                var csvLines = new[] { "Input,Timestamp,UniqueKey,SetName,Samples,Processed" }
                    .Concat(_results.Select(r =>
                        $"{r.Input},{r.Timestamp:O}{r.UniqueKey},{r.SetName},{r.Input.Regression.N},{r.Processed}"))
                    .ToList();

                File.WriteAllLines("processed_dust.csv", csvLines);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to CSV: {ex.Message}");
        }
    }

    // Query results using LINQ
    public IEnumerable<ProcessedDust> GetProcessedStrings(Func<ProcessedDust, bool>? predicate = null)
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
        lock (_lock)
        {
            SaveToCsv();
            // Use LINQ to summarize results
            var count = _results.Count;
            var latest = _results.OrderByDescending(r => r.Timestamp).FirstOrDefault();

            Console.WriteLine($"Final results: {count} items processed");
            if (latest != null)
            {
                Console.WriteLine($"Latest result: Input={latest.Input}, " +
                    $"Timestamp={latest.Timestamp:O}, Key={latest.UniqueKey}");
            }
        }
    }
}