using System.IO;
using System.Text;

namespace NVespa.NUtil;

public class StreamBinReader : NVespa.NUtil.IReader
{
	private BinaryReader reader;

	private int _offset;

	public FileStream stream { get; private set; }

	public int Offset
	{
		get
		{
			return _offset;
		}
		set
		{
			_offset = value;
		}
	}

	public StreamBinReader(string path, int offset = 0)
	{
		stream = new FileStream(path, FileMode.Open, FileAccess.Read);
		reader = new BinaryReader(stream);
		Offset = offset;
	}

	public void SetOffset(int offset)
	{
		Offset = offset;
		stream.Position = offset;
	}

	public bool CanRead(int byteCount)
	{
		if (stream == null)
		{
			return false;
		}
		if (stream.Length - Offset < byteCount)
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
		byte[] array = new byte[num];
		reader.Read(array, 0, num);
		string result = Encoding.Unicode.GetString(array);
		Offset += num;
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
		byte[] array = new byte[num];
		reader.Read(array, Offset, num);
		string result = Encoding.UTF8.GetString(array);
		Offset += num;
		return result;
	}

	public bool ReadBool()
	{
		if (!CanRead(1))
		{
			return false;
		}
		bool result = reader.ReadBoolean();
		Offset++;
		return result;
	}

	public byte ReadByte(byte defaultValue = 0)
	{
		if (!CanRead(1))
		{
			return defaultValue;
		}
		byte result = reader.ReadByte();
		Offset++;
		return result;
	}

	public short ReadShort()
	{
		if (!CanRead(2))
		{
			return 0;
		}
		short result = reader.ReadInt16();
		Offset += 2;
		return result;
	}

	public int ReadInt()
	{
		if (!CanRead(4))
		{
			return 0;
		}
		int result = reader.ReadInt32();
		Offset += 4;
		return result;
	}

	public long ReadLong()
	{
		if (!CanRead(8))
		{
			return 0L;
		}
		long result = reader.ReadInt64();
		Offset += 8;
		return result;
	}

	public double ReadDouble()
	{
		if (!CanRead(8))
		{
			return 0.0;
		}
		double result = reader.ReadDouble();
		Offset += 8;
		return result;
	}

	public void Close()
	{
		stream.Dispose();
		reader.Close();
		_offset = 0;
	}
}
