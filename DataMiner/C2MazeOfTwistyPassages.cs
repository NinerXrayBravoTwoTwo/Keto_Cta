using System.Reflection;
using System.Text.RegularExpressions;

namespace DataMiner;

public partial class CreateSelectorTwo
{
    #region Reflection and Nested Property Retrieval

    private static readonly Regex PropertyRegex =
        new(@"([a-zA-Z_][a-zA-Z0-9_]*)(\[(\d+)\])?", RegexOptions.Compiled);

    private static readonly Dictionary<string, PropertyInfo?> PropertyCache = new();

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
    private static object? GetNestedPropertyValue(object? obj, string propertyPath)
    {
        if (string.IsNullOrEmpty(propertyPath) || obj == null)
            return null;

        var properties = propertyPath.Split('.');

        var current = obj;

        foreach (var property in properties)
        {
            if (current == null) return null;

            var match = PropertyRegex.Match(property);
            if (!match.Success) return null;

            var propName = match.Groups[1].Value;
            var cacheKey = $"{current.GetType().FullName}.{propName}";
            if (!PropertyCache.TryGetValue(cacheKey, out var propInfo))
            {
                propInfo = current.GetType().GetProperty(propName);
                PropertyCache[cacheKey] = propInfo;
            }

            if (propInfo == null) return null;

            current = propInfo.GetValue(current);

            if (!match.Groups[2].Success || current is not System.Collections.IList list) continue;
            var index = int.Parse(match.Groups[3].Value);
            current = index >= 0 && index < list.Count ? list[index] : null;
        }

        return current;
    }

    //private static object? GetElementPropertyValue(object? obj, string propertyPath)
    //{
    //    var 
    //}

    //private static object? GetVisitPropertyValue(object? string propertyPath)
    //{

    //}

    #endregion
}