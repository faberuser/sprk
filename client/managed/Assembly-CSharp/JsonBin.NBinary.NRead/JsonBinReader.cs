using System;
using System.Collections.Generic;
using System.IO;

namespace JsonBin.NBinary.NRead;

public struct JsonBinReader
{
	private byte[] _bytes;

	private int _current;

	public bool IsValid => _bytes != null;

	public JsonBinReader(byte[] bytes)
	{
		_bytes = bytes;
		_current = 0;
	}

	public JsonBinReader(byte[] bytes, int position)
	{
		_bytes = bytes;
		_current = position;
	}

	public void Advance(int length)
	{
		_current += length;
	}

	public JsonBinReader Slice(int index)
	{
		return new JsonBinReader(_bytes, _current + index);
	}

	public JsonBinSpan Span(int index)
	{
		return new JsonBinSpan(_bytes, _current + index);
	}

	public void ReadPacked_Nil()
	{
		if (!TryReadPacked_Nil())
		{
			throw ThrowInvalidCall();
		}
	}

	public bool TryReadPacked_Nil()
	{
		if (!TryRead_Native(out byte value))
		{
			return false;
		}
		if (value != 192)
		{
			return false;
		}
		return true;
	}

	public bool ReadPacked_Boolean()
	{
		if (TryReadPacked_Boolean(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_Boolean(out bool value)
	{
		if (!TryRead_Native(out byte value2))
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

	public string ReadPacked_String()
	{
		if (TryReadPacked_String(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_String(out string value)
	{
		if (!TryRead_Native(out byte value2))
		{
			value = string.Empty;
			return false;
		}
		if (value2 == 192)
		{
			value = string.Empty;
			return true;
		}
		if (!TryReadPacked_StringLength(value2, out var length))
		{
			value = string.Empty;
			return false;
		}
		if (length == 0)
		{
			value = string.Empty;
			return true;
		}
		ThrowInsufficientBufferUnless(EnoughSpace(length));
		value = JsonBinCode.String_Encoding.GetString(_bytes, _current, length);
		_current += length;
		return true;
	}

	private bool TryReadPacked_StringLength(byte code, out int length)
	{
		switch (code)
		{
		case 212:
		{
			if (TryRead_Native(out byte value2))
			{
				length = value2;
				return true;
			}
			break;
		}
		case 213:
		{
			if (TryRead_Native(out ushort value3))
			{
				length = value3;
				return true;
			}
			break;
		}
		case 214:
		{
			if (TryRead_Native(out int value))
			{
				length = value;
				return true;
			}
			break;
		}
		}
		length = 0;
		return false;
	}

	public byte ReadPacked_Byte()
	{
		if (TryReadPacked_Byte(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_Byte(out byte value)
	{
		if (!TryRead_Native(out byte value2))
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
				bool result2 = TryRead_Native(out value4);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3);
				value = (byte)value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5);
				value = (byte)value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10);
				value = (byte)value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9);
				value = (byte)value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8);
				value = (byte)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7);
				value = (byte)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6);
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

	public sbyte ReadPacked_SByte()
	{
		if (TryReadPacked_SByte(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_SByte(out sbyte value)
	{
		if (!TryRead_Native(out byte value2))
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
				bool result2 = TryRead_Native(out value4);
				value = (sbyte)value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3);
				value = value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5);
				value = (sbyte)value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10);
				value = (sbyte)value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9);
				value = (sbyte)value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8);
				value = (sbyte)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7);
				value = (sbyte)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6);
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

	public ushort ReadPacked_UShort()
	{
		if (TryReadPacked_UShort(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_UShort(out ushort value)
	{
		if (!TryRead_Native(out byte value2))
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
				bool result2 = TryRead_Native(out value4);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3);
				value = (ushort)value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5);
				value = value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10);
				value = (ushort)value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9);
				value = (ushort)value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8);
				value = (ushort)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7);
				value = (ushort)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6);
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

	public short ReadPacked_Short()
	{
		if (TryReadPacked_Short(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_Short(out short value)
	{
		if (!TryRead_Native(out byte value2))
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
				bool result2 = TryRead_Native(out value4);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3);
				value = value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5);
				value = (short)value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10);
				value = value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9);
				value = (short)value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8);
				value = (short)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7);
				value = (short)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6);
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

	public uint ReadPacked_UInt()
	{
		if (TryReadPacked_UInt(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_UInt(out uint value)
	{
		if (!TryRead_Native(out byte value2))
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
				bool result2 = TryRead_Native(out value4);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3);
				value = (uint)value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5);
				value = value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10);
				value = (uint)value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9);
				value = value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8);
				value = (uint)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7);
				value = (uint)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6);
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

	public int ReadPacked_Int()
	{
		if (TryReadPacked_Int(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_Int(out int value)
	{
		if (!TryRead_Native(out byte value2))
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
				bool result2 = TryRead_Native(out value4);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3);
				value = value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5);
				value = value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10);
				value = value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9);
				value = (int)value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8);
				value = value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7);
				value = (int)value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6);
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

	public ulong ReadPacked_ULong()
	{
		if (TryReadPacked_ULong(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_ULong(out ulong value)
	{
		if (!TryRead_Native(out byte value2))
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
				bool result2 = TryRead_Native(out value4);
				value = value4;
				return result2;
			}
			case 208:
			{
				sbyte value3;
				bool result = TryRead_Native(out value3);
				value = (ulong)value3;
				return result;
			}
			case 202:
			{
				ushort value5;
				bool result3 = TryRead_Native(out value5);
				value = value5;
				return result3;
			}
			case 209:
			{
				short value10;
				bool result8 = TryRead_Native(out value10);
				value = (ulong)value10;
				return result8;
			}
			case 203:
			{
				uint value9;
				bool result7 = TryRead_Native(out value9);
				value = value9;
				return result7;
			}
			case 210:
			{
				int value8;
				bool result6 = TryRead_Native(out value8);
				value = (ulong)value8;
				return result6;
			}
			case 204:
			{
				ulong value7;
				bool result5 = TryRead_Native(out value7);
				value = value7;
				return result5;
			}
			case 211:
			{
				long value6;
				bool result4 = TryRead_Native(out value6);
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

	public long ReadPacked_Long()
	{
		if (TryReadPacked_Long(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_Long(out long value)
	{
		if (!TryRead_Native(out byte value2))
		{
			value = 0L;
			return false;
		}
		switch (value2)
		{
		case 201:
		{
			byte value4;
			bool result2 = TryRead_Native(out value4);
			value = value4;
			return result2;
		}
		case 208:
		{
			sbyte value3;
			bool result = TryRead_Native(out value3);
			value = value3;
			return result;
		}
		case 202:
		{
			ushort value5;
			bool result3 = TryRead_Native(out value5);
			value = value5;
			return result3;
		}
		case 209:
		{
			short value10;
			bool result8 = TryRead_Native(out value10);
			value = value10;
			return result8;
		}
		case 203:
		{
			uint value9;
			bool result7 = TryRead_Native(out value9);
			value = value9;
			return result7;
		}
		case 210:
		{
			int value8;
			bool result6 = TryRead_Native(out value8);
			value = value8;
			return result6;
		}
		case 204:
		{
			ulong value7;
			bool result5 = TryRead_Native(out value7);
			value = checked((long)value7);
			return result5;
		}
		case 211:
		{
			long value6;
			bool result4 = TryRead_Native(out value6);
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

	public float ReadPacked_Float()
	{
		if (TryReadPacked_Float(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_Float(out float value)
	{
		if (!TryRead_Native(out byte value2))
		{
			value = 0f;
			return false;
		}
		switch (value2)
		{
		case 199:
		{
			float value4;
			bool result2 = TryRead_Native(out value4);
			value = value4;
			return result2;
		}
		case 200:
		{
			double value3;
			bool result = TryRead_Native(out value3);
			value = (float)value3;
			return result;
		}
		case 208:
		{
			sbyte value5;
			bool result3 = TryRead_Native(out value5);
			value = value5;
			return result3;
		}
		case 209:
		{
			short value12;
			bool result10 = TryRead_Native(out value12);
			value = value12;
			return result10;
		}
		case 210:
		{
			int value11;
			bool result9 = TryRead_Native(out value11);
			value = value11;
			return result9;
		}
		case 211:
		{
			long value10;
			bool result8 = TryRead_Native(out value10);
			value = value10;
			return result8;
		}
		case 201:
		{
			byte value9;
			bool result7 = TryRead_Native(out value9);
			value = (int)value9;
			return result7;
		}
		case 202:
		{
			ushort value8;
			bool result6 = TryRead_Native(out value8);
			value = (int)value8;
			return result6;
		}
		case 203:
		{
			uint value7;
			bool result5 = TryRead_Native(out value7);
			value = value7;
			return result5;
		}
		case 204:
		{
			ulong value6;
			bool result4 = TryRead_Native(out value6);
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

	public double ReadPacked_Double()
	{
		if (TryReadPacked_Double(out var value))
		{
			return value;
		}
		throw ThrowInvalidCall();
	}

	public bool TryReadPacked_Double(out double value)
	{
		if (!TryRead_Native(out byte value2))
		{
			value = 0.0;
			return false;
		}
		switch (value2)
		{
		case 199:
		{
			float value4;
			bool result2 = TryRead_Native(out value4);
			value = value4;
			return result2;
		}
		case 200:
		{
			double value3;
			bool result = TryRead_Native(out value3);
			value = value3;
			return result;
		}
		case 208:
		{
			sbyte value5;
			bool result3 = TryRead_Native(out value5);
			value = value5;
			return result3;
		}
		case 209:
		{
			short value12;
			bool result10 = TryRead_Native(out value12);
			value = value12;
			return result10;
		}
		case 210:
		{
			int value11;
			bool result9 = TryRead_Native(out value11);
			value = value11;
			return result9;
		}
		case 211:
		{
			long value10;
			bool result8 = TryRead_Native(out value10);
			value = value10;
			return result8;
		}
		case 201:
		{
			byte value9;
			bool result7 = TryRead_Native(out value9);
			value = (int)value9;
			return result7;
		}
		case 202:
		{
			ushort value8;
			bool result6 = TryRead_Native(out value8);
			value = (int)value8;
			return result6;
		}
		case 203:
		{
			uint value7;
			bool result5 = TryRead_Native(out value7);
			value = value7;
			return result5;
		}
		case 204:
		{
			ulong value6;
			bool result4 = TryRead_Native(out value6);
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

	public List<int> ReadPacked_ArrayHeader()
	{
		List<int> list = TryReadPacked_ArrayHeader();
		if (list != null)
		{
			return list;
		}
		throw ThrowInvalidCall();
	}

	public List<int> TryReadPacked_ArrayHeader()
	{
		if (!TryRead_Native(out byte value))
		{
			return null;
		}
		switch (value)
		{
		case 215:
		{
			if (!TryRead_Native(out byte value6))
			{
				return null;
			}
			if (!EnoughSpace(value6))
			{
				return null;
			}
			List<int> list3 = new List<int>();
			for (int k = 0; k < value6; k++)
			{
				if (TryRead_Native(out byte value7))
				{
					list3.Add(value7);
				}
			}
			return list3;
		}
		case 216:
		{
			if (!TryRead_Native(out ushort value4))
			{
				return null;
			}
			if (!EnoughSpace(value4 * 2))
			{
				return null;
			}
			List<int> list2 = new List<int>();
			for (int j = 0; j < value4; j++)
			{
				if (TryRead_Native(out ushort value5))
				{
					list2.Add(value5);
				}
			}
			return list2;
		}
		case 217:
		{
			if (!TryRead_Native(out int value2))
			{
				return null;
			}
			if (!EnoughSpace(value2 * 4))
			{
				return null;
			}
			List<int> list = new List<int>();
			for (int i = 0; i < value2; i++)
			{
				if (TryRead_Native(out int value3))
				{
					list.Add(value3);
				}
			}
			return list;
		}
		default:
			return null;
		}
	}

	public bool TryRead_Native(out byte value)
	{
		if (!EnoughSpace(1))
		{
			value = 0;
			return false;
		}
		value = _bytes[_current];
		_current++;
		return true;
	}

	public bool TryRead_Native(out sbyte value)
	{
		if (!EnoughSpace(1))
		{
			value = 0;
			return false;
		}
		value = (sbyte)_bytes[_current];
		_current++;
		return true;
	}

	public bool TryRead_Native(out ushort value)
	{
		if (!EnoughSpace(2))
		{
			value = 0;
			return false;
		}
		value = BitConverter.ToUInt16(_bytes, _current);
		_current += 2;
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public bool TryRead_Native(out short value)
	{
		if (!EnoughSpace(2))
		{
			value = 0;
			return false;
		}
		value = BitConverter.ToInt16(_bytes, _current);
		_current += 2;
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public bool TryRead_Native(out uint value)
	{
		if (!EnoughSpace(4))
		{
			value = 0u;
			return false;
		}
		value = BitConverter.ToUInt32(_bytes, _current);
		_current += 4;
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
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

	public bool TryRead_Native(out int value)
	{
		if (!EnoughSpace(4))
		{
			value = 0;
			return false;
		}
		value = BitConverter.ToInt32(_bytes, _current);
		_current += 4;
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public bool TryRead_Native(out ulong value)
	{
		if (!EnoughSpace(8))
		{
			value = 0uL;
			return false;
		}
		value = BitConverter.ToUInt64(_bytes, _current);
		_current += 8;
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public bool TryRead_Native(out long value)
	{
		if (!EnoughSpace(8))
		{
			value = 0L;
			return false;
		}
		value = BitConverter.ToInt64(_bytes, _current);
		_current += 8;
		if (!BitConverter.IsLittleEndian)
		{
			value = ReverseEndian(value);
		}
		return true;
	}

	public unsafe bool TryRead_Native(out float value)
	{
		if (TryRead_Native(out int value2))
		{
			value = *(float*)(&value2);
			return true;
		}
		value = 0f;
		return false;
	}

	public unsafe bool TryRead_Native(out double value)
	{
		if (TryRead_Native(out long value2))
		{
			value = *(double*)(&value2);
			return true;
		}
		value = 0.0;
		return false;
	}

	public byte PeekByte()
	{
		if (!EnoughSpace(1))
		{
			return 0;
		}
		return _bytes[_current];
	}

	private bool EnoughSpace(int size)
	{
		if (_bytes == null)
		{
			return false;
		}
		return _current + size <= _bytes.Length;
	}

	private static EndOfStreamException ThrowNotEnoughBytesException()
	{
		throw new EndOfStreamException();
	}

	private static void ThrowInsufficientBufferUnless(bool condition)
	{
		if (!condition)
		{
			ThrowNotEnoughBytesException();
		}
	}

	private static Exception ThrowInvalidCode(byte code)
	{
		throw new Exception($"Unexpected msgpack code {code} ({JsonBinCode.ToFormatName(code)}) encountered.");
	}

	private static Exception ThrowInvalidCall()
	{
		throw new Exception("Unexpected binary read encountered.");
	}
}
