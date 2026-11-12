using System;
using System.Collections.Generic;

namespace NShared;

public class ArrayPool<T>
{
	public class AutoArrayPoolReleaser : IDisposable
	{
		private T[] _value;

		public T this[int index]
		{
			get
			{
				return _value[index];
			}
			set
			{
				_value[index] = value;
			}
		}

		internal AutoArrayPoolReleaser(int length)
		{
			_value = ArrayPool<T>.Alloc(length);
		}

		void IDisposable.Dispose()
		{
			ArrayPool<T>.Release(_value);
		}

		public static implicit operator T[](AutoArrayPoolReleaser array)
		{
			return array?._value;
		}

		public bool Exist(T value, Func<T, T, bool> equalityFunc, int maxCheckCount)
		{
			if (_value == null || _value.Length == 0)
			{
				return false;
			}
			maxCheckCount = ((maxCheckCount > 0) ? maxCheckCount : _value.Length);
			for (int i = 0; i < maxCheckCount; i++)
			{
				if (_value[i] != null && equalityFunc(_value[i], value))
				{
					return true;
				}
			}
			return false;
		}
	}

	public const int MaxPoolSize = 10;

	private static readonly Dictionary<int, Queue<T[]>> _poolDictionary;

	static ArrayPool()
	{
		_poolDictionary = new Dictionary<int, Queue<T[]>>();
	}

	public static T[] Alloc(int length)
	{
		Queue<T[]> value = null;
		lock (_poolDictionary)
		{
			_poolDictionary.TryGetValue(length, out value);
		}
		if (value != null && value.Count > 0)
		{
			T[] array = null;
			lock (value)
			{
				if (value.Count > 0)
				{
					array = value.Dequeue();
				}
			}
			if (array != null)
			{
				return array;
			}
		}
		return new T[length];
	}

	public static AutoArrayPoolReleaser AllocAuto(int length)
	{
		return new AutoArrayPoolReleaser(length);
	}

	public static AutoArrayPoolReleaser AllocAuto(T[] src)
	{
		int num = ((src != null) ? src.Length : 0);
		AutoArrayPoolReleaser autoArrayPoolReleaser = new AutoArrayPoolReleaser(num);
		for (int i = 0; i < num; i++)
		{
			autoArrayPoolReleaser[i] = src[i];
		}
		return autoArrayPoolReleaser;
	}

	public static void Release(T[] released)
	{
		if (released == null)
		{
			return;
		}
		int num = released.Length;
		Queue<T[]> value = null;
		lock (_poolDictionary)
		{
			_poolDictionary.TryGetValue(num, out value);
			if (value != null && value.Count > 10)
			{
				return;
			}
			if (value == null)
			{
				value = new Queue<T[]>();
				_poolDictionary.Add(num, value);
			}
		}
		for (int i = 0; i < num; i++)
		{
			released[i] = default(T);
		}
		if (value == null)
		{
			return;
		}
		lock (value)
		{
			value.Enqueue(released);
		}
	}
}
