using System.Collections.Concurrent;

namespace DataMiner;



public class RegressionNamesProcessor
{
    private readonly ConcurrentQueue<string> _inputQueue; // Reference to GoldMiner.DustsQueue
    private readonly List<ProcessedString> _results = [];
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly object _lock = new object();
    private readonly GoldMiner _goldMiner;

    public class ProcessedString(string input)
    {
        public string Input { get; set; } = input;
        public DateTime Timestamp { get; set; }
        public int Length { get; set; }
        public int CharCount { get; set; }
        public bool Processed { get; set; }
    }

    public RegressionNamesProcessor(GoldMiner goldMiner, ConcurrentQueue<string> inputQueue)
    {
        _goldMiner = goldMiner ?? throw new ArgumentNullException(nameof(goldMiner));
        _inputQueue = inputQueue ?? throw new ArgumentNullException(nameof(inputQueue));
        // Start background processing task
        Task.Run(() => ProcessStringsAsync(_cts.Token));
    }

    public void AddString(string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            _inputQueue.Enqueue(input);
            Console.WriteLine($"Added regression name: {input}");
        }
    }

    private async Task ProcessStringsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_goldMiner.DustQueue.Count > 40000)
            {
                // sleep for a couple of seconds if the queue is too large
                await Task.Delay(20000, cancellationToken);
            }
            try
            {
                if (_inputQueue.TryDequeue(out string? input) && !string.IsNullOrEmpty(input))
                {
                    // Create dusts and queue them for storage


                    foreach (var dust in _goldMiner.GoldDust(input))
                    {
                        _goldMiner.DustQueue.Enqueue(dust);
                    }

                    // Create metadata using LINQ
                    var processed = new ProcessedString(input)
                    {
                        Input = input,
                        Timestamp = DateTime.UtcNow,
                        Length = input.Length,
                        CharCount = input.Count(char.IsLetter), // LINQ Count for regression placeholder
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
                    await Task.Delay(1000, cancellationToken);
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
                var csvLines = new[] { "Input,Timestamp,Length,CharCount,Processed" }
                    .Concat(_results.Select(r =>
                        $"{r.Input},{r.Timestamp:O},{r.Length},{r.CharCount},{r.Processed}"))
                    .ToList();

                File.WriteAllLines("processed_strings.csv", csvLines);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to CSV: {ex.Message}");
        }
    }

    // Query results using LINQ
    public IEnumerable<ProcessedString> GetProcessedStrings(Func<ProcessedString, bool>? predicate = null)
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
                    $"Timestamp={latest.Timestamp:O}, Length={latest.Length}");
            }
        }
    }
}
