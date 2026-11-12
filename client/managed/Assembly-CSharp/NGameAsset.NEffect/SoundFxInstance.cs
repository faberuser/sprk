using NGameAsset.NSound;
using NVespa;

namespace NGameAsset.NEffect;

public class SoundFxInstance
{
	public enum EPlayState
	{
		Ready,
		Play,
		End
	}

	private EPlayState _playState;

	private ISound _sound;

	public SoundFxData Data { get; private set; }

	public EffectInstance Owner { get; private set; }

	public bool IsEnd => _playState == EPlayState.End;

	public float ElapsedTime { get; private set; }

	public SoundFxInstance(EffectInstance owner)
	{
		Owner = owner;
	}

	public bool Open(SoundFxData data)
	{
		Data = data;
		ElapsedTime = 0f;
		_playState = EPlayState.Ready;
		return true;
	}

	public void Close()
	{
		StopSound();
	}

	public bool IsEmpty()
	{
		return _sound == null;
	}

	public void Release()
	{
	}

	private void PlaySound()
	{
		SoundStock stock = new SoundStock
		{
			Code = Data.SoundCode,
			LocalTimer = Owner.LocalTimer
		};
		if (Data.EnableEndTime)
		{
			_sound = SingletonAuto<SoundContainer>.instance.CreateSound(stock);
			_playState = EPlayState.Play;
		}
		else
		{
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 999f);
			_playState = EPlayState.End;
		}
	}

	private void StopSound()
	{
		_playState = EPlayState.End;
		if (_sound != null)
		{
			SoundContainer.ReleaseSound(_sound);
			_sound = null;
		}
	}

	public void Update(float deltaTime)
	{
		if (_playState == EPlayState.End)
		{
			return;
		}
		ElapsedTime += deltaTime;
		if (!(ElapsedTime < Data.BeginTime))
		{
			if (_playState == EPlayState.Ready)
			{
				PlaySound();
			}
			else if (_playState == EPlayState.Play && Data.EnableEndTime && ElapsedTime > Data.EndTime)
			{
				StopSound();
			}
			else if (_sound != null)
			{
				_sound.Update();
			}
		}
	}
}
