using System;
using System.Collections.Generic;

namespace NNguiExtension;

public class CsvReader
{
	private ILineReader _reader;

	private static BetterList<string> mTemp = new BetterList<string>();

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

	public CsvReader(ILineReader lineReader)
	{
		_reader = lineReader;
	}

	public Dictionary<string, string> ReadDictionary()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		char[] separator = new char[1] { '=' };
		while (canRead)
		{
			string text = _reader.ReadLine();
			if (text == null)
			{
				break;
			}
			if (!text.StartsWith("//"))
			{
				string[] array = text.Split(separator, 2, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length == 2)
				{
					string key = array[0].Trim();
					string value = array[1].Trim().Replace("\\n", "\n");
					dictionary[key] = value;
				}
			}
		}
		return dictionary;
	}

	public BetterList<string> ReadCSV()
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

	public List<string> ReadCSV2(List<string> lst)
	{
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
						lst.Add(item);
						num = i + 1;
					}
					break;
				case '"':
					if (flag)
					{
						if (i + 1 >= length)
						{
							lst.Add(text.Substring(num, i - num).Replace("\"\"", "\""));
							return lst;
						}
						if (text[i + 1] != '"')
						{
							lst.Add(text.Substring(num, i - num).Replace("\"\"", "\""));
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
					lst.Add(text.Substring(num, text.Length - num));
				}
				return lst;
			}
		}
		return null;
	}
}
