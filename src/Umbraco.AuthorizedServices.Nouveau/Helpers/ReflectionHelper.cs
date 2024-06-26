using System.Reflection;

namespace Umbraco.AuthorizedServices.Helpers;

internal static class ReflectionHelper
{
    /// <summary>
    /// Gets a property value by reflection. If the property does not exist, a default value is returned.
    /// </summary>
    /// <typeparam name="T">Object type.</typeparam>
    /// <param name="obj">Object from which to get property value.</param>
    /// <param name="propertyName">Name of property.</param>
    /// <param name="defaultValue">Default value to use if property not available.</param>
    /// <remarks>
    /// This is used for accessing properties added in a supported version of Umbraco that's later than the
    /// one we take a dependency on.
    /// </remarks>
    /// <returns>The property value, or the default.</returns>
    public static T GetOptionalPropertyValue<T>(this object obj, string propertyName, T defaultValue)
    {
        PropertyInfo? propertyInfo = GetPropertyInfo(obj, propertyName);
        if (propertyInfo == null || !propertyInfo.CanRead)
        {
            return defaultValue;
        }

        var value = propertyInfo.GetValue(obj, null);
        if (value == null)
        {
            return defaultValue;
        }

        if (value is T t)
        {
            return t;
        }

        return defaultValue;
    }

    /// <summary>
    /// Sets a property value by reflection. If the property does not exist, no exception is thrown.
    /// </summary>
    /// <typeparam name="T">Object type.</typeparam>
    /// <param name="obj">Object on which to set property value.</param>
    /// <param name="propertyName">Name of property.</param>
    /// <param name="value">Value to set.</param>
    /// <remarks>
    /// See notes on <see cref="GetOptionalPropertyValue{T}(object, string, T)"/>
    /// </remarks>
    public static void SetOptionalPropertyValue<T>(this object obj, string propertyName, T value)
    {
        PropertyInfo? propertyInfo = GetPropertyInfo(obj, propertyName);
        if (propertyInfo == null || !propertyInfo.CanWrite)
        {
            return;
        }

        if (propertyInfo.PropertyType.IsAssignableFrom(typeof(T)))
        {
            propertyInfo.SetValue(obj, value, null);
        }
    }

    private static PropertyInfo? GetPropertyInfo(object obj, string propertyName) => obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
}
