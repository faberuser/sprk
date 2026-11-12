using NGameAsset.NAsyncLoad;
using NShader;
using NVespa;

namespace NGameAsset.NNguiAtlas;

public class AtlasLoad : INguiAtlas, IAsyncLoad<UIAtlas>, IAsyncLoadBase
{
	public AssetDataAsync<UIAtlas, AtlasData> AssetAsync;

	public string LoadMessage = string.Empty;

	public LockCounter Lock = new LockCounter();

	private bool _isClosed;

	private bool _checkAlphaTex;

	private bool _hasAlphaTexture;

	public string Code { get; private set; }

	AtlasData INguiAtlas.Data
	{
		get
		{
			if (AssetAsync != null)
			{
				return AssetAsync.Data;
			}
			return null;
		}
	}

	bool INguiAtlas.HasAlphaTexture
	{
		get
		{
			if (_checkAlphaTex)
			{
				return _hasAlphaTexture;
			}
			if (!AssetAsync.ISVALID())
			{
				return false;
			}
			UIAtlas asset = AssetAsync.Asset;
			if (asset == null)
			{
				_hasAlphaTexture = false;
			}
			else
			{
				_hasAlphaTexture = asset.spriteMaterial.HasProperty(NShader.Define.ALPHATEX_ID);
			}
			_checkAlphaTex = true;
			return _hasAlphaTexture;
		}
	}

	string IAsyncLoadBase.KeyDesc => Code;

	bool IAsyncLoadBase.IsDone => AssetAsync.ISDONE();

	bool IAsyncLoadBase.IsValid => AssetAsync.ISVALID();

	string IAsyncLoadBase.LoadMessage
	{
		get
		{
			if (AssetAsync != null)
			{
				return AssetAsync.LOADMESSAGE();
			}
			return LoadMessage;
		}
	}

	UIAtlas IAsyncLoad<UIAtlas>.Value
	{
		get
		{
			if (AssetAsync != null)
			{
				return AssetAsync.Asset;
			}
			return null;
		}
	}

	public bool Open(string code)
	{
		_isClosed = false;
		Code = code;
		AtlasData atlasData = SingletonAuto<AtlasDataContainer>.instance.Get(Code);
		if (atlasData == null)
		{
			AssetAsync = null;
			LoadMessage = $"Not Exist Code: {code}";
			return false;
		}
		AssetAsync = atlasData.CreateAssetAsync();
		_checkAlphaTex = false;
		_hasAlphaTexture = false;
		return true;
	}

	void INguiAtlas.Close(bool destroyImmediately)
	{
		_isClosed = true;
		if (AssetAsync != null)
		{
			AssetAsync.Close(destroyImmediately);
			AssetAsync = null;
		}
	}
}
