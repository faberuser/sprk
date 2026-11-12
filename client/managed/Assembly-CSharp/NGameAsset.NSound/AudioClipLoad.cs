using NGameAsset.NAsyncLoad;
using NVespa;
using UnityEngine;

namespace NGameAsset.NSound;

public class AudioClipLoad : IAsyncLoad<AudioClip>, IAsyncLoadBase
{
	public bool IsVoice;

	public string AudioPath;

	public bool IsDone;

	public bool IsValid;

	public string LoadMessage;

	public LockCounter Lock = new LockCounter();

	private string _assetNameInBundle = string.Empty;

	private AssetDataAsync<AudioClip, string> _assetAsync;

	string IAsyncLoadBase.KeyDesc => AudioPath;

	bool IAsyncLoadBase.IsDone => IsDone;

	bool IAsyncLoadBase.IsValid => IsValid;

	string IAsyncLoadBase.LoadMessage => LoadMessage;

	AudioClip IAsyncLoad<AudioClip>.Value
	{
		get
		{
			if (_assetAsync == null)
			{
				return null;
			}
			return _assetAsync.Asset;
		}
	}

	public void Open(string audioPath, bool isVoice)
	{
		AudioPath = audioPath;
		IsVoice = isVoice;
		if (!DefineSound.GetBundlePath(audioPath, out var bundleFileName, out _assetNameInBundle))
		{
			SetInvalid("Invalid AudioPath: " + audioPath, null);
			return;
		}
		_assetAsync = new AssetDataAsync<AudioClip, string>();
		_assetAsync.Open(AudioPath, bundleFileName, _assetNameInBundle);
		if (_assetAsync.ISDONE())
		{
			OnCompleteAssetAsync();
			return;
		}
		AsyncManager.SingleEntry entry = new AsyncManager.SingleEntry
		{
			AsyncLoad = _assetAsync,
			OnComplete = OnCompleteAssetAsync
		};
		SingletonUpdater<AsyncManager>.instance.Add(entry);
	}

	public void Close()
	{
		SetInvalid("Closed", null);
		if (_assetAsync != null)
		{
			_assetAsync.Close();
			_assetAsync = null;
		}
	}

	private void SetInvalid(string message, IAsyncLoadBase childAsyncLoad)
	{
		IsDone = true;
		IsValid = false;
		LoadMessage = message;
		if (childAsyncLoad != null && !String_Extension_Method.IsNullOrEmpty(childAsyncLoad.LoadMessage))
		{
			LoadMessage += $"\n - {childAsyncLoad.LoadMessage}";
		}
	}

	private void OnCompleteAssetAsync()
	{
		if (!_assetAsync.ISVALID())
		{
			SetInvalid("Invalid AudioClip AssetAsync", _assetAsync);
			return;
		}
		IsDone = true;
		IsValid = true;
	}
}
