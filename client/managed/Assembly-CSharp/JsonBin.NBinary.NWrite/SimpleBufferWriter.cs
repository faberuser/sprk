using System;
using System.Collections.Generic;
using JsonBin.NBinary.NRead;

namespace JsonBin.NBinary.NWrite;

public class SimpleBufferWriter : IDisposable
{
	public class BufferPool
	{
		private static Dictionary<int, Queue<byte[]>> _dicBySize = new Dictionary<int, Queue<byte[]>>();

		public static byte[] Alloc(int size)
		{
			if (!_dicBySize.TryGetValue(size, out var value))
			{
				value = null;
			}
			if (value != null && value.Count > 0)
			{
				return value.Dequeue();
			}
			return new byte[size];
		}

		public static void Release(byte[] buffer)
		{
			if (buffer != null && buffer.Length != 0)
			{
				if (!_dicBySize.TryGetValue(buffer.Length, out var value))
				{
					value = new Queue<byte[]>();
					_dicBySize.Add(buffer.Length, value);
				}
				value.Enqueue(buffer);
			}
		}
	}

	private byte[] _buffer;

	private int _position;

	public SimpleBufferWriter()
	{
		_buffer = BufferPool.Alloc(1048576);
		_position = 0;
	}

	public void Dispose()
	{
		BufferPool.Release(_buffer);
	}

	public ReadonlyBuffer ToArray()
	{
		return ReadonlyBuffer.CreateBuffer(_buffer, _position);
	}

	public byte[] ToArrayFinal()
	{
		byte[] array = new byte[_position];
		Array.Copy(_buffer, array, _position);
		return array;
	}

	public void Advance(int count)
	{
		Ensure(count);
		_position += count;
	}

	public JsonBinSpan GetSpan(int sizeHint)
	{
		sizeHint = Math.Max(sizeHint, 1);
		Ensure(sizeHint);
		return new JsonBinSpan(_buffer, _position);
	}

	private void Ensure(int count)
	{
		if (_position + count > _buffer.Length)
		{
			int num = _buffer.Length;
			while (_position + count > num)
			{
				num *= 2;
			}
			byte[] buffer = _buffer;
			_buffer = BufferPool.Alloc(num);
			Array.Copy(buffer, _buffer, buffer.Length);
			BufferPool.Release(buffer);
		}
	}
}
