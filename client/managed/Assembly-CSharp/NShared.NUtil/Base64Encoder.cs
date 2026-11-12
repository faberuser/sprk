using System;
using System.Text;

namespace NShared.NUtil;

public class Base64Encoder
{
	public static string ToBase64String(string encodingText, Encoding encoding = null)
	{
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		return Convert.ToBase64String(encoding.GetBytes(encodingText));
	}

	public static string FromBase64String(string decodingText, Encoding encoding = null)
	{
		if (encoding == null)
		{
			encoding = Encoding.UTF8;
		}
		byte[] bytes = Convert.FromBase64String(decodingText);
		return encoding.GetString(bytes);
	}
}
