using System.Collections.Generic;
using System.Text;

namespace NVespa.NInternalUsage;

public class GenericArrayCache<T> : SingletonAuto<GenericArrayCache<T>>, IGenericArrayCache
{
	public class Buffer
	{
		public T[] Array;

		public int Size;
	}

	internal class UnusedBuffer
	{
		public Queue<Buffer> UnusedQueue = new Queue<Buffer>();
	}

	public object LockObject;

	internal Buffer _emptyBuffer;

	internal Dictionary<int, UnusedBuffer> _unusedDictionary;

	protected override bool ThreadSafe => true;

	public GenericArrayCache()
	{
		LockObject = new object();
		_emptyBuffer = new Buffer();
		_emptyBuffer.Size = 0;
		_emptyBuffer.Array = new T[1];
		_unusedDictionary = new Dictionary<int, UnusedBuffer>();
		SingletonAuto<ArrayCacheContainer>.instance.Add(this);
	}

	void IGenericArrayCache.OnSceneChanged()
	{
		lock (LockObject)
		{
			_unusedDictionary = new Dictionary<int, UnusedBuffer>();
		}
	}

	void IGenericArrayCache.DumpSummary(StringBuilder builder)
	{
	}

	public int CalcBufferSize(int size, int baseCount)
	{
		int num = baseCount;
		for (int i = 0; i < 100; i++)
		{
			num = baseCount << i;
			if (size <= num)
			{
				break;
			}
		}
		return num;
	}

	public Buffer GetEmptyBuffer()
	{
		return _emptyBuffer;
	}

	public Buffer CreateBuffer(int size, int baseCount)
	{
		lock (LockObject)
		{
			int arraySize = CalcBufferSize(size, baseCount);
			UnusedBuffer unusedBuffer = GetUnusedBuffer(arraySize);
			if (unusedBuffer.UnusedQueue.Count > 0)
			{
				return unusedBuffer.UnusedQueue.Dequeue();
			}
			return CreateNewBuffer(arraySize);
		}
	}

	private Buffer CreateNewBuffer(int arraySize)
	{
		return new Buffer
		{
			Array = new T[arraySize],
			Size = arraySize
		};
	}

	public void ReleaseBuffer(Buffer buffer)
	{
		if (buffer == _emptyBuffer)
		{
			return;
		}
		lock (LockObject)
		{
			GetUnusedBuffer(buffer.Size).UnusedQueue.Enqueue(buffer);
		}
	}

	private UnusedBuffer GetUnusedBuffer(int arraySize)
	{
		if (!_unusedDictionary.TryGetValue(arraySize, out var value))
		{
			value = new UnusedBuffer();
			_unusedDictionary.Add(arraySize, value);
		}
		return value;
	}
}
