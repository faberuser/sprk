using System;
using System.Collections.Generic;
using System.Text;
using NPatchGenerator;

namespace NVespa;

internal class Utf8CsvReader
{
	internal static class ThreadLocalCache
	{
		[ThreadStatic]
		private static byte[] buffer;

		[ThreadStatic]
		private static char[] codePointStringBuffer;

		[ThreadStatic]
		private static BetterList<string> stringList;

		public static byte[] GetBuffer()
		{
			if (buffer == null)
			{
				buffer = new byte[2048];
			}
			return buffer;
		}

		public static char[] GetCodePointStringBuffer()
		{
			if (codePointStringBuffer == null)
			{
				codePointStringBuffer = new char[2048];
			}
			return codePointStringBuffer;
		}

		public static BetterList<string> GetStringList()
		{
			if (stringList == null)
			{
				stringList = new BetterList<string>();
			}
			else
			{
				stringList.Clear();
			}
			return stringList;
		}
	}

	private const int MAX_FIELD_BUFFER = 2048;

	private static readonly Encoding UTF8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	private static readonly byte[] bom = Encoding.UTF8.GetPreamble();

	private readonly byte[] bytes;

	private int offset;

	public Utf8CsvReader(byte[] bytes)
		: this(bytes, 0)
	{
	}

	public Utf8CsvReader(byte[] bytes, int offset)
	{
		this.bytes = bytes;
		this.offset = offset;
		if (bytes.Length >= 3 && bytes[offset] == bom[0] && bytes[offset + 1] == bom[1] && bytes[offset + 2] == bom[2])
		{
			this.offset = (offset += 3);
		}
	}

	public BetterList<string> ReadLine()
	{
		BetterList<string> stringList = ThreadLocalCache.GetStringList();
		int num = bytes.Length;
		bool flag = false;
		int num2 = offset;
		int num3 = -1;
		byte[] array = null;
		if (offset >= num)
		{
			return null;
		}
		int num4 = offset;
		while (num4 < num)
		{
			if (flag)
			{
				if (bytes[num4] == 34)
				{
					if (num4 + 1 >= num || bytes[num4 + 1] != 34)
					{
						goto IL_011a;
					}
					if (num3 == -1)
					{
						if (array == null)
						{
							array = ThreadLocalCache.GetBuffer();
						}
						num3 = num4 - offset + 1;
						Buffer.BlockCopy(bytes, num2, array, 0, num3);
					}
					else
					{
						array[num3++] = bytes[num4];
					}
				}
				else if (num3 != -1)
				{
					array[num3++] = bytes[num4];
				}
				goto IL_0112;
			}
			byte b = bytes[num4];
			if ((uint)b <= 13u)
			{
				if (b != 10 && b != 13)
				{
					goto IL_00fa;
				}
			}
			else
			{
				if (b == 34)
				{
					num2 = num4 + 1;
					flag = true;
					goto IL_0112;
				}
				if (b != 44)
				{
					goto IL_00fa;
				}
			}
			goto IL_011a;
			IL_00fa:
			if (num3 != -1)
			{
				array[num3++] = bytes[num4];
			}
			goto IL_0112;
			IL_011a:
			if (num3 == -1)
			{
				stringList.Add(UTF8.GetString(bytes, num2, num4 - num2));
			}
			else
			{
				stringList.Add(UTF8.GetString(array, 0, num3));
				num3 = -1;
			}
			offset = num4 + 1;
			if (flag)
			{
				offset++;
				flag = false;
			}
			if (SkipNewLine())
			{
				return stringList;
			}
			num4 = (num2 = offset);
			continue;
			IL_0112:
			num4++;
		}
		return stringList;
	}

	public bool ReadLines(ref Dictionary<uint, string> dict, int keyColumn, int valueColumn)
	{
		int num = bytes.Length;
		bool flag = false;
		int num2 = offset;
		int num3 = -1;
		byte[] array = null;
		char[] codePointStringBuffer = ThreadLocalCache.GetCodePointStringBuffer();
		int num4 = -1;
		int num5 = 0;
		uint num6 = 0u;
		string value = null;
		if (offset >= num)
		{
			return false;
		}
		int num7 = offset;
		while (num7 < num)
		{
			if (flag)
			{
				if (bytes[num7] == 34)
				{
					if (num7 + 1 >= num || bytes[num7 + 1] != 34)
					{
						goto IL_011e;
					}
					if (num3 == -1)
					{
						if (array == null)
						{
							array = ThreadLocalCache.GetBuffer();
						}
						num3 = num7 - offset + 1;
						Buffer.BlockCopy(bytes, num2, array, 0, num3);
					}
					else
					{
						array[num3++] = bytes[num7];
					}
				}
				else if (num3 != -1)
				{
					array[num3++] = bytes[num7];
				}
				goto IL_0113;
			}
			byte b = bytes[num7];
			if ((uint)b <= 13u)
			{
				if (b != 10 && b != 13)
				{
					goto IL_00fe;
				}
			}
			else
			{
				if (b == 34)
				{
					num2 = num7 + 1;
					flag = true;
					goto IL_0113;
				}
				if (b != 44)
				{
					goto IL_00fe;
				}
			}
			goto IL_011e;
			IL_00fe:
			if (num3 != -1)
			{
				array[num3++] = bytes[num7];
			}
			goto IL_0113;
			IL_011e:
			if (num3 == -1)
			{
				if (num5 == keyColumn)
				{
					num4 = UTF8.GetChars(bytes, num2, num7 - num2, codePointStringBuffer, 0);
					num6 = FileUtility.CharsToHashCode(codePointStringBuffer, num4);
				}
				else if (num5 == valueColumn)
				{
					value = UTF8.GetString(bytes, num2, num7 - num2);
					value = value.Replace("\\n", "\n");
				}
			}
			else
			{
				if (num5 == keyColumn)
				{
					num4 = UTF8.GetChars(array, 0, num3, codePointStringBuffer, 0);
					num6 = FileUtility.CharsToHashCode(codePointStringBuffer, num4);
				}
				else if (num5 == valueColumn)
				{
					value = UTF8.GetString(array, 0, num3);
					value = value.Replace("\\n", "\n");
				}
				num3 = -1;
			}
			offset = num7 + 1;
			if (flag)
			{
				offset++;
				flag = false;
			}
			if (SkipNewLine())
			{
				num5 = 0;
				if (num6 != 0)
				{
					dict[num6] = value;
				}
				num6 = 0u;
				value = string.Empty;
			}
			else
			{
				num5++;
			}
			num7 = (num2 = offset);
			continue;
			IL_0113:
			num7++;
		}
		return true;
	}

	public bool ReadLines(ref Dictionary<string, string> dict, int keyColumn, int valueColumn)
	{
		int num = bytes.Length;
		bool flag = false;
		int num2 = offset;
		int num3 = -1;
		byte[] array = null;
		ThreadLocalCache.GetCodePointStringBuffer();
		int num4 = 0;
		string text = null;
		string value = null;
		if (offset >= num)
		{
			return false;
		}
		int num5 = offset;
		while (num5 < num)
		{
			if (flag)
			{
				if (bytes[num5] == 34)
				{
					if (num5 + 1 >= num || bytes[num5 + 1] != 34)
					{
						goto IL_011a;
					}
					if (num3 == -1)
					{
						if (array == null)
						{
							array = ThreadLocalCache.GetBuffer();
						}
						num3 = num5 - offset + 1;
						Buffer.BlockCopy(bytes, num2, array, 0, num3);
					}
					else
					{
						array[num3++] = bytes[num5];
					}
				}
				else if (num3 != -1)
				{
					array[num3++] = bytes[num5];
				}
				goto IL_010f;
			}
			byte b = bytes[num5];
			if ((uint)b <= 13u)
			{
				if (b != 10 && b != 13)
				{
					goto IL_00fa;
				}
			}
			else
			{
				if (b == 34)
				{
					num2 = num5 + 1;
					flag = true;
					goto IL_010f;
				}
				if (b != 44)
				{
					goto IL_00fa;
				}
			}
			goto IL_011a;
			IL_00fa:
			if (num3 != -1)
			{
				array[num3++] = bytes[num5];
			}
			goto IL_010f;
			IL_011a:
			if (num3 == -1)
			{
				if (num4 == keyColumn)
				{
					text = UTF8.GetString(bytes, num2, num5 - num2);
				}
				else if (num4 == valueColumn)
				{
					value = UTF8.GetString(bytes, num2, num5 - num2);
					value = value.Replace("\\n", "\n");
				}
			}
			else
			{
				if (num4 == keyColumn)
				{
					text = UTF8.GetString(array, 0, num3);
				}
				else if (num4 == valueColumn)
				{
					value = UTF8.GetString(array, 0, num3);
					value = value.Replace("\\n", "\n");
				}
				num3 = -1;
			}
			offset = num5 + 1;
			if (flag)
			{
				offset++;
				flag = false;
			}
			if (SkipNewLine())
			{
				num4 = 0;
				if (text != null && !String_Extension_Method.IsNullOrEmpty(text))
				{
					dict[text] = value;
				}
				text = string.Empty;
				value = string.Empty;
			}
			else
			{
				num4++;
			}
			num5 = (num2 = offset);
			continue;
			IL_010f:
			num5++;
		}
		return true;
	}

	public bool SkipNewLine()
	{
		int num = bytes.Length;
		for (int i = offset; i < num; i++)
		{
			byte b = bytes[i];
			if (b != 10 && b != 13)
			{
				if (offset == i)
				{
					return false;
				}
				offset = i;
				return true;
			}
		}
		offset = num;
		return true;
	}
}
