namespace DataMiner;

public static class Deduplication
{
    //protected void DeduplicateAndSortDusts()
    //{
    //    //    _dust.Clear();
    //    var productionDusts = Deduplication
    //        .RemoveDuplicatesByGuid(_dust
    //            .ToArray())
    //        .OrderByDescending(d => d.Regression.PValue);

    //    _dust.AddRange(productionDusts);
    //}

    public static List<Dust> RemoveDuplicatesByGuid(Dust[] objects)
    {
        // Use HashSet to track seen GUIDs
        var seenGuids = new HashSet<Guid>();

        // Use List to store unique objects, preserving order
        var result = new List<Dust>(objects.Length);
        result.AddRange(objects.Where(obj => seenGuids.Add(obj.UniqueKey)));

        return result;
    }
}