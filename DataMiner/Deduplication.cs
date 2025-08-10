namespace DataMiner;

public static class Deduplication
{
    public static List<Dust> RemoveDuplicatesByGuid(Dust[] objects)
    {
        // Use HashSet to track seen GUIDs
        var seenGuids = new HashSet<Guid>();

        // Use List to store unique objects, preserving order
        var result = new List<Dust>(objects.Length);

        foreach (var obj in objects)
        {
            // Add to result only if GUID hasn't been seen
            if (seenGuids.Add(obj.UniqueKey))
            {
                result.Add(obj);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Duplicate: {obj.RegressionName}");
            }
        }

        return result;
    }
}