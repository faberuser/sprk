using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NShared.NLocalization;
using NVespa;

namespace NClientData.NLocalization;

public class CsvLoader
{
	public class SentenceInfo
	{
		public string Key { get; set; }

		public string Korean { get; set; }

		public string Value { get; set; }

		public LanguageType Language { get; set; }

		public SentenceInfo(string[] csvLine, LanguageType language)
		{
			FromCSV(csvLine, language);
			Language = language;
		}

		public void FromCSV(string[] csvLine, LanguageType language)
		{
			try
			{
				if (language == LanguageType.Korean)
				{
					if (csvLine.Length != 0)
					{
						Key = csvLine[0];
					}
					if (csvLine.Length > 1)
					{
						Korean = csvLine[1];
					}
					return;
				}
				if (csvLine.Length != 0)
				{
					Key = csvLine[0];
				}
				if (csvLine.Length > 1)
				{
					Korean = csvLine[1];
				}
				if (csvLine.Length > 2)
				{
					Value = csvLine[2];
				}
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("SentenceInfo FromCSV Error : " + csvLine?.ToString() + "\n" + ex.ToString());
			}
		}
	}

	public class LineReader
	{
		private byte[] mBuffer;

		private int mOffset;

		public bool canRead
		{
			get
			{
				if (mBuffer != null)
				{
					return mOffset < mBuffer.Length;
				}
				return false;
			}
		}

		public LineReader(byte[] bytes)
		{
			mBuffer = bytes;
		}

		public static LineReader Open(string path)
		{
			FileStream fileStream = File.OpenRead(path);
			if (fileStream != null)
			{
				fileStream.Seek(0L, SeekOrigin.End);
				byte[] array = new byte[fileStream.Position];
				fileStream.Seek(0L, SeekOrigin.Begin);
				fileStream.Read(array, 0, array.Length);
				fileStream.Close();
				return new LineReader(array);
			}
			return null;
		}

		private static string ReadLine(byte[] buffer, int start, int count)
		{
			return Encoding.UTF8.GetString(buffer, start, count);
		}

		public string ReadLine()
		{
			return ReadLine(skipEmptyLines: true);
		}

		public string ReadLine(bool skipEmptyLines)
		{
			int num = mBuffer.Length;
			if (skipEmptyLines)
			{
				while (mOffset < num && mBuffer[mOffset] < 32)
				{
					mOffset++;
				}
			}
			int num2 = mOffset;
			if (num2 < num)
			{
				int num3;
				do
				{
					if (num2 < num)
					{
						num3 = mBuffer[num2++];
						continue;
					}
					num2++;
					break;
				}
				while (num3 != 10 && num3 != 13);
				string result = ReadLine(mBuffer, mOffset, num2 - mOffset - 1);
				mOffset = num2;
				return result;
			}
			mOffset = num;
			return null;
		}
	}

	public class CsvReader
	{
		private LineReader _reader;

		private static List<string> mTemp = new List<string>();

		public bool canRead
		{
			get
			{
				if (_reader != null)
				{
					return _reader.canRead;
				}
				return false;
			}
		}

		public CsvReader(LineReader lineReader)
		{
			_reader = lineReader;
		}

		public List<string> ReadCSV()
		{
			mTemp.Clear();
			string text = "";
			bool flag = false;
			int num = 0;
			while (canRead)
			{
				if (flag)
				{
					string text2 = _reader.ReadLine(skipEmptyLines: false);
					if (text2 == null)
					{
						return null;
					}
					text2 = text2.Replace("\\n", "\n");
					text = text + "\n" + text2;
				}
				else
				{
					text = _reader.ReadLine(skipEmptyLines: true);
					if (text == null)
					{
						return null;
					}
					text = text.Replace("\\n", "\n");
					num = 0;
				}
				int i = num;
				for (int length = text.Length; i < length; i++)
				{
					switch (text[i])
					{
					case ',':
						if (!flag)
						{
							string item = text.Substring(num, i - num);
							mTemp.Add(item);
							num = i + 1;
						}
						break;
					case '"':
						if (flag)
						{
							if (i + 1 >= length)
							{
								mTemp.Add(text.Substring(num, i - num).Replace("\"\"", "\""));
								return mTemp;
							}
							if (text[i + 1] != '"')
							{
								mTemp.Add(text.Substring(num, i - num).Replace("\"\"", "\""));
								flag = false;
								if (text[i + 1] == ',')
								{
									i++;
									num = i + 1;
								}
							}
							else
							{
								i++;
							}
						}
						else
						{
							num = i + 1;
							flag = true;
						}
						break;
					}
				}
				if (!flag)
				{
					if (num < text.Length)
					{
						mTemp.Add(text.Substring(num, text.Length - num));
					}
					return mTemp;
				}
			}
			return null;
		}
	}

	public Dictionary<string, string> Dic = new Dictionary<string, string>();

	private Dictionary<string, string> _keyFrom = new Dictionary<string, string>();

	public LanguageType LoadCsv(byte[] bytes, string filename)
	{
		if (bytes == null)
		{
			return LanguageType.Unknown;
		}
		CsvReader csvReader = new CsvReader(new LineReader(bytes));
		List<string> list = csvReader.ReadCSV();
		if (list == null || list.Count < 2)
		{
			return LanguageType.Unknown;
		}
		if (list.Count == 2)
		{
			if (!Vespa_String_Extension_Method.TryToEnum<LanguageType>(list[1], out var o_output))
			{
				o_output = LanguageType.Unknown;
			}
			if (o_output == LanguageType.Unknown)
			{
				return LanguageType.Unknown;
			}
			if (!LoadLegacyTable(csvReader, o_output, filename))
			{
				return LanguageType.Unknown;
			}
			return o_output;
		}
		int index = ((list.Count == 5) ? 1 : 2);
		if (!Vespa_String_Extension_Method.TryToEnum<LanguageType>(list[index], out var o_output2))
		{
			o_output2 = LanguageType.Unknown;
		}
		if (o_output2 == LanguageType.Unknown)
		{
			return LanguageType.Unknown;
		}
		if (!LoadFullTable(csvReader, o_output2, filename))
		{
			return LanguageType.Unknown;
		}
		return o_output2;
	}

	private bool LoadFullTable(CsvReader reader, LanguageType curLang, string filename)
	{
		while (true)
		{
			List<string> list = reader.ReadCSV();
			if (list == null || list.Count == 0)
			{
				break;
			}
			if (!String_Extension_Method.IsNullOrEmpty(list[0]))
			{
				SentenceInfo sentenceInfo = new SentenceInfo(list.ToArray(), curLang);
				string text = "";
				text = ((curLang != LanguageType.Korean) ? sentenceInfo.Value : sentenceInfo.Korean);
				if (!String_Extension_Method.IsNullOrEmpty(text))
				{
					AddCsv(sentenceInfo.Key, text, curLang, filename);
				}
			}
		}
		return true;
	}

	private bool LoadLegacyTable(CsvReader reader, LanguageType curLang, string filename)
	{
		while (true)
		{
			List<string> list = reader.ReadCSV();
			if (list == null || list.Count == 0)
			{
				break;
			}
			if (!String_Extension_Method.IsNullOrEmpty(list[0]))
			{
				string curKey = list[0];
				string text = "";
				if (list.Count > 1)
				{
					text = list[1];
				}
				if (!String_Extension_Method.IsNullOrEmpty(text))
				{
					AddCsv(curKey, text, curLang, filename);
				}
			}
		}
		return true;
	}

	private void AddCsv(string curKey, string text, LanguageType language, string filename)
	{
		if (Dic.TryGetValue(curKey, out var value))
		{
			string vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_keyFrom, curKey, string.Empty);
			string text2 = $"Key={curKey}, OldFrom={vALUE}, CurFrom{filename}";
			if (value == text)
			{
				VespaDebug.LogError("Duplicate Same Key: " + text2);
			}
			else
			{
				VespaDebug.LogError("Duplicate Diff Key: " + text2);
			}
		}
		Dic[curKey] = text;
		_keyFrom[curKey] = filename;
	}

	public bool Exists(string key)
	{
		return Dic.ContainsKey(key);
	}

	public string Get(string key)
	{
		if (String_Extension_Method.IsNullOrEmpty(key))
		{
			return string.Empty;
		}
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(Dic, key, key);
	}

	public bool TryGet(string key, out string value)
	{
		if (String_Extension_Method.IsNullOrEmpty(key))
		{
			value = string.Empty;
			return false;
		}
		return Dic.TryGetValue(key, out value);
	}

	public void Clear()
	{
		Dic.Clear();
	}
}
