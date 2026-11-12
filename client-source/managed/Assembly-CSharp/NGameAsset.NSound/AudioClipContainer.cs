using System;
using NVespa;
using UnityEngine;

namespace NGameAsset.NSound;

public class AudioClipContainer : SingletonAuto<AudioClipContainer>
{
	private IncreaseDic<string, AudioClipLoad> _dic = new IncreaseDic<string, AudioClipLoad>(StringComparer.OrdinalIgnoreCase);

	public AudioClipLoad LockAudioClip(string audioPath, bool isVoice)
	{
		if (String_Extension_Method.IsNullOrEmpty(audioPath))
		{
			return null;
		}
		AudioClipLoad audioClipLoad = _dic.GETVALUE(audioPath, null);
		if (audioClipLoad == null)
		{
			audioClipLoad = new AudioClipLoad();
			audioClipLoad.Open(audioPath, isVoice);
			_dic.Add(audioPath, audioClipLoad);
		}
		audioClipLoad.Lock.Increase();
		return audioClipLoad;
	}

	public AudioClipLoad LockAudioClip(IAsyncLoad<AudioClip> asyncLoad)
	{
		if (!(asyncLoad is AudioClipLoad audioClipLoad))
		{
			return null;
		}
		audioClipLoad.Lock.Increase();
		return audioClipLoad;
	}

	public static void ReleaseAudioClip(IAsyncLoad<AudioClip> asyncLoad)
	{
		if (SingletonAuto<AudioClipContainer>.isValidInstance)
		{
			SingletonAuto<AudioClipContainer>.instance.ReleaseInternal(asyncLoad);
		}
	}

	private void ReleaseInternal(IAsyncLoad<AudioClip> asyncLoad)
	{
		if (asyncLoad != null && asyncLoad is AudioClipLoad audioClipLoad)
		{
			audioClipLoad.Lock.Decrease(null);
		}
	}

	public void DestroyUnusedLastOne()
	{
		int unscaledTick = UnitySystemTimer.unscaledTick;
		int index = -1;
		int num = int.MaxValue;
		for (int i = 0; i < _dic.Count; i++)
		{
			AudioClipLoad valueByIndex = _dic.GetValueByIndex(i);
			if (valueByIndex.IsDone && valueByIndex.Lock.Count <= 0 && unscaledTick - valueByIndex.Lock.UnlockTick >= 10000 && valueByIndex.Lock.UnlockTick < num)
			{
				num = valueByIndex.Lock.UnlockTick;
				index = i;
			}
		}
		if (_dic.IsValidIndex(index))
		{
			_dic.GetValueByIndex(index).Close();
			_dic.RemoveByIndex(index);
		}
	}

	public void DestroyUnusedAll()
	{
		for (int i = 0; i < _dic.Count; i++)
		{
			AudioClipLoad valueByIndex = _dic.GetValueByIndex(i);
			if (valueByIndex.IsDone && valueByIndex.Lock.Count <= 0)
			{
				valueByIndex.Close();
				_dic.RemoveByIndex(i);
				i--;
			}
		}
	}
}
