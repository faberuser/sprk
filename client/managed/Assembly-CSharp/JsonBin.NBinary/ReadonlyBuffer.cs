using System;
using System.Collections.Generic;

namespace JsonBin.NBinary;

public class ReadonlyBuffer : IDisposable
{
	public const int BaseSize = 1024;

	private static Dictionary<int, Queue<ReadonlyBuffer>> _dicBySize = new Dictionary<int, Queue<ReadonlyBuffer>>();

	private byte[] _bytes;

	public int Length { get; private set; }

	public static ReadonlyBuffer CreateBuffer(byte[] bytes, int length)
	{
		int num;
		for (num = 1024; num < length; num *= 16)
		{
		}
		ReadonlyBuffer buffer = GetBuffer(num);
		Array.Copy(bytes, buffer._bytes, length);
		buffer.Length = length;
		return buffer;
	}

	public static void ReleaseBuffer(ReadonlyBuffer buffer)
	{
		if (buffer != null)
		{
			Queue<ReadonlyBuffer> queue = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dicBySize, buffer._bytes.Length, null);
			if (queue == null)
			{
				queue = new Queue<ReadonlyBuffer>();
				_dicBySize.Add(buffer._bytes.Length, queue);
			}
			queue.Enqueue(buffer);
		}
	}

	private static ReadonlyBuffer GetBuffer(int size)
	{
		Queue<ReadonlyBuffer> vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dicBySize, size, null);
		if (vALUE != null && vALUE.Count > 0)
		{
			return vALUE.Dequeue();
		}
		return new ReadonlyBuffer
		{
			_bytes = new byte[size]
		};
	}

	public byte[] GetRawBytes()
	{
		return _bytes;
	}

	void IDisposable.Dispose()
	{
		ReleaseBuffer(this);
	}
}
