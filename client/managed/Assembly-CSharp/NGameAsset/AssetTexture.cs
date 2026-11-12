using NGameAsset.NAssetTexture;
using NGameAsset.NNguiAtlas;
using NVespa;
using UnityEngine;

namespace NGameAsset;

public class AssetTexture : IAsyncLoadBase
{
	public enum ESrcType
	{
		NguiAtlas,
		PlainAsset,
		Url
	}

	public class TextureInfo
	{
		public ESrcType SrcType;

		public Texture2D Tex;

		public Material Mtrl;

		public int Width;

		public int Height;

		public Rect UvRect;

		public Rect CalcMinMaxUv(float xMin, float yMin, float xMax, float yMax)
		{
			float t = Mathf.Clamp01(xMin / (float)Width);
			float t2 = Mathf.Clamp01(xMax / (float)Width);
			float t3 = Mathf.Clamp01(yMin / (float)Height);
			float t4 = Mathf.Clamp01(yMax / (float)Height);
			return Rect.MinMaxRect(Mathf.Lerp(UvRect.xMin, UvRect.xMax, t), Mathf.Lerp(UvRect.yMin, UvRect.yMax, t3), Mathf.Lerp(UvRect.xMin, UvRect.xMax, t2), Mathf.Lerp(UvRect.yMin, UvRect.yMax, t4));
		}
	}

	public interface ILoad
	{
		TextureInfo Value { get; }

		void Close();

		void CloseAndRelease();
	}

	public string Path = string.Empty;

	private ILoad _load;

	public bool IsDone { get; private set; }

	string IAsyncLoadBase.KeyDesc => Path;

	bool IAsyncLoadBase.IsDone
	{
		get
		{
			if (_load == null)
			{
				return true;
			}
			return IsDone;
		}
	}

	bool IAsyncLoadBase.IsValid
	{
		get
		{
			if (_load != null)
			{
				return _load.Value != null;
			}
			return false;
		}
	}

	string IAsyncLoadBase.LoadMessage => Path;

	public void Open(string path)
	{
		Path = path;
		IsDone = false;
		AtlasDataContainer.AtlasLink atlasLink = SingletonAuto<AtlasDataContainer>.instance.GetAtlasLink(path);
		if (atlasLink != null)
		{
			TextureFromNguiAtlas textureFromNguiAtlas = new TextureFromNguiAtlas();
			textureFromNguiAtlas.Open(atlasLink, OnComplete);
			_load = textureFromNguiAtlas;
		}
		else
		{
			OnComplete();
		}
	}

	public void OpenUrl(string url)
	{
		Path = url;
		IsDone = false;
		TextureFromPlainAsset textureFromPlainAsset = new TextureFromPlainAsset();
		textureFromPlainAsset.OpenUrl(url, OnComplete);
		_load = textureFromPlainAsset;
	}

	public void Close()
	{
		if (_load != null)
		{
			_load.Close();
			_load = null;
		}
		IsDone = true;
	}

	public void CloseAndRelease()
	{
		if (_load != null)
		{
			_load.CloseAndRelease();
			_load = null;
		}
		IsDone = true;
	}

	public TextureInfo GetTextureInfo()
	{
		if (_load == null)
		{
			return null;
		}
		return _load.Value;
	}

	private void OnComplete()
	{
		IsDone = true;
	}
}
