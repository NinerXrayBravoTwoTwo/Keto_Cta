using System.Collections.Concurrent;
using System.Reflection;

namespace DataMiner;

public class MazeTwistyPassages
{
    #region Reflection and Nested Property Retrieval

    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropertyCache = new();

    private static readonly ConcurrentDictionary<string, List<(string PropName, int Index)>> ParsedPaths = new();

    /// <summary>
    /// Retrieves the value of a nested property from an object, navigating through a dot-separated property path.
    /// </summary>
    /// <remarks>This method supports accessing indexed properties, such as elements in a list, using
    /// square bracket notation  (e.g., "Items[0]"). If an index is out of range or the property path is invalid,
    /// the method returns <see langword="null"/>. Property information is cached for performance
    /// optimization.</remarks>
    /// <param name="obj">The object from which to retrieve the property value. Can be <see langword="null"/>.</param>
    /// <param name="propertyPath">A dot-separated string representing the path to the nested property.  For example, "Parent.Child.Property"
    /// or "Items[0].Name".</param>
    /// <returns>The value of the nested property if found; otherwise, <see langword="null"/>.  Returns <see
    /// langword="null"/> if <paramref name="obj"/> is <see langword="null"/>,  <paramref name="propertyPath"/> is
    /// <see langword="null"/> or empty, or if any part of the path is invalid.</returns>
    public static object? GetNestedPropertyValue(object? obj, string propertyPath)
    {
        if (obj == null || string.IsNullOrEmpty(propertyPath))
            return null;

        if (!ParsedPaths.TryGetValue(propertyPath, out var parts))
        {
            parts = BuildParsedPath(propertyPath);
            if (parts == null) return null;
            ParsedPaths[propertyPath] = parts;
        }

        object? current = obj;

        foreach (var (propName, index) in parts)
        {
            if (current == null) return null;

            var type = current.GetType();
            var cacheKey = (type, propName);
            if (!PropertyCache.TryGetValue(cacheKey, out var propInfo))
            {
                propInfo = type.GetProperty(propName);
                PropertyCache[cacheKey] = propInfo;
            }

            if (propInfo == null) return null;

            current = propInfo.GetValue(current);

            if (index == -1) continue;

            if (current is not System.Collections.IList list) return null;

            if (index < 0 || index >= list.Count) return null;

            current = list[index];
        }

        return current;
    }

    private static List<(string PropName, int Index)>? BuildParsedPath(string propertyPath)
    {
        var pathParts = propertyPath.Split('.');
        var parsed = new List<(string, int)>();

        foreach (var part in pathParts)
        {
            if (string.IsNullOrEmpty(part)) return null;

            int bracketIndex = part.IndexOf('[');
            string propName;
            int index = -1;

            if (bracketIndex != -1)
            {
                if (!part.EndsWith(']')) return null;
                propName = part.Substring(0, bracketIndex);
                var indexStr = part.Substring(bracketIndex + 1, part.Length - bracketIndex - 2);
                if (!int.TryParse(indexStr, out index) || index < 0) return null;
            }
            else
            {
                propName = part;
            }

            if (string.IsNullOrEmpty(propName) || !(char.IsLetter(propName[0]) || propName[0] == '_')) return null;
            for (int i = 1; i < propName.Length; i++)
            {
                if (!(char.IsLetterOrDigit(propName[i]) || propName[i] == '_')) return null;
            }

            parsed.Add((propName, index));
        }

        return parsed;
    }

    //public static object? GetElementPropertyValue(Element element, string path)
    //{
    //    if (element == null || string.IsNullOrWhiteSpace(path))
    //        return null;
    //    // Handle direct properties of Element
    //    if (path == "PropertyName")
    //        return element.PropertyName;
    //    // Handle Visits array with index
    //    if (path.StartsWith("Visits[") && path.EndsWith("].PropertyName"))
    //    {
    //        var indexStart = path.IndexOf('[') + 1;
    //        var indexEnd = path.IndexOf(']');
    //        if (int.TryParse(path.Substring(indexStart, indexEnd - indexStart), out int index))
    //        {
    //            if (index >= 0 && index < element.Visits.Length)
    //                return element.Visits[index]?.PropertyName;
    //        }
    //    }
    //    // Path not supported
    //    return null;
    //}


    //private static object? GetElementPropertyValue(object? obj, string propertyPath)
    //{
    //    var 
    //}

    //private static object? GetVisitPropertyValue(object? string propertyPath)
    //{

    //}

    #endregion
}