using System.Collections.Generic;
using NGameAsset;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;
using UnityEngine.Audio;

namespace NVespa;

public class AudioManager : SingletonUpdater<AudioManager>
{
	internal enum EAudioType
	{
		Open,
		Closed,
		Auto
	}

	public class AudioInstance
	{
		internal EAudioType _audioType;

		internal EAudioMixerFlag _audioMixerFlag;

		internal AudioSource _source;

		internal float _volume;

		internal WeakRef<IAudioVolumeListener> _volumeListener;

		public float Volume
		{
			get
			{
				return _volume;
			}
			set
			{
				_volume = value;
				if (!(_source == null))
				{
					float num = _volume / CurrentMaxVolume;
					num = (((_audioMixerFlag & EAudioMixerFlag.Bgm) == EAudioMixerFlag.Bgm) ? (num * System_BgmVolume) : (((_audioMixerFlag & EAudioMixerFlag.Voice) != EAudioMixerFlag.Voice) ? (num * System_GameSoundVolume) : (num * System_CharacterVoiceVolume)));
					if (CheckConvertVolume())
					{
						num = ConvertListenerVolume(num);
					}
					_source.volume = num;
				}
			}
		}

		public bool IsPlaying
		{
			get
			{
				if (_source == null)
				{
					return false;
				}
				return _source.isPlaying;
			}
		}

		internal AudioInstance(AudioSource source)
		{
			_source = source;
		}

		public void SetVolumeListener(WeakRef<IAudioVolumeListener> volumeListener)
		{
			_volumeListener = volumeListener;
		}

		private bool CheckConvertVolume()
		{
			if (_volumeListener == null)
			{
				return false;
			}
			if (_volumeListener.Value == null)
			{
				return false;
			}
			return _volumeListener.Value.CheckConvertVolume(_audioMixerFlag);
		}

		private float ConvertListenerVolume(float volume)
		{
			if (_volumeListener == null)
			{
				return volume;
			}
			if (_volumeListener.Value == null)
			{
				return volume;
			}
			return _volumeListener.Value.ConvertVolume(volume);
		}

		public void Play(bool isLoop)
		{
			_source.loop = isLoop;
			_source.Play();
		}
	}

	public class SkillAudio
	{
		public AudioInstance AudioInstance;

		public float DefaultVolume;

		public float FadeOutElaspedTime;
	}

	public const string PrefabPath = "Resources/CommonPrefab/AudioSource2D";

	public static PrefabAuto<AudioSource> PrefabAuto = new PrefabAuto<AudioSource>("Resources/CommonPrefab/AudioSource2D", isGlobal: true, EPrefabType.Sound);

	public const float BaseMaxVolume = 2f;

	public static float CurrentMaxVolume = 2f;

	public static float TargetMaxVolume = 2f;

	public static float System_BgmVolume;

	public static float System_GameSoundVolume;

	public static float System_CharacterVoiceVolume;

	public static bool MuteEnemyVoice = false;

	private static string SkillSoundMaxCountStr = GameSharedSetting.data.Sound.SkillSoundMaxCount.ToString();

	private static string SkillSoundDecreaseVolumeRateStr = GameSharedSetting.data.Sound.SkillSoundDecreaseVolumeRate.ToString();

	private static string SkillSoundFadeoutTimeStr = GameSharedSetting.data.Sound.SkillSoundFadeOutTime.ToString();

	private Transform _audioInstanceRoot;

	private AudioMixerGroup _audioMixerDarkness;

	private List<AudioInstance> _audioList = new List<AudioInstance>();

	private Dictionary<IAsyncLoadBase, float> _playAudioTimeDic = new Dictionary<IAsyncLoadBase, float>();

	private WeakRef<IAudioVolumeListener> _audioVolumeListener = new WeakRef<IAudioVolumeListener>();

	public List<SkillAudio> _skillVoiceAudioList = new List<SkillAudio>();

	public List<SkillAudio> _skillVoiceFadeoutAudioList = new List<SkillAudio>();

	public AudioManager()
	{
		System_BgmVolume = SystemOptionData.Sound_BgmVolume;
		System_GameSoundVolume = SystemOptionData.Sound_GameSoundVolume;
		System_CharacterVoiceVolume = SystemOptionData.Sound_CharacterVoiceVolume;
		AudioMixer audioMixer = VespaResource.Load<AudioMixer>("Resources/CommonPrefab/Darkness");
		if (audioMixer != null)
		{
			AudioMixerGroup[] array = audioMixer.FindMatchingGroups("Master");
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(array))
			{
				_audioMixerDarkness = array[0];
			}
		}
		GameObject singletonComponent = GlobalAutoManager.SingletonComponent;
		if (singletonComponent != null)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = "AudioManagerGo";
			gameObject.transform.parent = singletonComponent.transform;
			_audioInstanceRoot = gameObject.transform;
		}
		GlobalAutoBehaviour<GuiManager>.instance.Add("Skill Voice StateViewer", this, delegate
		{
			using (new BlockManager().Vertical())
			{
				MuteEnemyVoice = GUILayout.Toggle(MuteEnemyVoice, "Mute Enemy Voice");
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("SkillSound Max Count");
					SkillSoundMaxCountStr = GUILayout.TextField(SkillSoundMaxCountStr);
					if (GUILayout.Button("Apply", GUILayout.Width(50f)))
					{
						int result = 0;
						if (int.TryParse(SkillSoundMaxCountStr, out result))
						{
							GameSharedSetting.data.Sound.SkillSoundMaxCount = result;
						}
					}
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("SkillSound Decrease Volume Rate");
					SkillSoundDecreaseVolumeRateStr = GUILayout.TextField(SkillSoundDecreaseVolumeRateStr);
					if (GUILayout.Button("Apply", GUILayout.Width(50f)))
					{
						float result2 = 0f;
						if (float.TryParse(SkillSoundDecreaseVolumeRateStr, out result2))
						{
							GameSharedSetting.data.Sound.SkillSoundDecreaseVolumeRate = result2;
						}
					}
				}
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("SkillSound FadeOut Time");
					SkillSoundFadeoutTimeStr = GUILayout.TextField(SkillSoundFadeoutTimeStr);
					if (GUILayout.Button("Apply", GUILayout.Width(50f)))
					{
						float result3 = 0f;
						if (float.TryParse(SkillSoundFadeoutTimeStr, out result3))
						{
							GameSharedSetting.data.Sound.SkillSoundFadeOutTime = result3;
						}
					}
				}
				GUILayout.Space(20f);
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("[Skill Voice Play AudioList]");
				}
				foreach (SkillAudio skillVoiceAudio in _skillVoiceAudioList)
				{
					AudioInstance audioInstance = skillVoiceAudio.AudioInstance;
					if (audioInstance != null && !(audioInstance._source == null) && !(audioInstance._source.clip == null))
					{
						using (new BlockManager().Horizontal())
						{
							GUILayout.Label($"Name = {audioInstance._source.clip.name}      Volume = {audioInstance.Volume}");
						}
					}
				}
				GUILayout.Space(20f);
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("[Skill Voice FadeOut AudioList]");
				}
				foreach (SkillAudio skillVoiceFadeoutAudio in _skillVoiceFadeoutAudioList)
				{
					AudioInstance audioInstance2 = skillVoiceFadeoutAudio.AudioInstance;
					if (audioInstance2 != null && !(audioInstance2._source == null) && !(audioInstance2._source.clip == null))
					{
						using (new BlockManager().Horizontal())
						{
							GUILayout.Label($"Name = {audioInstance2._source.clip.name}      Volume = {audioInstance2.Volume}");
						}
					}
				}
			}
		});
	}

	public void OnSceneChanged()
	{
		_playAudioTimeDic.Clear();
	}

	public bool IsDuplicateAudio(IAsyncLoadBase asyncLoad, bool add)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (_playAudioTimeDic.TryGetValue(asyncLoad, out var value))
		{
			if (realtimeSinceStartup - value < GameSharedSetting.data.Sound.TimeForOverlapDetect)
			{
				return true;
			}
			if (add)
			{
				_playAudioTimeDic[asyncLoad] = realtimeSinceStartup;
			}
		}
		else if (add)
		{
			_playAudioTimeDic.Add(asyncLoad, realtimeSinceStartup);
		}
		return false;
	}

	public AudioInstance CreateInstance(AudioClip clip, EAudioMixerFlag mixerFlag, float skillAudioStartVolume = 0f)
	{
		AudioInstance audioInstance = CreateInstanceInternal(clip, EAudioType.Open, mixerFlag);
		if (audioInstance == null)
		{
			return null;
		}
		EAudioMixerFlag eAudioMixerFlag = EAudioMixerFlag.Voice | EAudioMixerFlag.Player | EAudioMixerFlag.Skill;
		if ((mixerFlag & eAudioMixerFlag) == eAudioMixerFlag)
		{
			SkillAudio skillAudio = new SkillAudio();
			skillAudio.AudioInstance = audioInstance;
			skillAudio.FadeOutElaspedTime = 0f;
			skillAudio.DefaultVolume = skillAudioStartVolume;
			_skillVoiceAudioList.Add(skillAudio);
			int count = _skillVoiceAudioList.Count;
			int skillSoundMaxCount = GameSharedSetting.data.Sound.SkillSoundMaxCount;
			if (count > skillSoundMaxCount)
			{
				SkillAudio skillAudio2 = _skillVoiceAudioList[0];
				skillAudio2.DefaultVolume = skillAudio2.AudioInstance.Volume;
				_skillVoiceFadeoutAudioList.Add(skillAudio2);
				_skillVoiceAudioList.RemoveAt(0);
			}
			if (count > 1)
			{
				float skillSoundDecreaseVolumeRate = GameSharedSetting.data.Sound.SkillSoundDecreaseVolumeRate;
				int num = 1;
				for (int num2 = _skillVoiceAudioList.Count - 2; num2 >= 0; num2--)
				{
					SkillAudio skillAudio3 = _skillVoiceAudioList[num2];
					float num3 = skillAudio3.DefaultVolume * Mathf.Pow(skillSoundDecreaseVolumeRate, num);
					if (num3 < skillAudio3.AudioInstance.Volume)
					{
						skillAudio3.AudioInstance.Volume = num3;
					}
					num++;
				}
			}
		}
		_audioList.Add(audioInstance);
		return audioInstance;
	}

	public static void ReleaseInstance(AudioInstance audioInstance)
	{
		if (SingletonUpdater<AudioManager>.isValidInstance && audioInstance != null)
		{
			SingletonUpdater<AudioManager>.instance.ReleaseInstanceInternal(audioInstance);
		}
	}

	public void Play(AudioClip clip, float volume, EAudioMixerFlag mixerFlag)
	{
		AudioInstance audioInstance = CreateInstanceInternal(clip, EAudioType.Auto, mixerFlag);
		if (audioInstance != null)
		{
			audioInstance.Volume = volume;
			audioInstance.Play(isLoop: false);
			_audioList.Add(audioInstance);
		}
	}

	private AudioInstance CreateInstanceInternal(AudioClip clip, EAudioType audioType, EAudioMixerFlag mixerFlag)
	{
		if (clip == null)
		{
			return null;
		}
		AudioSource audioSource = PrefabAuto.Factory.Create(_audioInstanceRoot);
		if (audioSource == null)
		{
			return null;
		}
		audioSource.clip = clip;
		audioSource.timeSamples = 0;
		audioSource.loop = false;
		audioSource.outputAudioMixerGroup = GetAudioMixer(mixerFlag);
		AudioInstance audioInstance = new AudioInstance(audioSource);
		audioInstance._audioType = audioType;
		audioInstance._audioMixerFlag = mixerFlag;
		audioInstance.SetVolumeListener(_audioVolumeListener);
		return audioInstance;
	}

	private AudioMixerGroup GetAudioMixer(EAudioMixerFlag mixerFlag)
	{
		if (!SystemOptionData.Sound_EnableAudioMixer)
		{
			return null;
		}
		EAudioMixerFlag eAudioMixerFlag = EAudioMixerFlag.DarknessCreature | EAudioMixerFlag.Voice;
		if ((mixerFlag & eAudioMixerFlag) == eAudioMixerFlag)
		{
			return _audioMixerDarkness;
		}
		return null;
	}

	private void ReleaseInstanceInternal(AudioInstance audioInstance)
	{
		AudioSource source = audioInstance._source;
		if (source != null)
		{
			source.Stop();
			source.clip = null;
			source.outputAudioMixerGroup = null;
			FactoryBase.ReleaseComponent(source);
			audioInstance._source = null;
		}
		audioInstance._audioType = EAudioType.Closed;
	}

	public override void ChainUpdate()
	{
		float num = 2f;
		for (int num2 = _audioList.Count - 1; num2 >= 0; num2--)
		{
			AudioInstance audioInstance = _audioList[num2];
			if (audioInstance._audioType == EAudioType.Auto && !audioInstance.IsPlaying)
			{
				ReleaseInstanceInternal(audioInstance);
			}
			if (audioInstance._audioType == EAudioType.Closed)
			{
				_audioList.RemoveAt(num2);
			}
			else
			{
				num = Mathf.Max(num, audioInstance.Volume);
			}
		}
		for (int i = 0; i < _skillVoiceAudioList.Count; i++)
		{
			SkillAudio skillAudio = _skillVoiceAudioList[i];
			if (skillAudio.AudioInstance._audioType == EAudioType.Closed)
			{
				skillAudio.AudioInstance = null;
				_skillVoiceAudioList.RemoveAt(i);
			}
		}
		for (int j = 0; j < _skillVoiceFadeoutAudioList.Count; j++)
		{
			SkillAudio skillAudio2 = _skillVoiceFadeoutAudioList[j];
			AudioInstance audioInstance2 = skillAudio2.AudioInstance;
			if (audioInstance2._audioType == EAudioType.Closed)
			{
				skillAudio2.AudioInstance = null;
				_skillVoiceFadeoutAudioList.RemoveAt(j);
				continue;
			}
			if (audioInstance2.Volume <= 0f)
			{
				skillAudio2.AudioInstance = null;
				_skillVoiceFadeoutAudioList.RemoveAt(j);
				continue;
			}
			skillAudio2.FadeOutElaspedTime += UnitySystemTimer.deltaTime;
			float num3 = skillAudio2.FadeOutElaspedTime / GameSharedSetting.data.Sound.SkillSoundFadeOutTime;
			float num4 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(GameSharedSetting.data.Sound.SkillSoundFadeOutCurve, num3, 1f - num3);
			float num5 = skillAudio2.DefaultVolume * num4;
			if (num5 <= 0f)
			{
				num5 = 0f;
			}
			audioInstance2.Volume = num5;
		}
		bool flag = false;
		TargetMaxVolume = num;
		if (!Mathf.Approximately(TargetMaxVolume, CurrentMaxVolume))
		{
			float maxDelta = Mathf.Max(1f, Mathf.Abs(CurrentMaxVolume - TargetMaxVolume)) * Time.unscaledDeltaTime / GameSharedSetting.data.Sound.MaxVolumeBlendTime;
			CurrentMaxVolume = Mathf.MoveTowards(CurrentMaxVolume, TargetMaxVolume, maxDelta);
			flag = true;
		}
		if (!Mathf.Approximately(System_BgmVolume, SystemOptionData.Sound_BgmVolume) || !Mathf.Approximately(System_GameSoundVolume, SystemOptionData.Sound_GameSoundVolume) || !Mathf.Approximately(System_CharacterVoiceVolume, SystemOptionData.Sound_CharacterVoiceVolume))
		{
			System_BgmVolume = SystemOptionData.Sound_BgmVolume;
			System_GameSoundVolume = SystemOptionData.Sound_GameSoundVolume;
			System_CharacterVoiceVolume = SystemOptionData.Sound_CharacterVoiceVolume;
			flag = true;
		}
		if (flag)
		{
			for (int k = 0; k < _audioList.Count; k++)
			{
				_audioList[k].Volume = _audioList[k].Volume;
			}
		}
	}

	public void SetAudioVolumeListener(IAudioVolumeListener listener)
	{
		if (_audioVolumeListener != null && _audioVolumeListener.Value != null)
		{
			Debug.Log("AudioVolumeListener Release Old : " + _audioVolumeListener.Value.GetType().FullName);
		}
		_audioVolumeListener.Value = listener;
		SetAudioVolumeListener(_audioVolumeListener);
	}

	public void RelaseAudioVolumeListener(IAudioVolumeListener listener)
	{
		if (_audioVolumeListener.Value == listener)
		{
			_audioVolumeListener.Value = null;
			SetAudioVolumeListener(_audioVolumeListener);
		}
	}

	private void SetAudioVolumeListener(WeakRef<IAudioVolumeListener> listener)
	{
		for (int i = 0; i < _audioList.Count; i++)
		{
			if (_audioVolumeListener.Value != null)
			{
				_audioList[i].SetVolumeListener(listener);
				_audioList[i].Volume = _audioList[i].Volume;
			}
		}
	}
}
