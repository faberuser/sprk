using System.Collections.Generic;

namespace NVespa.NInternalUsage;

public static class GenericCircularCache<T>
{
	public class Buffer
	{
		public CircularBuffer<T> Circular;

		public int BitShift;

		internal Buffer _unusedNext;
	}

	private class UnusedBuffer
	{
		public Buffer Head;

		public Buffer Pop()
		{
			if (Head != null)
			{
				Buffer head = Head;
				Head = head._unusedNext;
				head._unusedNext = null;
				return head;
			}
			return null;
		}

		public void Put(Buffer buffer)
		{
			buffer._unusedNext = Head;
			Head = buffer;
		}
	}

	public const int MinimalBitShift = 5;

	private static object _lockObject;

	private static Dictionary<int, UnusedBuffer> _unusedDictionaryInternal;

	public static object LockObject
	{
		get
		{
			if (_lockObject == null)
			{
				_lockObject = new object();
			}
			return _lockObject;
		}
	}

	private static Dictionary<int, UnusedBuffer> _unusedDictionary
	{
		get
		{
			if (_unusedDictionaryInternal == null)
			{
				_unusedDictionaryInternal = new Dictionary<int, UnusedBuffer>();
			}
			return _unusedDictionaryInternal;
		}
	}

	public static Buffer CreateBuffer(int size)
	{
		lock (LockObject)
		{
			int num = 1;
			int num2 = 0;
			for (int i = 5; i < 100; i++)
			{
				num = 1 << i;
				num2 = i;
				if (size <= num)
				{
					break;
				}
			}
			UnusedBuffer vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_unusedDictionary, num2, null);
			if (vALUE != null)
			{
				Buffer buffer = vALUE.Pop();
				if (buffer != null)
				{
					return buffer;
				}
			}
			return CreateNewBuffer(num, num2);
		}
	}

	private static Buffer CreateNewBuffer(int arraySize, int bitShift)
	{
		return new Buffer
		{
			Circular = new CircularBuffer<T>(arraySize),
			BitShift = bitShift
		};
	}

	public static void ReleaseBuffer(Buffer buffer)
	{
		lock (LockObject)
		{
			buffer.Circular.Clear();
			UnusedBuffer unusedBuffer = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_unusedDictionary, buffer.BitShift, null);
			if (unusedBuffer == null)
			{
				unusedBuffer = new UnusedBuffer();
				_unusedDictionary.Add(buffer.BitShift, unusedBuffer);
			}
			unusedBuffer.Put(buffer);
		}
	}
}
