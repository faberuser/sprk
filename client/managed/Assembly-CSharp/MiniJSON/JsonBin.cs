using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MiniJSON;

public static class JsonBin
{
	public class JsonBytesReader
	{
		public byte[] Bytes { get; private set; }

		public int Position { get; private set; }

		public int RemainedLength => ((Bytes != null) ? Bytes.Length : 0) - Position;

		public JsonBytesReader(byte[] bytes)
		{
			Bytes = bytes;
			Position = 0;
		}

		private bool CanRead(int byteCount)
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

	private sealed class Parser : IDisposable
	{
		private const string WHITE_SPACE = " \t\n\r";

		private const string WORD_BREAK = " \t\n\r{}[],:\"";

		private JsonBytesReader _reader;

		private List<string> _stringList;

		private Parser(byte[] bytes)
		{
			_reader = new JsonBytesReader(bytes);
			_stringList = new List<string>();
		}

		public void Dispose()
		{
			_reader = null;
			_stringList = null;
		}

		public static object Parse(byte[] bytes)
		{
			using Parser parser = new Parser(bytes);
			return parser.ParseRoot();
		}

		private object ParseRoot()
		{
			if (_reader.ReadInt() != 20160919)
			{
				return null;
			}
			ParseStringList();
			return ParseValue();
		}

		private void ParseStringList()
		{
			int num = _reader.ReadInt();
			if (_stringList.Capacity < _stringList.Count + num)
			{
				_stringList.Capacity = _stringList.Count + num;
			}
			for (int i = 0; i < num; i++)
			{
				string item = _reader.ReadString();
				_stringList.Add(item);
			}
		}

		private string ParseStringFromList()
		{
			int num = _reader.ReadInt() - 1;
			if (num < 0 || num >= _stringList.Count)
			{
				return string.Empty;
			}
			return _stringList[num];
		}

		private Dictionary<string, object> ParseValueObject()
		{
			int num = _reader.ReadInt();
			Dictionary<string, object> dictionary = JsonPool.instance.CreateDic(num);
			for (int i = 0; i < num; i++)
			{
				string text = ParseStringFromList();
				object value = ParseValue();
				if (!string.IsNullOrEmpty(text))
				{
					dictionary[text] = value;
				}
			}
			return dictionary;
		}

		private List<object> ParseValueArray()
		{
			int num = _reader.ReadInt();
			List<object> list = JsonPool.instance.CreateList(num);
			for (int i = 0; i < num; i++)
			{
				object item = ParseValue();
				list.Add(item);
			}
			return list;
		}

		private object ParseValue()
		{
			return _reader.ReadByte(byte.MaxValue) switch
			{
				0 => null, 
				10 => ParseValueObject(), 
				20 => ParseValueArray(), 
				30 => ParseValueString(), 
				40 => ParseValueBool(), 
				50 => ParseValueDouble(), 
				60 => ParseValueLong(), 
				61 => ParseValueInt(), 
				62 => ParseValueShort(), 
				_ => null, 
			};
		}

		private string ParseValueString()
		{
			return ParseStringFromList();
		}

		private long ParseValueLong()
		{
			return _reader.ReadLong();
		}

		private long ParseValueInt()
		{
			return _reader.ReadInt();
		}

		private long ParseValueShort()
		{
			return _reader.ReadShort();
		}

		private double ParseValueDouble()
		{
			return _reader.ReadDouble();
		}

		private bool ParseValueBool()
		{
			return _reader.ReadBool();
		}
	}

	private sealed class Serializer
	{
		private MemoryStream _bodyStream;

		private Dictionary<string, int> _stringMapper;

		private List<string> _stringList;

		private BitConverter2 _converter;

		private Serializer()
		{
			_bodyStream = new MemoryStream();
			_stringMapper = new Dictionary<string, int>();
			_stringList = new List<string>();
			_converter = new BitConverter2();
		}

		public static byte[] Serialize(object obj)
		{
			Serializer serializer = new Serializer();
			serializer.SerializeValue(obj);
			MemoryStream memoryStream = new MemoryStream();
			memoryStream.WRITEINT(20160919, serializer._converter);
			serializer.SerializeStringList(memoryStream);
			serializer._bodyStream.WriteTo(memoryStream);
			return memoryStream.ToArray();
		}

		private void SerializeStringList(MemoryStream stream)
		{
			stream.WRITEINT(_stringList.Count, _converter);
			for (int i = 0; i < _stringList.Count; i++)
			{
				stream.WRITESTRING(_stringList[i], _converter);
			}
		}

		private int GetStringIndex(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return 0;
			}
			if (!_stringMapper.TryGetValue(value, out var value2))
			{
				value2 = _stringList.Count + 1;
				_stringList.Add(value);
				_stringMapper.Add(value, value2);
			}
			return value2;
		}

		private void SerializeString(string value)
		{
			int stringIndex = GetStringIndex(value);
			_bodyStream.WRITEINT(stringIndex, _converter);
		}

		private void SerializeValue(object value)
		{
			if (value == null)
			{
				_bodyStream.WriteByte(0);
			}
			else if (value is string value2)
			{
				SerializeValueString(value2);
			}
			else if (value is bool)
			{
				SerializeValueBool((bool)value);
			}
			else if (value is IList anArray)
			{
				SerializeArray(anArray);
			}
			else if (value is IDictionary obj)
			{
				SerializeObject(obj);
			}
			else if (value is char)
			{
				SerializeValueString(value.ToString());
			}
			else
			{
				SerializeValueOther(value);
			}
		}

		private void SerializeObject(IDictionary obj)
		{
			_bodyStream.WriteByte(10);
			_bodyStream.WRITEINT(obj.Count, _converter);
			foreach (object key in obj.Keys)
			{
				SerializeString(key.ToString());
				SerializeValue(obj[key]);
			}
		}

		private void SerializeArray(IList anArray)
		{
			_bodyStream.WriteByte(20);
			_bodyStream.WRITEINT(anArray.Count, _converter);
			foreach (object item in anArray)
			{
				SerializeValue(item);
			}
		}

		private void SerializeValueString(string value)
		{
			_bodyStream.WriteByte(30);
			SerializeString(value);
		}

		private void SerializeValueBool(bool value)
		{
			_bodyStream.WriteByte(40);
			_bodyStream.WRITEBOOL(value, _converter);
		}

		private void SerializeValueLong(long value)
		{
			if (value < 32767 && value > -32768)
			{
				_bodyStream.WriteByte(62);
				_bodyStream.WRITESHORT((short)value, _converter);
			}
			else if (value < int.MaxValue && value > int.MinValue)
			{
				_bodyStream.WriteByte(61);
				_bodyStream.WRITEINT((int)value, _converter);
			}
			else
			{
				_bodyStream.WriteByte(60);
				_bodyStream.WRITELONG(value, _converter);
			}
		}

		private void SerializeValueDouble(double value)
		{
			_bodyStream.WriteByte(50);
			_bodyStream.WRITEDOUBLE(value, _converter);
		}

		private void SerializeValueOther(object value)
		{
			if (value is int)
			{
				SerializeValueLong((int)value);
			}
			else if (value is uint)
			{
				SerializeValueLong((uint)value);
			}
			else if (value is long)
			{
				SerializeValueLong((long)value);
			}
			else if (value is sbyte)
			{
				SerializeValueLong((sbyte)value);
			}
			else if (value is byte)
			{
				SerializeValueLong((byte)value);
			}
			else if (value is short)
			{
				SerializeValueLong((short)value);
			}
			else if (value is ushort)
			{
				SerializeValueLong((ushort)value);
			}
			else if (value is float)
			{
				SerializeValueDouble((float)value);
			}
			else if (value is double)
			{
				SerializeValueDouble((double)value);
			}
			else
			{
				SerializeValueString(value.ToString());
			}
		}
	}

	private const int Version = 20160919;

	private const byte JsonTypeNull = 0;

	private const byte JsonTypeObject = 10;

	private const byte JsonTypeArray = 20;

	private const byte JsonTypeString = 30;

	private const byte JsonTypeBool = 40;

	private const byte JsonTypeDouble = 50;

	private const byte JsonTypeLong = 60;

	private const byte JsonTypeInt = 61;

	private const byte JsonTypeShort = 62;

	private const byte JsonTypeError = byte.MaxValue;

	public static object Deserialize(byte[] bytes)
	{
		if (bytes == null || bytes.Length == 0)
		{
			return null;
		}
		return Parser.Parse(bytes);
	}

	public static byte[] Serialize(object obj)
	{
		return Serializer.Serialize(obj);
	}
}
