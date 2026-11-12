using NVespa;
using UnityEngine;

namespace NDrawCall;

public class ShaderContainer : SingletonAuto<ShaderContainer>
{
	private struct Key
	{
		public string ShaderName;

		public UIDrawCall.Clipping Clipping;

		public int ClipCount;
	}

	public class ShaderCache
	{
		public Shader Shader;

		public bool IsLegacyShader;

		public bool TextureClip;
	}

	private IncreaseDic<Key, ShaderCache> _dic;

	public ShaderContainer()
	{
		SingletonAuto<EqualityComparerCache>.instance.Add((Key x, Key y) => x.ShaderName == y.ShaderName && x.Clipping == y.Clipping && x.ClipCount == y.ClipCount, (Key obj) => obj.ShaderName.GetHashCode() ^ obj.ClipCount ^ ((int)obj.Clipping * 77));
		_dic = IncreaseDic<Key, ShaderCache>.CreateWithComparer();
	}

	public ShaderCache GetShaderCache(string shaderName, UIDrawCall.Clipping clipping, int clipCount)
	{
		Key key = new Key
		{
			ShaderName = shaderName,
			Clipping = clipping,
			ClipCount = clipCount
		};
		if (_dic.TryGetValue(key, out var o_value))
		{
			return o_value;
		}
		o_value = CreateShaderCache(key);
		_dic.Add(key, o_value);
		return o_value;
	}

	private ShaderCache CreateShaderCache(Key key)
	{
		ShaderCache shaderCache = new ShaderCache();
		shaderCache.IsLegacyShader = false;
		shaderCache.TextureClip = false;
		string text = key.ShaderName.Replace("GUI/Text Shader", "Unlit/Text");
		if (text.Length > 2 && text[text.Length - 2] == ' ')
		{
			int num = text[text.Length - 1];
			if (num > 48 && num <= 57)
			{
				text = text.Substring(0, text.Length - 2);
			}
		}
		if (text.StartsWith("Hidden/"))
		{
			text = text.Substring(7);
		}
		text = text.Replace(" (SoftClip)", "");
		text = text.Replace(" (TextureClip)", "");
		if (key.Clipping == UIDrawCall.Clipping.TextureMask)
		{
			shaderCache.TextureClip = true;
			shaderCache.Shader = Shader.Find("Hidden/" + text + " (TextureClip)");
		}
		else if (key.ClipCount != 0)
		{
			shaderCache.Shader = Shader.Find("Hidden/" + text + " " + key.ClipCount);
			if (shaderCache.Shader == null)
			{
				shaderCache.Shader = Shader.Find(text + " " + key.ClipCount);
			}
			if (shaderCache.Shader == null && key.ClipCount == 1)
			{
				shaderCache.IsLegacyShader = true;
				shaderCache.Shader = Shader.Find(text + " (SoftClip)");
			}
		}
		else
		{
			shaderCache.Shader = Shader.Find(text);
		}
		if (shaderCache.Shader == null)
		{
			shaderCache.Shader = Shader.Find("Unlit/Transparent Colored");
		}
		return shaderCache;
	}
}
