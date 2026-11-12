using System.Collections.Generic;
using NGameAsset.NSound;
using NVespa;
using UnityEngine;

namespace NGameAsset;

public class SoundContainer : SingletonAuto<SoundContainer>
{
	private class UniqueSound
	{
		public string Code;

		public ISound Sound;

		public List<WeakRef<object>> CallerList = new List<WeakRef<object>>();
	}

	private SoundInvalid _soundInvalid = new SoundInvalid();

	private HashSet<SoundInstance> _set = new HashSet<SoundInstance>();

	private IncreaseDic<string, UniqueSound> _uniqueDic = new IncreaseDic<string, UniqueSound>();

	public SoundContainer()
	{
		SingletonUpdater<AssetUpdateManager>.CreateInstance();
	}

	public void StopAll()
	{
		foreach (SoundInstance item in _set)
		{
			item.Close();
		}
		_set.Clear();
		for (int i = 0; i < _uniqueDic.Count; i++)
		{
			ReleaseSound(_uniqueDic.GetValueByIndex(i).Sound);
		}
		_uniqueDic.Clear();
	}

	public List<IAsyncLoad<AudioClip>> ReserveSound(string code, List<IAsyncLoad<AudioClip>> list = null)
	{
		if (String_Extension_Method.IsNullOrEmpty(code))
		{
			return list;
		}
		SoundData sound = SingletonAuto<SoundDataContainer>.instance.GetSound(code);
		if (sound == null)
		{
			return list;
		}
		List<IAsyncLoad<AudioClip>> list2 = list;
		for (int i = 0; i < sound.ClipList.Count; i++)
		{
			ClipData clipData = sound.ClipList[i];
			if (!clipData.IsEnableByOption())
			{
				continue;
			}
			AudioClipLoad audioClipLoad = SingletonAuto<AudioClipContainer>.instance.LockAudioClip(clipData.Name, clipData.IsVoice);
			if (audioClipLoad != null)
			{
				if (list2 == null)
				{
					list2 = ReservePoolUtil.CreateList<IAsyncLoad<AudioClip>>();
				}
				list2.Add(audioClipLoad);
			}
		}
		return list2;
	}

	private bool IsValidLanguage(SystemLanguage language)
	{
		if (language != SystemLanguage.English && language != SystemLanguage.Japanese)
		{
			return language == SystemLanguage.Korean;
		}
		return true;
	}

	public bool IsSoundExist(SoundStock stock)
	{
		if (SingletonAuto<SoundDataContainer>.instance.GetSound(stock.Code) == null)
		{
			return false;
		}
		return true;
	}

	public ISound CreateSoundExact(SoundStock stock)
	{
		if (!SystemOptionData.Sound_EnableGameSound && !SystemOptionData.Sound_EnableCharacterVoice)
		{
			return null;
		}
		SoundData soundData = null;
		if (IsValidLanguage(stock.Language))
		{
			soundData = SingletonAuto<SoundDataContainer>.instance.GetSound(stock.Language, stock.Code);
			if (soundData == null)
			{
				soundData = SingletonAuto<SoundDataContainer>.instance.GetSound(stock.Code);
			}
		}
		else
		{
			soundData = SingletonAuto<SoundDataContainer>.instance.GetSound(stock.Code);
		}
		if (soundData == null)
		{
			return null;
		}
		SoundInstance soundInstance = new SoundInstance();
		soundInstance.Open(soundData, stock);
		_set.Add(soundInstance);
		return soundInstance;
	}

	public ISound CreateSound(SoundStock stock)
	{
		ISound sound = CreateSoundExact(stock);
		if (sound == null)
		{
			return _soundInvalid;
		}
		return sound;
	}

	public bool IsEmptySoundList()
	{
		return SingletonUpdater<AssetUpdateManager>.instance.IsEmptySoundList();
	}

	public void PlayOnce(SoundStock stock, float detectZombieTime)
	{
		ISound sound = CreateSoundExact(stock);
		if (sound != null)
		{
			SingletonUpdater<AssetUpdateManager>.instance.Add(sound, detectZombieTime);
		}
	}

	public void PlayOnce(ISound sound, float detectZombieTime)
	{
		if (sound != null)
		{
			SingletonUpdater<AssetUpdateManager>.instance.Add(sound, detectZombieTime);
		}
	}

	public bool TryPlayOnce(SoundStock stock, float detectZombieTime)
	{
		ISound sound = CreateSoundExact(stock);
		if (sound == null)
		{
			return false;
		}
		SingletonUpdater<AssetUpdateManager>.instance.Add(sound, detectZombieTime);
		return true;
	}

	public void PlayUnique(string code, object caller)
	{
		UniqueSound uniqueSound = _uniqueDic.GETVALUE(code, null);
		if (uniqueSound == null)
		{
			ISound sound = CreateSoundExact(new SoundStock
			{
				Code = code
			});
			if (sound == null)
			{
				return;
			}
			uniqueSound = new UniqueSound();
			uniqueSound.Code = code;
			uniqueSound.Sound = sound;
			_uniqueDic.Add(code, uniqueSound);
		}
		WeakRef<object> weakRef = new WeakRef<object>();
		weakRef.Value = caller;
		uniqueSound.CallerList.Add(weakRef);
	}

	public static void StopUnique(string code, object caller)
	{
		if (!SingletonAuto<SoundContainer>.isValidInstance)
		{
			return;
		}
		UniqueSound vALUE = SingletonAuto<SoundContainer>.instance._uniqueDic.GETVALUE(code, null);
		if (vALUE == null)
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(vALUE.CallerList); i++)
		{
			object value = vALUE.CallerList[i].Value;
			if (value == null || value == caller)
			{
				vALUE.CallerList.RemoveAt(i);
				i--;
			}
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(vALUE.CallerList))
		{
			ReleaseSound(vALUE.Sound);
			vALUE.Sound = null;
			SingletonAuto<SoundContainer>.instance._uniqueDic.RemoveSlow(code);
		}
	}

	public static void ReleaseSound(ISound sound)
	{
		if (SingletonAuto<SoundContainer>.isValidInstance && sound is SoundInstance soundInstance)
		{
			SingletonAuto<SoundContainer>.instance._set.Remove(soundInstance);
			soundInstance.Close();
		}
	}
}
