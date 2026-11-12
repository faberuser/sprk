using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using MessagePack;

namespace NShared.NJit.NCommon;

public class SimpleBufferWriter : IBufferWriter<byte>, IDisposable
{
	private static class Pool
	{
		private static Queue<SimpleBufferWriter> _queue = new Queue<SimpleBufferWriter>();

		public static SimpleBufferWriter Create()
		{
			if (_queue.Count > 0)
			{
				return _queue.Dequeue();
			}
			return new SimpleBufferWriter();
		}

		public static void Release(SimpleBufferWriter writer)
		{
			_queue.Enqueue(writer);
		}
	}

	private byte[] _buffer;

	private int _position;

	public static SimpleBufferWriter CreateWriter()
	{
		return Pool.Create();
	}

	public static byte[] ErrorWrite(int value)
	{
		byte[] array = new byte[4];
		WriteToBuffer((uint)value, array, 0);
		return array;
	}

	public static byte[] DirectWriteByMsgPack(int value)
	{
		return MessagePackSerializer.Serialize(value, MessagePackInitializer.Options_with_BuiltinResolver);
	}

	private SimpleBufferWriter()
	{
		_buffer = new byte[1048576];
		_position = 0;
	}

	public void Dispose()
	{
		_position = 0;
		Pool.Release(this);
	}

	public byte[] ToArray()
	{
		byte[] array = new byte[_position];
		Array.Copy(_buffer, array, _position);
		return array;
	}

	public byte[] ComputeHash(MD5 md5)
	{
		return md5.ComputeHash(_buffer, 0, _position);
	}

	public void Advance(int count)
	{
		Ensure(count);
		_position += count;
	}

	public void WriteByMsgPack<T>(T value)
	{
		MessagePackWriter writer = new MessagePackWriter(this);
		MessagePackSerializer.Serialize(ref writer, value, MessagePackInitializer.Options_with_BuiltinResolver);
		writer.Flush();
	}

	public void WriteCharByMsgPack(char value)
	{
		WriteByMsgPack(value);
	}

	public void WriteByteByMsgPack(byte value)
	{
		WriteByMsgPack(value);
	}

	public void WriteSByteByMsgPack(sbyte value)
	{
		WriteByMsgPack(value);
	}

	public void WriteInt16ByMsgPack(short value)
	{
		WriteByMsgPack(value);
	}

	public void WriteUInt16ByMsgPack(ushort value)
	{
		WriteByMsgPack(value);
	}

	public void WriteInt32ByMsgPack(int value)
	{
		WriteByMsgPack(value);
	}

	public void WriteUInt32ByMsgPack(uint value)
	{
		WriteByMsgPack(value);
	}

	public void WriteInt64ByMsgPack(long value)
	{
		WriteByMsgPack(value);
	}

	public void WriteUInt64ByMsgPack(ulong value)
	{
		WriteByMsgPack(value);
	}

	public void WriteStringByMsgPack(string value)
	{
		WriteByMsgPack(value);
	}

	public void WriteSByte(sbyte value)
	{
		WriteByte((byte)value);
	}

	public void WriteByte(byte value)
	{
		Ensure(1);
		_buffer[_position] = value;
		Advance(1);
	}

	public void WriteShort(short value)
	{
		WriteUShort((ushort)value);
	}

	public void WriteUShort(ushort value)
	{
		Ensure(2);
		_buffer[_position] = (byte)value;
		_buffer[_position + 1] = (byte)(value >> 8);
		Advance(2);
	}

	public void WriteInt(int value)
	{
		WriteUInt((uint)value);
	}

	public void WriteUInt(uint value)
	{
		Ensure(4);
		WriteToBuffer(value, _buffer, _position);
		Advance(4);
	}

	private static void WriteToBuffer(uint value, byte[] buffer, int position)
	{
		buffer[position] = (byte)value;
		buffer[position + 1] = (byte)(value >> 8);
		buffer[position + 2] = (byte)(value >> 16);
		buffer[position + 3] = (byte)(value >> 24);
	}

	public void WriteBytes(byte[] bytes)
	{
		if (bytes != null && bytes.Length != 0)
		{
			Ensure(bytes.Length);
			Array.Copy(bytes, 0, _buffer, _position, bytes.Length);
			Advance(bytes.Length);
		}
	}

	public void WriteFromStream(Stream stream)
	{
		int count = (int)(stream.Length - stream.Position);
		Ensure(count);
		int count2 = stream.Read(_buffer, _position, count);
		Advance(count2);
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
			_buffer = new byte[num];
			Array.Copy(buffer, _buffer, buffer.Length);
		}
	}

	Memory<byte> IBufferWriter<byte>.GetMemory(int sizeHint)
	{
		Ensure(sizeHint);
		return _buffer.AsMemory(_position);
	}

	Span<byte> IBufferWriter<byte>.GetSpan(int sizeHint)
	{
		Ensure(sizeHint);
		return _buffer.AsSpan(_position);
	}
}
