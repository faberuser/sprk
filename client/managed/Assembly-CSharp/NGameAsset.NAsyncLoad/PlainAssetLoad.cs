using NVespa;
using UnityEngine;

namespace NGameAsset.NAsyncLoad;

public class PlainAssetLoad : IPlainAssetLoad2, IAsyncLoadBase
{
	private WWW _www;

	private bool _isDone;

	private string _wwwText;

	private Texture2D _wwwTexture;

	private bool _closed;

	private LockCounter _lockCounter = new LockCounter();

	public string AssetPath { get; private set; }

	string IPlainAssetLoad2.Path => AssetPath;

	ILockCounter IPlainAssetLoad2.Lock => _lockCounter;

	WWW IPlainAssetLoad2.WWW => _www;

	Texture2D IPlainAssetLoad2.Texture
	{
		get
		{
			if (!this.ISVALID())
			{
				return null;
			}
			if (_wwwTexture == null)
			{
				_wwwTexture = _www.texture;
			}
			return _wwwTexture;
		}
	}

	string IPlainAssetLoad2.Text
	{
		get
		{
			if (!this.ISVALID())
			{
				return null;
			}
			if (_wwwText == null)
			{
				_wwwText = _www.text;
			}
			return _wwwText;
		}
	}

	string IAsyncLoadBase.KeyDesc => AssetPath;

	bool IAsyncLoadBase.IsDone
	{
		get
		{
			if (_closed)
			{
				return true;
			}
			return _isDone;
		}
	}

	bool IAsyncLoadBase.IsValid
	{
		get
		{
			if (_closed)
			{
				return false;
			}
			if (!this.ISDONE())
			{
				return false;
			}
			return String_Extension_Method.IsNullOrEmpty(_www.error);
		}
	}

	string IAsyncLoadBase.LoadMessage
	{
		get
		{
			if (_closed)
			{
				return "Closed";
			}
			if (!this.ISDONE())
			{
				return string.Empty;
			}
			return _www.error;
		}
	}

	public void Open(string assetPath)
	{
		AssetPath = assetPath;
		_closed = false;
	}

	void IPlainAssetLoad2.StartLoad()
	{
		if (!_closed && !String_Extension_Method.IsNullOrEmpty(AssetPath))
		{
			if (AssetPath.Contains("://"))
			{
				_www = new WWW(AssetPath);
			}
			else
			{
				_www = new WWW("file://" + AssetPath);
			}
			SingletonUpdater<AsyncManager>.instance.Add(_www, delegate
			{
				_isDone = true;
			});
		}
	}

	void IPlainAssetLoad2.Close()
	{
		_closed = true;
		_wwwTexture = null;
		_wwwText = null;
		if (_www != null)
		{
			_www.Dispose();
			_www = null;
		}
	}
}
