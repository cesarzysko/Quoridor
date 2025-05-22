using System;
using System.Collections.Generic;

public static class ArrayUtilities
{
	public static void Fill<T>(this T[] array, T element) 
		where T : struct
	{
		if (array == null) { return; }
		
		int length = array.Length;
		for (int i = 0; i < length; ++i)
		{
			array[i] = element;
		}
	}

	public static int IndexOf<T>(this T[] array, T element)
		where T : IEquatable<T>
	{
		if (array == null) { return -1; }

		int length = array.Length;
		for (int i = 0; i < length; ++i)
		{
			if (array[i].Equals(element))
			{
				return i;
			}
		}

		return -1;
	}

	public static int IndexOf<T>(this T[] array, Predicate<T> predicate)
	{
		if (array == null || predicate == null) { return -1; }
		
		int length = array.Length;
		for (int i = 0; i < length; ++i)
		{
			if (predicate.Invoke(array[i]))
			{
				return i;
			}
		}

		return -1;
	}

	public static int[] IndexesOf<T>(this T[] array, Predicate<T> predicate)
	{
		if (array == null || predicate == null) { return Array.Empty<int>(); }
		
		List<int> indexes = new();
		int count = array.Length;
		for (int i = 0; i < count; ++i)
		{
			if (predicate.Invoke(array[i]))
			{
				indexes.Add(i);
			}
		}

		return indexes.ToArray();
	}

	public static T[] Filter<T>(this T[] array, Predicate<T> predicate)
	{
		if (array == null || predicate == null) { return Array.Empty<T>(); }
		
		int count = array.Length;
		List<T> filteredElements = new List<T>(count);
		for (int i = 0; i < count; ++i)
		{
			if (predicate.Invoke(array[i]))
			{
				filteredElements.Add(array[i]);
			}
		}

		return filteredElements.ToArray();
	}

	public static void ForEach<T>(this T[] array, Action<T> action)
	{
		if (array == null || action == null) { return; }

		int count = array.Length;
		for (int i = 0; i < count; ++i)
		{
			action.Invoke(array[i]);
		}
	}

	public static string ElementsToString<T>(this T[] array, string separator)
	{
		if (array == null) { return string.Empty; }

		string message = "";
		int count = array.Length;
		for (int i = 0; i < count; ++i)
		{
			message += array[i].ToString();
			if (i < count - 1)
			{
				message += separator;
			}
		}

		return message;
	}
}
