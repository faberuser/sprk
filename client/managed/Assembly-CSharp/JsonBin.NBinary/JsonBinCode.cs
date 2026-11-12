using System.Text;

namespace JsonBin.NBinary;

public static class JsonBinCode
{
	public static readonly Encoding String_Encoding;

	public const byte MinFixInt = 0;

	public const byte MaxFixInt = 127;

	public const byte Nil = 192;

	public const byte NeverUsed = 193;

	public const byte False = 194;

	public const byte True = 195;

	public const byte Bin8 = 196;

	public const byte Bin16 = 197;

	public const byte Bin32 = 198;

	public const byte Float32 = 199;

	public const byte Float64 = 200;

	public const byte UInt8 = 201;

	public const byte UInt16 = 202;

	public const byte UInt32 = 203;

	public const byte UInt64 = 204;

	public const byte Int8 = 208;

	public const byte Int16 = 209;

	public const byte Int32 = 210;

	public const byte Int64 = 211;

	public const byte Str8 = 212;

	public const byte Str16 = 213;

	public const byte Str32 = 214;

	public const byte Array8 = 215;

	public const byte Array16 = 216;

	public const byte Array32 = 217;

	public const byte Map8 = 218;

	public const byte Map16 = 219;

	public const byte Map32 = 220;

	public const byte IndexedStr = 221;

	public const byte MinNegativeFixInt = 224;

	public const byte MaxNegativeFixInt = byte.MaxValue;

	private static readonly JsonType[] TypeLookupTable;

	private static readonly string[] FormatNameTable;

	static JsonBinCode()
	{
		String_Encoding = Encoding.UTF8;
		TypeLookupTable = new JsonType[256];
		FormatNameTable = new string[256];
		for (int i = 0; i < TypeLookupTable.Length; i++)
		{
			TypeLookupTable[i] = JsonType.Error;
		}
		for (int j = 0; j <= 127; j++)
		{
			TypeLookupTable[j] = JsonType.ULong;
			FormatNameTable[j] = "positive fixint";
		}
		TypeLookupTable[192] = JsonType.None;
		TypeLookupTable[193] = JsonType.None;
		TypeLookupTable[194] = JsonType.Boolean;
		TypeLookupTable[195] = JsonType.Boolean;
		TypeLookupTable[199] = JsonType.Float;
		TypeLookupTable[200] = JsonType.Double;
		TypeLookupTable[201] = JsonType.ULong;
		TypeLookupTable[202] = JsonType.ULong;
		TypeLookupTable[203] = JsonType.ULong;
		TypeLookupTable[204] = JsonType.ULong;
		TypeLookupTable[208] = JsonType.Long;
		TypeLookupTable[209] = JsonType.Long;
		TypeLookupTable[210] = JsonType.Long;
		TypeLookupTable[211] = JsonType.Long;
		TypeLookupTable[212] = JsonType.String;
		TypeLookupTable[213] = JsonType.String;
		TypeLookupTable[214] = JsonType.String;
		TypeLookupTable[215] = JsonType.Array;
		TypeLookupTable[216] = JsonType.Array;
		TypeLookupTable[217] = JsonType.Array;
		TypeLookupTable[218] = JsonType.Object;
		TypeLookupTable[219] = JsonType.Object;
		TypeLookupTable[220] = JsonType.Object;
		TypeLookupTable[221] = JsonType.String;
		FormatNameTable[192] = "nil";
		FormatNameTable[193] = "(never used)";
		FormatNameTable[194] = "false";
		FormatNameTable[195] = "true";
		FormatNameTable[196] = "bin 8";
		FormatNameTable[197] = "bin 16";
		FormatNameTable[198] = "bin 32";
		FormatNameTable[199] = "float 32";
		FormatNameTable[200] = "float 64";
		FormatNameTable[201] = "uint 8";
		FormatNameTable[202] = "uint 16";
		FormatNameTable[203] = "uint 32";
		FormatNameTable[204] = "uint 64";
		FormatNameTable[208] = "int 8";
		FormatNameTable[209] = "int 16";
		FormatNameTable[210] = "int 32";
		FormatNameTable[211] = "int 64";
		FormatNameTable[212] = "str 8";
		FormatNameTable[213] = "str 16";
		FormatNameTable[214] = "str 32";
		FormatNameTable[215] = "array 8";
		FormatNameTable[216] = "array 16";
		FormatNameTable[217] = "array 32";
		FormatNameTable[218] = "map 8";
		FormatNameTable[219] = "map 16";
		FormatNameTable[220] = "map 32";
		FormatNameTable[221] = "hashed str";
		for (int k = 224; k <= 255; k++)
		{
			TypeLookupTable[k] = JsonType.Long;
			FormatNameTable[k] = "negative fixint";
		}
	}

	public static JsonType ToJsonType(byte code)
	{
		return TypeLookupTable[code];
	}

	public static string ToFormatName(byte code)
	{
		return FormatNameTable[code];
	}
}
