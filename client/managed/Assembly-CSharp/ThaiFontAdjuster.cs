using System.Text;

public static class ThaiFontAdjuster
{
	public static bool IsThaiString(string s)
	{
		int length = s.Length;
		for (int i = 0; i < length; i++)
		{
			char c = s[i];
			if (c >= '\u0e00' && c <= '\u0e7f')
			{
				return true;
			}
		}
		return false;
	}

	public static string Adjust(string s)
	{
		int length = s.Length;
		StringBuilder stringBuilder = new StringBuilder(length);
		for (int i = 0; i < length; i++)
		{
			char c = s[i];
			if (IsTop(c) && i > 0)
			{
				char c2 = s[i - 1];
				if (IsLower(c2) && i > 1)
				{
					c2 = s[i - 2];
				}
				if (IsBase(c2))
				{
					bool flag = i < length - 1 && (s[i + 1] == 'ำ' || s[i + 1] == '\u0e4d');
					if (IsBaseAsc(c2))
					{
						if (flag)
						{
							c = (char)(c + 59595);
							stringBuilder.Append('\uf711');
							stringBuilder.Append(c);
							if (s[i + 1] == 'ำ')
							{
								stringBuilder.Append('า');
							}
							i++;
							continue;
						}
						c = (char)(c + 59581);
					}
					else if (!flag)
					{
						c = (char)(c + 59586);
					}
				}
				if (i > 1 && IsUpper(s[i - 1]) && IsBaseAsc(s[i - 2]))
				{
					c = (char)(c + 59595);
				}
			}
			else if (IsUpper(c) && i > 0 && IsBaseAsc(s[i - 1]))
			{
				switch (c)
				{
				case '\u0e31':
					c = '\uf710';
					break;
				case '\u0e34':
					c = '\uf701';
					break;
				case '\u0e35':
					c = '\uf702';
					break;
				case '\u0e36':
					c = '\uf703';
					break;
				case '\u0e37':
					c = '\uf704';
					break;
				case '\u0e4d':
					c = '\uf711';
					break;
				case '\u0e47':
					c = '\uf712';
					break;
				}
			}
			else if (IsLower(c) && i > 0 && IsBaseDesc(s[i - 1]))
			{
				c = (char)(c + 59616);
			}
			else if (c == 'ญ' && i < length - 1 && IsLower(s[i + 1]))
			{
				c = '\uf70f';
			}
			else if (c == 'ฐ' && i < length - 1 && IsLower(s[i + 1]))
			{
				c = '\uf700';
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	private static bool IsBase(char c)
	{
		if ((c < 'ก' || c > 'ฯ') && c != 'ะ' && c != 'เ')
		{
			return c == 'แ';
		}
		return true;
	}

	private static bool IsBaseDesc(char c)
	{
		if (c != 'ฎ')
		{
			return c == 'ฏ';
		}
		return true;
	}

	private static bool IsBaseAsc(char c)
	{
		if (c != 'ป' && c != 'ฝ' && c != 'ฟ')
		{
			return c == 'ฬ';
		}
		return true;
	}

	private static bool IsTop(char c)
	{
		if (c >= '\u0e48')
		{
			return c <= '\u0e4c';
		}
		return false;
	}

	private static bool IsLower(char c)
	{
		if (c >= '\u0e38')
		{
			return c <= '\u0e3a';
		}
		return false;
	}

	private static bool IsUpper(char c)
	{
		if (c != '\u0e31' && c != '\u0e34' && c != '\u0e35' && c != '\u0e36' && c != '\u0e37' && c != '\u0e47')
		{
			return c == '\u0e4d';
		}
		return true;
	}
}
