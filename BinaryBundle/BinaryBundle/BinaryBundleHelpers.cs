using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BinaryBundle; 

public static class BinaryBundleHelpers {
    /// <summary>
    /// Returns the provided array if their size matches with the second argument, otherwise creates a new array with the specified size
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="size"></param>
    public static void CreateArrayIfSizeDiffers<T>(ref T[]? array, int size) {
        if (array != null && array.Length == size) {
            return;
        }
        array = new T[size];
    }

    /// <summary>
    /// Returns the provided array if their size matches with the arguments, otherwise creates a new array with the specified sizes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="dimension0Size"></param>
    /// <param name="dimension1Size"></param>
    public static void CreateArrayIfSizeDiffers<T>(ref T[,]? array, int dimension0Size, int dimension1Size) {
        if (array != null && 
            array.GetLength(0) == dimension0Size &&
            array.GetLength(1) == dimension1Size) {
            return;
        }

        array = new T[dimension0Size, dimension1Size];
    }

    /// <summary>
    /// Returns the provided array if their size matches with the arguments, otherwise creates a new array with the specified sizes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="dimension0Size"></param>
    /// <param name="dimension1Size"></param>
    /// <param name="dimension2Size"></param>
    public static void CreateArrayIfSizeDiffers<T>(ref T[,,]? array, int dimension0Size, int dimension1Size, int dimension2Size) {
        if (array != null &&
            array.GetLength(0) == dimension0Size &&
            array.GetLength(1) == dimension1Size &&
            array.GetLength(2) == dimension2Size) {
            return;
        }

        array = new T[dimension0Size, dimension1Size, dimension2Size];
    }

    /// <summary>
    /// Returns the provided list with the new capacity. If list is null create a new one
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="size"></param>
    public static void ClearListAndPrepareCapacity<T>(ref List<T>? list, int size) {
        if (list == null) {
            list = new List<T>(size);
            return;
        }

        if (size > list.Capacity) {
            list.Capacity = size;
        }
        list.Clear();
    }

    /// <summary>
    /// Reads the size of a collection. Reads 1-4 bytes depending on the size. Can read numbers up to 268435456.
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    public static int ReadCollectionSize(IBundleReader reader) {
        byte firstByte = reader.ReadByte();
        // If the first continuation bit is 0, return the byte as is
        if ((firstByte & 0b1000_0000) == 0) {
            return firstByte;
        }

        // Store everything but the continuation bit
        int compoundSize = firstByte & 0b0111_1111;

        // Read up to 3 more times
        for (int i = 0; i < 3; i++) {
            byte nextByte = reader.ReadByte();
            // Move previous bits up and add the new bits sans their continuation bit
            compoundSize = (compoundSize << 7) | (nextByte & 0b0111_1111);
            // If the continuation bit isn't set, number is fully read
            if ((nextByte & 0b1000_0000) == 0) {
                return compoundSize;
            }
        }

        throw new Exception("Bits indicated an illegal read past 4 bytes");
    }

    /// <summary>
    /// Writes the number to the buffer. Writes 1-4 bytes depending on the numbers size. Can write numbers up to 268435456.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="size"></param>
    public static void WriteCollectionSize(IBundleWriter writer, int size) {
        // Early return for common single byte case
        if (size < 0x80) {
            writer.WriteByte((byte)size);
            return;
        }

        if (size >= (0x1 << (7 * 4))) {
            throw new Exception("Maximum size exceeded");
        }

        for (int i = 3; i > 0; i--) {
            if (size >= (1 << (7 * i))) {
                // Get 7 bits from the number
                byte value = (byte)(size >> (7*i));
                // Add a leading bit to say that the value should continue to be read
                value |= 0b1000_0000;
                writer.WriteByte(value);
            }
        }


        // Write the last one without continuation bit
        writer.WriteByte((byte)(size & 0x7F));

    }
}