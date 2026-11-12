using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NGame2.NUtil.NSystemInfo;

public class SupportTextureFormatChecker
{
	private Dictionary<TextureFormat, bool> _supportedTextureFormatDict = new Dictionary<TextureFormat, bool>();

	private string _logCache;

	public SupportTextureFormatChecker()
	{
		CheckTextureForamt(TextureFormat.ASTC_4x4);
		CheckTextureForamt(TextureFormat.ASTC_5x5);
		CheckTextureForamt(TextureFormat.ASTC_6x6);
		CheckTextureForamt(TextureFormat.ASTC_8x8);
		CheckTextureForamt(TextureFormat.ASTC_10x10);
		CheckTextureForamt(TextureFormat.ASTC_12x12);
		CheckTextureForamt(TextureFormat.ASTC_4x4);
		CheckTextureForamt(TextureFormat.ASTC_5x5);
		CheckTextureForamt(TextureFormat.ASTC_6x6);
		CheckTextureForamt(TextureFormat.ASTC_8x8);
		CheckTextureForamt(TextureFormat.ASTC_10x10);
		CheckTextureForamt(TextureFormat.ASTC_12x12);
		CheckTextureForamt(TextureFormat.ETC2_RGB);
		CheckTextureForamt(TextureFormat.ETC2_RGBA1);
		CheckTextureForamt(TextureFormat.ETC2_RGBA8);
	}

	private void CheckTextureForamt(TextureFormat format)
	{
		_supportedTextureFormatDict[format] = SystemInfo.SupportsTextureFormat(format);
	}

	public string GetLog(string seperator)
	{
		if (_logCache != null)
		{
			return _logCache;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<TextureFormat, bool> item in _supportedTextureFormatDict)
		{
			if (item.Value)
			{
				stringBuilder.Append($"{seperator}{item.Key}");
			}
		}
		_logCache = stringBuilder.ToString();
		return _logCache;
	}
}
