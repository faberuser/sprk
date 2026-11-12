using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGameAsset.NSound;

public class BgmPlayer : SingletonAuto<BgmPlayer>
{
	private BgmInstance _currentBgm;

	private List<BgmInstance> _fadeoutList = new List<BgmInstance>();

	public void Play(AudioClip clip, float volume, IAsyncLoad<AudioClip> asyncLoad)
	{
		if (!VolatileOptionData.Sound_EnableBgm || !SystemOptionData.Sound_EnableBgm)
		{
			return;
		}
		if (_currentBgm != null)
		{
			string text = ((_currentBgm.AsyncLoad == null) ? string.Empty : _currentBgm.AsyncLoad.KeyDesc);
			string text2 = ((asyncLoad == null) ? string.Empty : asyncLoad.KeyDesc);
			if (_currentBgm.Clip == clip && text == text2)
			{
				if (!Mathf.Approximately(_currentBgm.Volume, volume))
				{
					_currentBgm.Volume = volume;
				}
				return;
			}
			_currentBgm.Stop();
			_fadeoutList.Add(_currentBgm);
		}
		_currentBgm = new BgmInstance();
		_currentBgm.Open(clip, volume, asyncLoad);
	}

	public void ChainUpdate()
	{
		if ((!SystemOptionData.Sound_EnableBgm || !VolatileOptionData.Sound_EnableBgm) && _currentBgm != null)
		{
			_currentBgm.Stop();
			_fadeoutList.Add(_currentBgm);
			_currentBgm = null;
		}
		for (int i = 0; i < _fadeoutList.Count; i++)
		{
			BgmInstance bgmInstance = _fadeoutList[i];
			bgmInstance.Update();
			if (bgmInstance.Status == BgmInstance.EStatus.Stop)
			{
				bgmInstance.Close();
				_fadeoutList.RemoveAt(i);
				i--;
			}
			else if (bgmInstance.Status != BgmInstance.EStatus.FadeOut)
			{
				bgmInstance.Stop();
			}
		}
		if (_currentBgm != null)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(_fadeoutList) && _currentBgm.Status == BgmInstance.EStatus.Ready)
			{
				_currentBgm.Play();
			}
			_currentBgm.Update();
		}
	}
}
