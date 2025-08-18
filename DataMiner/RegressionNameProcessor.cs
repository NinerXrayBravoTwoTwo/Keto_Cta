using System.Collections.Concurrent;

namespace DataMiner;

// Assuming GoldMiner class with DustsQueue
public partial class GoldMiner
{
    public static readonly ConcurrentQueue<string> RegressionNameQueue = new ConcurrentQueue<string>();
}

public class RegressionNamesProcessor
{
    private readonly ConcurrentQueue<string> _inputQueue; // Reference to GoldMiner.DustsQueue
    private readonly List<ProcessedString> _results = new List<ProcessedString>();
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly object _lock = new object();

    public class ProcessedString
    {
        public string Input { get; set; }
        public DateTime Timestamp { get; set; }
        public int Length { get; set; }
        public int CharCount { get; set; }
        public bool Processed { get; set; }
    }

    public RegressionNamesProcessor(ConcurrentQueue<string> inputQueue)
    {
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
            try
            {
                if (_inputQueue.TryDequeue(out string input))
                {
                    // Create metadata using LINQ
                    var processed = new ProcessedString
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
    public IEnumerable<ProcessedString> GetProcessedStrings(Func<ProcessedString, bool> predicate = null)
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

class Program
{
    // Example PermutationsA method (replace with your actual implementation)
    static IEnumerable<string> PermutationsA(IEnumerable<string> visitAttributes, IEnumerable<string> elementAttributes)
    {
        // Example: Combine attributes into strings (replace with your permutation logic)
        return from v in visitAttributes
               from e in elementAttributes
               select $"{v}-{e}";
    }

    static async Task Main()
    {
        // Initialize processor with GoldMiner.DustsQueue
        var processor = new RegressionNamesProcessor(GoldMiner.RegressionNameQueue);

        // Example data
        var visitAttributes = new List<string> { "visit1", "visit2" };
        var elementAttributes = new List<string> { "elem1", "elem2" };

        // Enqueue permutations using LINQ
        PermutationsA(visitAttributes, elementAttributes)
            .ToList() // Materialize to avoid multiple enumerations
            .ForEach(item => GoldMiner.RegressionNameQueue.Enqueue(item));

        // Wait for processing
        await Task.Delay(2000);

        // Example LINQ queries
        var longStrings = processor.GetProcessedStrings(r => r.Length > 10);
        Console.WriteLine("\nStrings longer than 10 characters:");
        foreach (var result in longStrings)
        {
            Console.WriteLine($"Input: {result.Input}, Length: {result.Length}");
        }

        var recentStrings = processor.GetProcessedStrings(r => r.Timestamp > DateTime.UtcNow.AddSeconds(-10));
        Console.WriteLine("\nStrings processed in last 10 seconds:");
        foreach (var result in recentStrings)
        {
            Console.WriteLine($"Input: {result.Input}, Timestamp: {result.Timestamp:O}");
        }

        Console.WriteLine("\nPress any key to stop...");
        Console.ReadKey();
        processor.Stop();
    }
}
