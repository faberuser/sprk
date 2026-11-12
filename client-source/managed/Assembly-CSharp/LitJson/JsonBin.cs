using System;
using System.Collections.Generic;
using System.IO;
using NVespa;
using NVespa.NUtil;
using Vespa_System_MemoryStream_ExtensionMethod;

namespace LitJson;

public static class JsonBin
{
	private sealed class Parser : IDisposable
	{
		private static object locker = new object();

		private NVespa.NUtil.IReader _reader;

		private List<string> _stringList;

		private Parser(NVespa.NUtil.IReader reader)
		{
			_reader = reader;
			_stringList = new List<string>();
		}

		public void Dispose()
		{
			_reader = null;
			_stringList = null;
		}

		public static JsonData Parse(NVespa.NUtil.IReader stream)
		{
			using Parser parser = new Parser(stream);
			return parser.ParseRoot();
		}

		private JsonData ParseRoot()
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

		private string ParseString()
		{
			int num = _reader.ReadInt() - 1;
			if (num < 0 || num >= _stringList.Count)
			{
				return string.Empty;
			}
			return _stringList[num];
		}

		private JsonData ParseValueObject()
		{
			JsonData jsonData = JsonData.CreateJsonData();
			int num = _reader.ReadInt();
			for (int i = 0; i < num; i++)
			{
				string text = ParseString();
				JsonData value = ParseValue();
				if (!string.IsNullOrEmpty(text))
				{
					jsonData.AddField(text, value);
				}
			}
			return jsonData;
		}

		private JsonData ParseValueArray()
		{
			JsonData jsonData = JsonData.CreateJsonData();
			int num = _reader.ReadInt();
			jsonData.ReserveArrayCapacity(num);
			for (int i = 0; i < num; i++)
			{
				JsonData data = ParseValue();
				jsonData.Add(data);
			}
			return jsonData;
		}

		private JsonData ParseValue()
		{
			return _reader.ReadByte(153) switch
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

		private JsonData ParseValueString()
		{
			return JsonData.CreateJsonString(ParseString());
		}

		private JsonData ParseValueLong()
		{
			return JsonData.CreateJsonLong(_reader.ReadLong());
		}

		private JsonData ParseValueInt()
		{
			return JsonData.CreateJsonLong(_reader.ReadInt());
		}

		private JsonData ParseValueShort()
		{
			return JsonData.CreateJsonLong(_reader.ReadShort());
		}

		private JsonData ParseValueDouble()
		{
			return JsonData.CreateJsonDouble(_reader.ReadDouble());
		}

		private JsonData ParseValueBool()
		{
			return JsonData.CreateJsonBool(_reader.ReadBool());
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

		public static byte[] Serialize(JsonData json)
		{
			Serializer serializer = new Serializer();
			serializer.SerializeValue(json);
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

		private void SerializeValue(JsonData json)
		{
			if (json.isNone())
			{
				_bodyStream.WriteByte(0);
			}
			else if (json.IsString())
			{
				SerializeValueString(json);
			}
			else if (json.IsBoolean())
			{
				SerializeValueBool(json);
			}
			else if (json.IsArray())
			{
				SerializeArray(json);
			}
			else if (json.IsObject())
			{
				SerializeObject(json);
			}
			else
			{
				SerializeValueOther(json);
			}
		}

		private void SerializeObject(JsonData json)
		{
			if (!json.IsObject())
			{
				return;
			}
			_bodyStream.WriteByte(10);
			_bodyStream.WRITEINT(json.Count, _converter);
			foreach (KeyValuePair<string, JsonData> item in json.ToDictionary())
			{
				string key = item.Key;
				JsonData value = item.Value;
				SerializeString(key);
				SerializeValue(value);
			}
		}

		private void SerializeArray(JsonData json)
		{
			if (json.IsArray())
			{
				_bodyStream.WriteByte(20);
				_bodyStream.WRITEINT(json.Count, _converter);
				for (int i = 0; i < json.Count; i++)
				{
					SerializeValue(json.GetFromArray(i));
				}
			}
		}

		private void SerializeValueString(JsonData json)
		{
			_bodyStream.WriteByte(30);
			SerializeString(json.valueStr);
		}

		private void SerializeValueBool(JsonData json)
		{
			_bodyStream.WriteByte(40);
			_bodyStream.WRITEBOOL(json.valueBool, _converter);
		}

		private void SerializeValueLong(JsonData json)
		{
			long valueLong = json.valueLong;
			if (valueLong < 32767 && valueLong > -32768)
			{
				_bodyStream.WriteByte(62);
				_bodyStream.WRITESHORT((short)valueLong, _converter);
			}
			else if (valueLong < int.MaxValue && valueLong > int.MinValue)
			{
				_bodyStream.WriteByte(61);
				_bodyStream.WRITEINT((int)valueLong, _converter);
			}
			else
			{
				_bodyStream.WriteByte(60);
				_bodyStream.WRITELONG(valueLong, _converter);
			}
		}

		private void SerializeValueDouble(JsonData json)
		{
			_bodyStream.WriteByte(50);
			_bodyStream.WRITEDOUBLE(json.valueDouble, _converter);
		}

		private void SerializeValueOther(JsonData json)
		{
			if (json.IsLong())
			{
				SerializeValueLong(json);
			}
			else if (json.IsDouble())
			{
				SerializeValueDouble(json);
			}
			else
			{
				SerializeValueString(json);
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

	private const byte JsonTypeError = 153;

	public static JsonData Deserialize(byte[] bytes)
	{
		if (bytes == null || bytes.Length == 0)
		{
			return null;
		}
		return Parser.Parse(new BytesReader(bytes));
	}

	public static JsonData Deserialize(StreamBinReader reader)
	{
		if (reader == null)
		{
			return null;
		}
		return Parser.Parse(reader);
	}

	public static JsonData Deserialize(BytesReader reader)
	{
		return Parser.Parse(reader);
	}

	public static byte[] Serialize(JsonData json)
	{
		return Serializer.Serialize(json);
	}
}
