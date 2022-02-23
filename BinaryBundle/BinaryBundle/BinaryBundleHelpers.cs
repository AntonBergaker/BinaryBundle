using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryBundle; 

public static class BinaryBundleHelpers {
    /// <summary>
    /// Returns the provided array if their size matches with the second argument, otherwise creates a new array with the specified size
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="oldArray"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static T[] CreateArrayIfSizeDiffers<T>(T[]? oldArray, int size) {
        if (oldArray != null && oldArray.Length == size) {
            return oldArray;
        }
        return new T[size];
    }

    /// <summary>
    /// Returns the provided array if their size matches with the arguments, otherwise creates a new array with the specified sizes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="oldArray"></param>
    /// <param name="dimension0Size"></param>
    /// <param name="dimension1Size"></param>
    /// <returns></returns>
    public static T[,] CreateArrayIfSizeDiffers<T>(T[,]? oldArray, int dimension0Size, int dimension1Size) {
        if (oldArray != null && 
            oldArray.GetLength(0) == dimension0Size &&
            oldArray.GetLength(1) == dimension1Size) {
            return oldArray;
        }

        return new T[dimension0Size, dimension1Size];
    }

    /// <summary>
    /// Returns the provided array if their size matches with the arguments, otherwise creates a new array with the specified sizes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="oldArray"></param>
    /// <param name="dimension0Size"></param>
    /// <param name="dimension1Size"></param>
    /// <param name="dimension2Size"></param>
    /// <returns></returns>
    public static T[,,] CreateArrayIfSizeDiffers<T>(T[,,]? oldArray, int dimension0Size, int dimension1Size, int dimension2Size) {
        if (oldArray != null &&
            oldArray.GetLength(0) == dimension0Size &&
            oldArray.GetLength(1) == dimension1Size &&
            oldArray.GetLength(2) == dimension2Size) {
            return oldArray;
        }

        return new T[dimension0Size, dimension1Size, dimension2Size];
    }
}