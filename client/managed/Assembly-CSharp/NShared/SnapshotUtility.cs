using System.Collections.Generic;
using NVespa;

namespace NShared;

public class SnapshotUtility
{
	private class ValueTypeChecker<T>
	{
		public static bool IsValueType;

		static ValueTypeChecker()
		{
			IsValueType = typeof(T).IsValueType;
		}
	}

	public static void IncRefCount<T>(T value)
	{
		if (!ValueTypeChecker<T>.IsValueType && value is ISnapshotObject snapshotObject)
		{
			snapshotObject.RefCounter.Increment();
		}
	}

	public static void IncRefCountList<T>(IEnumerable<T> values)
	{
		if (values == null || ValueTypeChecker<T>.IsValueType)
		{
			return;
		}
		foreach (T value in values)
		{
			IncRefCount(value);
		}
	}

	public static void IncRefCountList<T>(T[] values)
	{
		if (values != null && !ValueTypeChecker<T>.IsValueType)
		{
			for (int i = 0; i < values.Length; i++)
			{
				IncRefCount(values[i]);
			}
		}
	}

	public static void DecRefCount<T>(T value)
	{
		if (!ValueTypeChecker<T>.IsValueType && value is ISnapshotObject snapshotObject)
		{
			int num = snapshotObject.RefCounter.Decrement();
			if (num < 0)
			{
				VespaDebug.LogError("Releasing already pooled object. " + snapshotObject.GetTypeName());
			}
			else if (num == 0 && snapshotObject.Pool != null)
			{
				snapshotObject.Pool.Release(value);
			}
		}
	}

	public static void DecRefCountList<T>(IEnumerable<T> values)
	{
		if (values == null || ValueTypeChecker<T>.IsValueType)
		{
			return;
		}
		foreach (T value in values)
		{
			DecRefCount(value);
		}
	}

	public static void DecRefCountList<T>(T[] values)
	{
		if (values != null && !ValueTypeChecker<T>.IsValueType)
		{
			for (int i = 0; i < values.Length; i++)
			{
				DecRefCount(values[i]);
			}
		}
	}
}
