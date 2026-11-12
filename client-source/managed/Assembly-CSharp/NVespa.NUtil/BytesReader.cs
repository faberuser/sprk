using System;
using System.Text;

namespace NVespa.NUtil;

public class BytesReader : NVespa.NUtil.IReader
{
	private int _position;

	private Action<string> _notify;

	public byte[] Bytes { get; private set; }

	public int Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public int RemainedLength => __Extension_Method_ICollection__.COUNT(Bytes) - Position;

	public BytesReader(byte[] bytes, int position = 0, Action<string> notify = null)
	{
		Bytes = bytes;
		_notify = notify;
		Position = position;
	}

	public bool CanRead(int byteCount)
	{
		if (Bytes == null)
		{
			return false;
		}
		if (Bytes.Length - Position < byteCount)
		{
			return false;
		}
		return true;
	}

	public string ReadString()
	{
		int num = ReadInt();
		if (num == 0)
		{
			return string.Empty;
		}
		if (!CanRead(num))
		{
			return string.Empty;
		}
		string result = Encoding.Unicode.GetString(Bytes, Position, num);
		Position += num;
		return result;
	}

	public string ReadUtf8String()
	{
		int num = ReadInt();
		if (num == 0)
		{
			return string.Empty;
		}
		if (!CanRead(num))
		{
			return string.Empty;
		}
		string result = Encoding.UTF8.GetString(Bytes, Position, num);
		Position += num;
		return result;
	}

	public bool ReadBool()
	{
		if (!CanRead(1))
		{
			return false;
		}
		bool result = BitConverter.ToBoolean(Bytes, Position);
		Position++;
		return result;
	}

	public byte ReadByte(byte defaultValue = 0)
	{
		if (!CanRead(1))
		{
			return defaultValue;
		}
		byte result = Bytes[Position];
		Position++;
		return result;
	}

	public short ReadShort()
	{
		if (!CanRead(2))
		{
			return 0;
		}
		short result = BitConverter.ToInt16(Bytes, Position);
		Position += 2;
		return result;
	}

	public int ReadInt()
	{
		if (!CanRead(4))
		{
			return 0;
		}
		int result = BitConverter.ToInt32(Bytes, Position);
		Position += 4;
		return result;
	}

	public long ReadLong()
	{
		if (!CanRead(8))
		{
			return 0L;
		}
		long result = BitConverter.ToInt64(Bytes, Position);
		Position += 8;
		return result;
	}

	public double ReadDouble()
	{
		if (!CanRead(8))
		{
			return 0.0;
		}
		double result = BitConverter.ToDouble(Bytes, Position);
		Position += 8;
		return result;
	}
}
