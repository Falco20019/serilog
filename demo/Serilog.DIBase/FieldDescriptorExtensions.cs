namespace Google.Protobuf.Reflection;

/// <summary>
/// Summary of common extensions on <see cref="FieldDescriptor"/>.
/// </summary>
public static class FieldDescriptorExtensions
{
    /// <summary>
    /// Checks if the is_hidden option of the field is set to true.
    /// </summary>
    /// <param name="field">The descriptor of the field to check.</param>
    /// <returns>True, if is_hidden is set and true.</returns>
    public static bool IsHidden(this FieldDescriptor field)
    {
        var fieldOptions = field.GetOptions();
        return fieldOptions != null &&
               fieldOptions.HasExtension(DescriptorExtensions.IsHidden) &&
               fieldOptions.GetExtension(DescriptorExtensions.IsHidden);
    }
}
