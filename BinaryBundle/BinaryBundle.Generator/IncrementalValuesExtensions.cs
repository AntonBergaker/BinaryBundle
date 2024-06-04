
using Microsoft.CodeAnalysis;

namespace BinaryBundle.Generator;
public static class IncrementalValuesExtensions {

    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> provider) {
        return provider.Where(static x => x is not null)!;
    }

}
