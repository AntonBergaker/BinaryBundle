
using System;

namespace BinaryBundle;
public class BundleCollectionLimitExceededException : Exception {
    public BundleCollectionLimitExceededException(int countTriedToWrite, int limit) {
        CountTriedToWrite = countTriedToWrite;
        Limit = limit;
    }

    public int CountTriedToWrite { get; }
    public int Limit { get; }
}
