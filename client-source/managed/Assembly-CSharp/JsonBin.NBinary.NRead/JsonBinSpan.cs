using System;
using System.Text;

namespace JsonBin.NBinary.NRead;

public struct JsonBinSpan
{
	private static readonly Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	private readonly byte[] _bytes;

	private readonly int _current;

	public byte this[int offset]
	{
		get
		{
			return _bytes[_current + offset];
		}
		set
		{
			_bytes[_current + offset] = value;
		}
	}

	public bool IsValid => _bytes != null;

	public JsonType NextJsonType
	{
		get
		{
			if (TryNextCode(out var code))
			{
				return JsonBinCode.ToJsonType(code);
			}
			return JsonType.None;
		}
	}

	public JsonBinSpan(byte[] bytes)
	{
		_bytes = bytes;
		_current = 0;
	}

	public JsonBinSpan(byte[] bytes, int position)
	{
		_bytes = bytes;
		_current = position;
	}

	public void CopyFrom(byte[] bytes)
	{
		Array.Copy(bytes, 0, _bytes, _current, bytes.Length);
	}

	public void CopyFrom(byte[] bytes, int length)
	{
		Array.Copy(bytes, 0, _bytes, _current, length);
	}

	public bool TryNextJsonType(out JsonType type)
	{
		if (TryNextCode(out var code))
		{
			type = JsonBinCode.ToJsonType(code);
			return true;
		}
		type = JsonType.None;
		return false;
	}

	private bool TryNextCode(out byte code)
	{
		if (!EnoughSpace(1))
		{
			code = 192;
			return false;
		}
		code = _bytes[_current];
		return true;
	}

	public JsonBinSpan Slice(int index)
	{
		return new JsonBinSpan(_bytes, _current + index);
	}

	private JsonBinReader CreateReader()
	{
		return new JsonBinReader(_bytes, _current);
	}

	public bool TryReadPacked_Nil()
	{
		if (!TryRead_Native(out byte value, 0))
		{
			return false;
		}
		if (value != 192)
		{
			return false;
		}
		return true;
	}

	public bool TryReadPacked_Boolean(out bool value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = false;
			return false;
		}
		switch (value2)
		{
		case 195:
			value = true;
			return true;
		case 194:
			value = false;
			return true;
		default:
			value = false;
			return false;
		}
	}

	public bool TryReadPacked_IndexedStr(out int stringIndex)
	{
		if (!TryRead_Native(out byte value, 0))
		{
			stringIndex = 0;
			return false;
		}
		if (value != 221)
		{
			stringIndex = 0;
			return false;
		}
		return TryRead_Native(out stringIndex, 1);
	}

	public bool TryReadPacked_String(out string value)
	{
		return CreateReader().TryReadPacked_String(out value);
	}

	public bool TryReadPacked_String(out string value, out JsonBinSpan nextSpan)
	{
		JsonBinReader jsonBinReader = CreateReader();
		if (!jsonBinReader.TryReadPacked_String(out value))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = jsonBinReader.Span(0);
		return true;
	}

	public bool TryReadPacked_Byte(out byte value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0;
			return false;
		}
		checked
		{
			switch (value2)
			{
			case 201:
			{
				byte value4;
				bool result2 = TryRead_Native(out value4, 1);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3, 1);
				value = (byte)value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5, 1);
				value = (byte)value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10, 1);
				value = (byte)value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9, 1);
				value = (byte)value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8, 1);
				value = (byte)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7, 1);
				value = (byte)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6, 1);
				value = (byte)value6;
				return result4;
			}
			case 224:
			case 225:
			case 226:
			case 227:
			case 228:
			case 229:
			case 230:
			case 231:
			case 232:
			case 233:
			case 234:
			case 235:
			case 236:
			case 237:
			case 238:
			case 239:
			case 240:
			case 241:
			case 242:
			case 243:
			case 244:
			case 245:
			case 246:
			case 247:
			case 248:
			case 249:
			case 250:
			case 251:
			case 252:
			case 253:
			case 254:
			case byte.MaxValue:
				value = (byte)unchecked((sbyte)value2);
				return true;
			default:
				if (value2 >= 0 && value2 <= 127)
				{
					value = value2;
					return true;
				}
				value = 0;
				return false;
			}
		}
	}

	public bool TryReadPacked_SByte(out sbyte value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0;
			return false;
		}
		checked
		{
			switch (value2)
			{
			case 201:
			{
				byte value4;
				bool result2 = TryRead_Native(out value4, 1);
				value = (sbyte)value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3, 1);
				value = value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5, 1);
				value = (sbyte)value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10, 1);
				value = (sbyte)value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9, 1);
				value = (sbyte)value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8, 1);
				value = (sbyte)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7, 1);
				value = (sbyte)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6, 1);
				value = (sbyte)value6;
				return result4;
			}
			case 224:
			case 225:
			case 226:
			case 227:
			case 228:
			case 229:
			case 230:
			case 231:
			case 232:
			case 233:
			case 234:
			case 235:
			case 236:
			case 237:
			case 238:
			case 239:
			case 240:
			case 241:
			case 242:
			case 243:
			case 244:
			case 245:
			case 246:
			case 247:
			case 248:
			case 249:
			case 250:
			case 251:
			case 252:
			case 253:
			case 254:
			case byte.MaxValue:
				value = unchecked((sbyte)value2);
				return true;
			default:
				if (value2 >= 0 && value2 <= 127)
				{
					value = unchecked((sbyte)value2);
					return true;
				}
				value = 0;
				return false;
			}
		}
	}

	public bool TryReadPacked_UShort(out ushort value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0;
			return false;
		}
		checked
		{
			switch (value2)
			{
			case 201:
			{
				byte value4;
				bool result2 = TryRead_Native(out value4, 1);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3, 1);
				value = (ushort)value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5, 1);
				value = value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10, 1);
				value = (ushort)value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9, 1);
				value = (ushort)value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8, 1);
				value = (ushort)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7, 1);
				value = (ushort)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6, 1);
				value = (ushort)value6;
				return result4;
			}
			case 224:
			case 225:
			case 226:
			case 227:
			case 228:
			case 229:
			case 230:
			case 231:
			case 232:
			case 233:
			case 234:
			case 235:
			case 236:
			case 237:
			case 238:
			case 239:
			case 240:
			case 241:
			case 242:
			case 243:
			case 244:
			case 245:
			case 246:
			case 247:
			case 248:
			case 249:
			case 250:
			case 251:
			case 252:
			case 253:
			case 254:
			case byte.MaxValue:
				value = (ushort)unchecked((sbyte)value2);
				return true;
			default:
				if (value2 >= 0 && value2 <= 127)
				{
					value = value2;
					return true;
				}
				value = 0;
				return false;
			}
		}
	}

	public bool TryReadPacked_Short(out short value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0;
			return false;
		}
		checked
		{
			switch (value2)
			{
			case 201:
			{
				byte value4;
				bool result2 = TryRead_Native(out value4, 1);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3, 1);
				value = value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5, 1);
				value = (short)value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10, 1);
				value = value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9, 1);
				value = (short)value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8, 1);
				value = (short)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7, 1);
				value = (short)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6, 1);
				value = (short)value6;
				return result4;
			}
			case 224:
			case 225:
			case 226:
			case 227:
			case 228:
			case 229:
			case 230:
			case 231:
			case 232:
			case 233:
			case 234:
			case 235:
			case 236:
			case 237:
			case 238:
			case 239:
			case 240:
			case 241:
			case 242:
			case 243:
			case 244:
			case 245:
			case 246:
			case 247:
			case 248:
			case 249:
			case 250:
			case 251:
			case 252:
			case 253:
			case 254:
			case byte.MaxValue:
				value = unchecked((sbyte)value2);
				return true;
			default:
				if (value2 >= 0 && value2 <= 127)
				{
					value = value2;
					return true;
				}
				value = 0;
				return false;
			}
		}
	}

	public bool TryReadPacked_UInt(out uint value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0u;
			return false;
		}
		checked
		{
			switch (value2)
			{
			case 201:
			{
				byte value4;
				bool result2 = TryRead_Native(out value4, 1);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3, 1);
				value = (uint)value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5, 1);
				value = value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10, 1);
				value = (uint)value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9, 1);
				value = value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8, 1);
				value = (uint)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7, 1);
				value = (uint)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6, 1);
				value = (uint)value6;
				return result4;
			}
			case 224:
			case 225:
			case 226:
			case 227:
			case 228:
			case 229:
			case 230:
			case 231:
			case 232:
			case 233:
			case 234:
			case 235:
			case 236:
			case 237:
			case 238:
			case 239:
			case 240:
			case 241:
			case 242:
			case 243:
			case 244:
			case 245:
			case 246:
			case 247:
			case 248:
			case 249:
			case 250:
			case 251:
			case 252:
			case 253:
			case 254:
			case byte.MaxValue:
				value = (uint)unchecked((sbyte)value2);
				return true;
			default:
				if (value2 >= 0 && value2 <= 127)
				{
					value = value2;
					return true;
				}
				value = 0u;
				return false;
			}
		}
	}

	public bool TryReadPacked_Int(out int value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0;
			return false;
		}
		checked
		{
			switch (value2)
			{
			case 201:
			{
				byte value4;
				bool result2 = TryRead_Native(out value4, 1);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3, 1);
				value = value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5, 1);
				value = value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10, 1);
				value = value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9, 1);
				value = (int)value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8, 1);
				value = value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7, 1);
				value = (int)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6, 1);
				value = (int)value6;
				return result4;
			}
			case 224:
			case 225:
			case 226:
			case 227:
			case 228:
			case 229:
			case 230:
			case 231:
			case 232:
			case 233:
			case 234:
			case 235:
			case 236:
			case 237:
			case 238:
			case 239:
			case 240:
			case 241:
			case 242:
			case 243:
			case 244:
			case 245:
			case 246:
			case 247:
			case 248:
			case 249:
			case 250:
			case 251:
			case 252:
			case 253:
			case 254:
			case byte.MaxValue:
				value = unchecked((sbyte)value2);
				return true;
			default:
				if (value2 >= 0 && value2 <= 127)
				{
					value = value2;
					return true;
				}
				value = 0;
				return false;
			}
		}
	}

	public bool TryReadPacked_Int(out int value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0;
			nextSpan = this;
			return false;
		}
		checked
		{
			switch (value2)
			{
			case 201:
			{
				byte value4;
				bool result2 = TryRead_Native(out value4, 1);
				value = value4;
				nextSpan = Slice(2);
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3, 1);
				value = value3;
				nextSpan = Slice(2);
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5, 1);
				value = value5;
				nextSpan = Slice(3);
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10, 1);
				value = value10;
				nextSpan = Slice(3);
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9, 1);
				value = (int)value9;
				nextSpan = Slice(5);
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8, 1);
				value = value8;
				nextSpan = Slice(5);
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7, 1);
				value = (int)value7;
				nextSpan = Slice(9);
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6, 1);
				value = (int)value6;
				nextSpan = Slice(9);
				return result4;
			}
			case 224:
			case 225:
			case 226:
			case 227:
			case 228:
			case 229:
			case 230:
			case 231:
			case 232:
			case 233:
			case 234:
			case 235:
			case 236:
			case 237:
			case 238:
			case 239:
			case 240:
			case 241:
			case 242:
			case 243:
			case 244:
			case 245:
			case 246:
			case 247:
			case 248:
			case 249:
			case 250:
			case 251:
			case 252:
			case 253:
			case 254:
			case byte.MaxValue:
				value = unchecked((sbyte)value2);
				nextSpan = Slice(1);
				return true;
			default:
				if (value2 >= 0 && value2 <= 127)
				{
					value = value2;
					nextSpan = Slice(1);
					return true;
				}
				value = 0;
				nextSpan = this;
				return false;
			}
		}
	}

	public bool TryReadPacked_ULong(out ulong value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0uL;
			return false;
		}
		checked
		{
			switch (value2)
			{
			case 201:
			{
				byte value4;
				bool result2 = TryRead_Native(out value4, 1);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3, 1);
				value = (ulong)value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5, 1);
				value = value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10, 1);
				value = (ulong)value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9, 1);
				value = value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8, 1);
				value = (ulong)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7, 1);
				value = value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6, 1);
				value = (ulong)value6;
				return result4;
			}
			case 224:
			case 225:
			case 226:
			case 227:
			case 228:
			case 229:
			case 230:
			case 231:
			case 232:
			case 233:
			case 234:
			case 235:
			case 236:
			case 237:
			case 238:
			case 239:
			case 240:
			case 241:
			case 242:
			case 243:
			case 244:
			case 245:
			case 246:
			case 247:
			case 248:
			case 249:
			case 250:
			case 251:
			case 252:
			case 253:
			case 254:
			case byte.MaxValue:
				value = (ulong)unchecked((sbyte)value2);
				return true;
			default:
				if (value2 >= 0 && value2 <= 127)
				{
					value = value2;
					return true;
				}
				value = 0uL;
				return false;
			}
		}
	}

	public bool TryReadPacked_Long(out long value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0L;
			return false;
		}
		switch (value2)
		{
		case 201:
		{
			byte value4;
			bool result2 = TryRead_Native(out value4, 1);
			value = value4;
			return result2;
		}
		case 208:
		{
			sbyte value3;
			bool result = TryRead_Native(out value3, 1);
			value = value3;
			return result;
		}
		case 202:
		{
			ushort value5;
			bool result3 = TryRead_Native(out value5, 1);
			value = value5;
			return result3;
		}
		case 209:
		{
			short value10;
			bool result8 = TryRead_Native(out value10, 1);
			value = value10;
			return result8;
		}
		case 203:
		{
			uint value9;
			bool result7 = TryRead_Native(out value9, 1);
			value = value9;
			return result7;
		}
		case 210:
		{
			int value8;
			bool result6 = TryRead_Native(out value8, 1);
			value = value8;
			return result6;
		}
		case 204:
		{
			ulong value7;
			bool result5 = TryRead_Native(out value7, 1);
			value = checked((long)value7);
			return result5;
		}
		case 211:
		{
			long value6;
			bool result4 = TryRead_Native(out value6, 1);
			value = value6;
			return result4;
		}
		case 224:
		case 225:
		case 226:
		case 227:
		case 228:
		case 229:
		case 230:
		case 231:
		case 232:
		case 233:
		case 234:
		case 235:
		case 236:
		case 237:
		case 238:
		case 239:
		case 240:
		case 241:
		case 242:
		case 243:
		case 244:
		case 245:
		case 246:
		case 247:
		case 248:
		case 249:
		case 250:
		case 251:
		case 252:
		case 253:
		case 254:
		case byte.MaxValue:
			value = (sbyte)value2;
			return true;
		default:
			if (value2 >= 0 && value2 <= 127)
			{
				value = value2;
				return true;
			}
			value = 0L;
			return false;
		}
	}

	public bool TryReadPacked_Float(out float value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0f;
			return false;
		}
		switch (value2)
		{
		case 199:
		{
			float value4;
			bool result2 = TryRead_Native(out value4, 1);
			value = value4;
			return result2;
		}
		case 200:
		{
			double value3;
			bool result = TryRead_Native(out value3, 1);
			value = (float)value3;
			return result;
		}
		case 208:
		{
			sbyte value5;
			bool result3 = TryRead_Native(out value5, 1);
			value = value5;
			return result3;
		}
		case 209:
		{
			short value12;
			bool result10 = TryRead_Native(out value12, 1);
			value = value12;
			return result10;
		}
		case 210:
		{
			int value11;
			bool result9 = TryRead_Native(out value11, 1);
			value = value11;
			return result9;
		}
		case 211:
		{
			long value10;
			bool result8 = TryRead_Native(out value10, 1);
			value = value10;
			return result8;
		}
		case 201:
		{
			byte value9;
			bool result7 = TryRead_Native(out value9, 1);
			value = (int)value9;
			return result7;
		}
		case 202:
		{
			ushort value8;
			bool result6 = TryRead_Native(out value8, 1);
			value = (int)value8;
			return result6;
		}
		case 203:
		{
			uint value7;
			bool result5 = TryRead_Native(out value7, 1);
			value = value7;
			return result5;
		}
		case 204:
		{
			ulong value6;
			bool result4 = TryRead_Native(out value6, 1);
			value = value6;
			return result4;
		}
		case 224:
		case 225:
		case 226:
		case 227:
		case 228:
		case 229:
		case 230:
		case 231:
		case 232:
		case 233:
		case 234:
		case 235:
		case 236:
		case 237:
		case 238:
		case 239:
		case 240:
		case 241:
		case 242:
		case 243:
		case 244:
		case 245:
		case 246:
		case 247:
		case 248:
		case 249:
		case 250:
		case 251:
		case 252:
		case 253:
		case 254:
		case byte.MaxValue:
			value = (sbyte)value2;
			return true;
		default:
			if (value2 >= 0 && value2 <= 127)
			{
				value = (int)value2;
				return true;
			}
			value = 0f;
			return false;
		}
	}

	public bool TryReadPacked_Double(out double value)
	{
		if (!TryRead_Native(out byte value2, 0))
		{
			value = 0.0;
			return false;
		}
		switch (value2)
		{
		case 199:
		{
			float value4;
			bool result2 = TryRead_Native(out value4, 1);
			value = value4;
			return result2;
		}
		case 200:
		{
			double value3;
			bool result = TryRead_Native(out value3, 1);
			value = value3;
			return result;
		}
		case 208:
		{
			sbyte value5;
			bool result3 = TryRead_Native(out value5, 1);
			value = value5;
			return result3;
		}
		case 209:
		{
			short value12;
			bool result10 = TryRead_Native(out value12, 1);
			value = value12;
			return result10;
		}
		case 210:
		{
			int value11;
			bool result9 = TryRead_Native(out value11, 1);
			value = value11;
			return result9;
		}
		case 211:
		{
			long value10;
			bool result8 = TryRead_Native(out value10, 1);
			value = value10;
			return result8;
		}
		case 201:
		{
			byte value9;
			bool result7 = TryRead_Native(out value9, 1);
			value = (int)value9;
			return result7;
		}
		case 202:
		{
			ushort value8;
			bool result6 = TryRead_Native(out value8, 1);
			value = (int)value8;
			return result6;
		}
		case 203:
		{
			uint value7;
			bool result5 = TryRead_Native(out value7, 1);
			value = value7;
			return result5;
		}
		case 204:
		{
			ulong value6;
			bool result4 = TryRead_Native(out value6, 1);
			value = value6;
			return result4;
		}
		case 224:
		case 225:
		case 226:
		case 227:
		case 228:
		case 229:
		case 230:
		case 231:
		case 232:
		case 233:
		case 234:
		case 235:
		case 236:
		case 237:
		case 238:
		case 239:
		case 240:
		case 241:
		case 242:
		case 243:
		case 244:
		case 245:
		case 246:
		case 247:
		case 248:
		case 249:
		case 250:
		case 251:
		case 252:
		case 253:
		case 254:
		case byte.MaxValue:
			value = (sbyte)value2;
			return true;
		default:
			if (value2 >= 0 && value2 <= 127)
			{
				value = (int)value2;
				return true;
			}
			value = 0.0;
			return false;
		}
	}

	public bool TryRead_Native(out byte value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out value, 0))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = Slice(1);
		return true;
	}

	public bool TryRead_Native(out byte value, int offset)
	{
		if (!EnoughSpace(1 + offset))
		{
			value = 0;
			return false;
		}
		value = _bytes[_current + offset];
		return true;
	}

	public bool TryRead_Native(out sbyte value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out value, 0))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = Slice(1);
		return true;
	}

	public bool TryRead_Native(out sbyte value, int offset)
	{
		if (!EnoughSpace(1 + offset))
		{
			value = 0;
			return false;
		}
		value = (sbyte)_bytes[_current + offset];
		return true;
	}

	public bool TryRead_Native(out ushort value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out value, 0))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = Slice(2);
		return true;
	}

	public bool TryRead_Native(out ushort value, int offset)
	{
		if (!EnoughSpace(2 + offset))
		{
			value = 0;
			return false;
		}
		value = BitConverter.ToUInt16(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public bool TryRead_Native(out short value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out value, 0))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = Slice(2);
		return true;
	}

	public bool TryRead_Native(out short value, int offset)
	{
		if (!EnoughSpace(2 + offset))
		{
			value = 0;
			return false;
		}
		value = BitConverter.ToInt16(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public bool TryRead_Native(out uint value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out value, 0))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = Slice(4);
		return true;
	}

	public bool TryRead_Native(out uint value, int offset)
	{
		if (!EnoughSpace(4 + offset))
		{
			value = 0u;
			return false;
		}
		value = BitConverter.ToUInt32(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public bool TryRead_Native(out int value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out value, 0))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = Slice(4);
		return true;
	}

	public bool TryRead_Native(out int value, int offset)
	{
		if (!EnoughSpace(4 + offset))
		{
			value = 0;
			return false;
		}
		value = BitConverter.ToInt32(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public bool TryRead_Native(out ulong value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out value, 0))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = Slice(8);
		return true;
	}

	public bool TryRead_Native(out ulong value, int offset)
	{
		if (!EnoughSpace(8 + offset))
		{
			value = 0uL;
			return false;
		}
		value = BitConverter.ToUInt64(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public bool TryRead_Native(out long value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out value, 0))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = Slice(8);
		return true;
	}

	public bool TryRead_Native(out long value, int offset)
	{
		if (!EnoughSpace(8 + offset))
		{
			value = 0L;
			return false;
		}
		value = BitConverter.ToInt64(_bytes, _current + offset);
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public bool TryRead_Native(out float value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out value, 0))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = Slice(4);
		return true;
	}

	public unsafe bool TryRead_Native(out float value, int offset)
	{
		if (TryRead_Native(out int value2, offset))
		{
			value = *(float*)(&value2);
			return true;
		}
		value = 0f;
		return false;
	}

	public bool TryRead_Native(out double value, out JsonBinSpan nextSpan)
	{
		if (!TryRead_Native(out value, 0))
		{
			nextSpan = this;
			return false;
		}
		nextSpan = Slice(8);
		return true;
	}

	public unsafe bool TryRead_Native(out double value, int offset)
	{
		if (TryRead_Native(out long value2, offset))
		{
			value = *(double*)(&value2);
			return true;
		}
		value = 0.0;
		return false;
	}

	public uint ReverseEndian(uint uint32)
	{
		byte[] bytes = BitConverter.GetBytes(uint32);
		Array.Reverse(bytes);
		return BitConverter.ToUInt32(bytes, 0);
	}

	public ushort ReverseEndian(ushort uint16)
	{
		byte[] bytes = BitConverter.GetBytes(uint16);
		Array.Reverse(bytes);
		return BitConverter.ToUInt16(bytes, 0);
	}

	public ulong ReverseEndian(ulong uint64)
	{
		byte[] bytes = BitConverter.GetBytes(uint64);
		Array.Reverse(bytes);
		return BitConverter.ToUInt64(bytes, 0);
	}

	public short ReverseEndian(short int16)
	{
		byte[] bytes = BitConverter.GetBytes(int16);
		Array.Reverse(bytes);
		return BitConverter.ToInt16(bytes, 0);
	}

	public long ReverseEndian(long int64)
	{
		byte[] bytes = BitConverter.GetBytes(int64);
		Array.Reverse(bytes);
		return BitConverter.ToInt64(bytes, 0);
	}

	public int ReverseEndian(int int32)
	{
		byte[] bytes = BitConverter.GetBytes(int32);
		Array.Reverse(bytes);
		return BitConverter.ToInt32(bytes, 0);
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
