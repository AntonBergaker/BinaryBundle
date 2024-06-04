
using System;

namespace BinaryBundle;
public class BundleCollectionLimitExceededException : Exception {
    public BundleCollectionLimitExceededException(object collection, int countTriedToWrite, int limit) {
        Collection = collection;
        CountTriedToWrite = countTriedToWrite;
        Limit = limit;
    }

    public object Collection { get; }
    public int CountTriedToWrite { get; }
    public int Limit { get; }
}
