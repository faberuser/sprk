using System;
using System.Text;
using MessagePack;

namespace NShared.NJit.NCommon;

public struct ByteSpan
{
	public const int SIZE_INT = 4;

	private static readonly Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	private readonly byte[] _bytes;

	private readonly int _current;

	public bool IsValid => _bytes != null;

	public ByteSpan(byte[] bytes)
	{
		_bytes = bytes;
		_current = 0;
	}

	public ByteSpan(byte[] bytes, int position)
	{
		_bytes = bytes;
		_current = position;
	}

	public ByteSpan Slice(int index)
	{
		return new ByteSpan(_bytes, _current + index);
	}

	public ArraySegment<byte> SliceSegment(int index)
	{
		int num = _current + index;
		int count = _bytes.Length - num;
		return new ArraySegment<byte>(_bytes, _current + index, count);
	}

	public ReadOnlyMemory<byte> SliceReadOnlyMemory(int index)
	{
		int num = _current + index;
		int length = _bytes.Length - num;
		return new ReadOnlyMemory<byte>(_bytes, num, length);
	}

	public int ErrorRead(int offset, out ByteSpan nextSpan)
	{
		nextSpan = Slice(_current + offset + 4);
		return 0;
	}

	public T ReadByMsgPackT<T>(int offset, out ByteSpan nextSpan)
	{
		int bytesRead;
		T result = MessagePackSerializer.Deserialize<T>(new ReadOnlyMemory<byte>(_bytes, _current + offset, _bytes.Length - _current - offset), MessagePackInitializer.Options_with_BuiltinResolver, out bytesRead);
		nextSpan = Slice(offset + bytesRead);
		return result;
	}

	public char ReadByMsgPack_Char(int offset, out ByteSpan nextSpan)
	{
		return ReadByMsgPackT<char>(offset, out nextSpan);
	}

	public sbyte ReadByMsgPack_SByte(int offset, out ByteSpan nextSpan)
	{
		return ReadByMsgPackT<sbyte>(offset, out nextSpan);
	}

	public byte ReadByMsgPack_Byte(int offset, out ByteSpan nextSpan)
	{
		return ReadByMsgPackT<byte>(offset, out nextSpan);
	}

	public short ReadByMsgPack_Int16(int offset, out ByteSpan nextSpan)
	{
		return ReadByMsgPackT<short>(offset, out nextSpan);
	}

	public ushort ReadByMsgPack_UInt16(int offset, out ByteSpan nextSpan)
	{
		return ReadByMsgPackT<ushort>(offset, out nextSpan);
	}

	public int ReadByMsgPack_Int32(int offset, out ByteSpan nextSpan)
	{
		return ReadByMsgPackT<int>(offset, out nextSpan);
	}

	public uint ReadByMsgPack_UInt32(int offset, out ByteSpan nextSpan)
	{
		return ReadByMsgPackT<uint>(offset, out nextSpan);
	}

	public long ReadByMsgPack_Int64(int offset, out ByteSpan nextSpan)
	{
		return ReadByMsgPackT<long>(offset, out nextSpan);
	}

	public ulong ReadByMsgPack_UInt64(int offset, out ByteSpan nextSpan)
	{
		return ReadByMsgPackT<ulong>(offset, out nextSpan);
	}

	public string ReadByMsgPack_String(int offset, out int readSize)
	{
		return MessagePackSerializer.Deserialize<string>(new ReadOnlyMemory<byte>(_bytes, _current + offset, _bytes.Length - _current - offset), MessagePackInitializer.Options_with_BuiltinResolver, out readSize);
	}

	public byte ReadByte(int offset)
	{
		if (!TryRead(out byte value, offset))
		{
			return 0;
		}
		return value;
	}

	public int ReadInt(int offset)
	{
		if (!TryRead(out int value, offset))
		{
			return 0;
		}
		return value;
	}

	public bool TryRead(out byte value, int offset)
	{
		if (!EnoughSpace(1 + offset))
		{
			value = 0;
			return false;
		}
		value = _bytes[_current + offset];
		return true;
	}

	public bool TryRead(out sbyte value, int offset)
	{
		if (!EnoughSpace(1 + offset))
		{
			value = 0;
			return false;
		}
		value = (sbyte)_bytes[_current + offset];
		return true;
	}

	public bool TryRead(out ushort value, int offset)
	{
		if (!EnoughSpace(2 + offset))
		{
			value = 0;
			return false;
		}
		value = BitConverter.ToUInt16(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = BinaryPrimitives.ReverseEndianness(value);
		}
		return true;
	}

	public bool TryRead(out short value, int offset)
	{
		if (!EnoughSpace(2 + offset))
		{
			value = 0;
			return false;
		}
		value = BitConverter.ToInt16(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = BinaryPrimitives.ReverseEndianness(value);
		}
		return true;
	}

	public bool TryRead(out uint value, int offset)
	{
		if (!EnoughSpace(4 + offset))
		{
			value = 0u;
			return false;
		}
		value = BitConverter.ToUInt32(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = BinaryPrimitives.ReverseEndianness(value);
		}
		return true;
	}

	public bool TryRead(out int value, int offset)
	{
		if (!EnoughSpace(4 + offset))
		{
			value = 0;
			return false;
		}
		value = BitConverter.ToInt32(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = BinaryPrimitives.ReverseEndianness(value);
		}
		return true;
	}

	public bool TryRead(out ulong value, int offset)
	{
		if (!EnoughSpace(8 + offset))
		{
			value = 0uL;
			return false;
		}
		value = BitConverter.ToUInt64(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = BinaryPrimitives.ReverseEndianness(value);
		}
		return true;
	}

	public bool TryRead(out long value, int offset)
	{
		if (!EnoughSpace(8 + offset))
		{
			value = 0L;
			return false;
		}
		value = BitConverter.ToInt64(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = BinaryPrimitives.ReverseEndianness(value);
		}
		return true;
	}

	public unsafe bool TryRead(out float value, int offset)
	{
		if (TryRead(out int value2, offset))
		{
			value = *(float*)(&value2);
			return true;
		}
		value = 0f;
		return false;
	}

	public unsafe bool TryRead(out double value, int offset)
	{
		if (TryRead(out long value2, offset))
		{
			value = *(double*)(&value2);
			return true;
		}
		value = 0.0;
		return false;
	}

	private bool EnoughSpace(int size)
	{
		if (_bytes == null)
		{
			return false;
		}
		return _current + size <= _bytes.Length;
	}
}
