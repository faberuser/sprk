using NVespa;

namespace NGameAsset.NSound;

public class ClipInstance
{
	public enum EStatus
	{
		Ready,
		Play,
		End
	}

	private SoundInstance _owner;

	private int _elapsedTick;

	private AudioClipLoad _audioClipLoad;

	private AudioManager.AudioInstance _audioInstance;

	private int _addVolume;

	public EStatus Status { get; private set; }

	public ClipData Data { get; private set; }

	public int BeginTickMs { get; private set; }

	public ClipInstance(SoundInstance owner)
	{
		_owner = owner;
	}

	public void Open(ClipData data, int beginTickMs, int addVolume)
	{
		Data = data;
		BeginTickMs = beginTickMs;
		_elapsedTick = 0;
		_addVolume = addVolume;
		if (Data != null && !String_Extension_Method.IsNullOrEmpty(Data.Name))
		{
			_audioClipLoad = SingletonAuto<AudioClipContainer>.instance.LockAudioClip(Data.Name, Data.IsVoice);
			Status = EStatus.Ready;
		}
		else
		{
			Status = EStatus.End;
		}
	}

	public void Close()
	{
		if (_audioInstance != null)
		{
			AudioManager.ReleaseInstance(_audioInstance);
			_audioInstance = null;
		}
		if (_audioClipLoad != null)
		{
			AudioClipContainer.ReleaseAudioClip(_audioClipLoad);
			_audioClipLoad = null;
		}
		_addVolume = 0;
		Status = EStatus.End;
	}

	public void Update()
	{
		_elapsedTick += _owner.DeltaTick;
		if (Status == EStatus.Ready)
		{
			if (_elapsedTick < BeginTickMs || !_audioClipLoad.IsDone)
			{
				return;
			}
			if (!_audioClipLoad.IsValid)
			{
				Status = EStatus.End;
				return;
			}
			if (!SingletonUpdater<AudioManager>.instance.IsDuplicateAudio(_audioClipLoad, add: true))
			{
				EAudioMixerFlag eAudioMixerFlag = _owner.Stock.LocalTimer.AUDIOMIXERFLAG();
				eAudioMixerFlag |= _owner.Stock.MixerFlag;
				if (_audioClipLoad.IsVoice)
				{
					eAudioMixerFlag |= EAudioMixerFlag.Voice;
				}
				float skillAudioStartVolume = (float)(Data.Volume + _addVolume) * 0.001f;
				_audioInstance = SingletonUpdater<AudioManager>.instance.CreateInstance(_audioClipLoad.VALUE(), eAudioMixerFlag, skillAudioStartVolume);
			}
			if (_audioInstance != null)
			{
				_audioInstance.Volume = (float)(Data.Volume + _addVolume) * 0.001f;
				if (AudioManager.MuteEnemyVoice)
				{
					EAudioMixerFlag eAudioMixerFlag2 = _owner.Stock.LocalTimer.AUDIOMIXERFLAG();
					eAudioMixerFlag2 |= _owner.Stock.MixerFlag;
					if (_audioClipLoad.IsVoice)
					{
						eAudioMixerFlag2 |= EAudioMixerFlag.Voice;
					}
					if ((eAudioMixerFlag2 & EAudioMixerFlag.Player) != EAudioMixerFlag.Player && (eAudioMixerFlag2 & EAudioMixerFlag.Voice) == EAudioMixerFlag.Voice)
					{
						_audioInstance.Volume = 0f;
					}
				}
				_audioInstance.Play(Data.PlayType == ClipData.EPlayType.Loop);
				Status = EStatus.Play;
			}
			else
			{
				Status = EStatus.End;
			}
		}
		else if (Status == EStatus.Play)
		{
			if (Data.PlayType == ClipData.EPlayType.Once && _audioInstance != null && !_audioInstance.IsPlaying)
			{
				AudioManager.ReleaseInstance(_audioInstance);
				_audioInstance = null;
				Status = EStatus.End;
			}
		}
		else if (Status != EStatus.End)
		{
			Status = EStatus.End;
		}
	}
}
