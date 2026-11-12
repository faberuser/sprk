using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NHelper.NPet;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPet;

public class PetFeedComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface IListener
	{
		void OnClickPetFeed(PetInfo petInfo, long amount, Action<bool> callBack);

		void UpdatePetFeedAmountPopup();

		void SetPetFeedAmount(long usedAmount);
	}

	private enum EStatusIcon
	{
		Food,
		Sleep,
		Explore
	}

	private enum EGaugeAni
	{
		IDLE,
		RUN
	}

	public static string PrefabPath = "Prefab2/Window/Pet/PetFeedComponent";

	public static PrefabAuto<PetFeedComponent> PrefabAuto = new PrefabAuto<PetFeedComponent>(PrefabPath);

	[Header("[Pet Portrait]")]
	public Transform PetPortraitRoot;

	[Header("[Pet Basic Info]")]
	public UILabel PetGradeLabel;

	public UILabel PetNameLabel;

	[Header("[Pet Status Icon]")]
	public UISpriteSelector PetStatusIconSelector;

	[Header("[Pet Feed Gauge]")]
	public UIClipSprite GaugeSprite;

	public UIClipSprite FullGaugeSprite;

	public UILabel FeedPercentLabel;

	public UILabel PetRemainTimeLabel;

	public ParticleSystem FeedGageParticle;

	public ParticleSystem FeedMaxParticle;

	public float FeedMaxParticleLifeTime = 4f;

	public UIGame3DWidget GaugeRootEffect;

	[Header("[Button Group]")]
	public UIButton FeedMaxButton;

	public UIButton FeedOnceButton;

	public UIButton DisableFeedButton;

	[Header("[Label Group]")]
	public UILabel DisableFeedButtonLabel;

	[Header("[Button Group]")]
	public float GaugeAniTime;

	private CreatureData _creatureData;

	private IListener _listener;

	private EGaugeAni _gaugeAni;

	private bool _isPress;

	private PetPortraitComponent _petPortraitComponent;

	private Coroutine _runtimeCo;

	private long _currFullPoint;

	private long _nextFullPoint;

	private long _feedAmount;

	private int _maxFullPoint;

	private long _usedCount;

	private int _onceFeedPoint;

	private int _sleepRemainTimeSec;

	private float _lastCheckedRemainTime;

	private float _lastCheckedFeedEffectTime = -1f;

	public PetInfo _petInfo { get; private set; }

	private long _remainFullPoint
	{
		get
		{
			int maxFullPoint = _maxFullPoint;
			long currFullPoint = _currFullPoint;
			int num = maxFullPoint;
			return checked(num - currFullPoint);
		}
	}

	public void Open(PetInfo petInfo, IListener listener)
	{
		_listener = listener;
		_petInfo = petInfo;
		_creatureData = _petInfo.CreatureData;
		_isPress = false;
		int fullPoint = _petInfo.FullPoint;
		_currFullPoint = fullPoint;
		_maxFullPoint = _petInfo.MaxFood;
		_onceFeedPoint = GlobalAutoBehaviour<AccountManager>.instance.PetManager.PetFoodValue;
		_feedAmount = GlobalAutoBehaviour<AccountManager>.instance.PetManager.RemainPetFoodCount;
		Init();
	}

	private void Init()
	{
		_petPortraitComponent = PetPortraitComponent.SmallPrefabAuto.Factory.Create(PetPortraitRoot);
		_petPortraitComponent.Open(_petInfo, null);
		_petPortraitComponent.transform.localPosition = Vector3.zero;
		UIHelper.SetLabelText(PetGradeLabel, _petInfo.Grade);
		UIHelper.SetLabelText(PetNameLabel, _creatureData.Name);
		SetGauge(_currFullPoint);
		UIHelper.EnableGameObject(FeedGageParticle, enable: false);
		UIHelper.EnableGameObject(FeedMaxParticle, enable: false);
		_lastCheckedFeedEffectTime = -1f;
		_lastCheckedRemainTime = Time.time;
		UpdatePetState();
		UpdateButtonState();
	}

	public void RefreshPetInfo(long feedAmount)
	{
		PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(_petInfo.PetIndex);
		if (petInfo != null)
		{
			_petInfo = petInfo;
			int fullPoint = _petInfo.FullPoint;
			_currFullPoint = fullPoint;
			_feedAmount = feedAmount;
		}
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		_listener = null;
		_isPress = false;
		FactoryBase.SafeReleaseComponent(ref _petPortraitComponent);
		if (_gaugeAni == EGaugeAni.RUN && _runtimeCo != null)
		{
			VespaCoroutine.Stop(_runtimeCo);
		}
	}

	private void Update()
	{
		UpdateComp();
		if (Time.time - _lastCheckedRemainTime >= 1f)
		{
			UpdatePetState();
			_lastCheckedRemainTime = Time.time;
		}
		UpdateFeedMaxEffectState();
	}

	private void UpdateFeedMaxEffectState()
	{
		if (_lastCheckedFeedEffectTime != -1f && Time.time - _lastCheckedFeedEffectTime >= FeedMaxParticleLifeTime)
		{
			_lastCheckedFeedEffectTime = -1f;
			if (UIHelper.IsEnabledGameObjet(FeedMaxParticle))
			{
				FeedMaxParticle.Stop();
			}
		}
	}

	private void UpdateComp()
	{
		if (_isPress)
		{
			if (_usedCount >= _feedAmount || _usedCount >= _remainFullPoint)
			{
				OnReleaseFeedButton();
				return;
			}
			int num = 1;
			StartGaugeAnimation(num);
		}
	}

	public void UpdatePetState()
	{
		int num = 0;
		TimeSpan? petGetUpRemainTime = PetHelper.GetPetGetUpRemainTime(_petInfo);
		if (petGetUpRemainTime.HasValue)
		{
			num = (int)petGetUpRemainTime.Value.TotalSeconds;
		}
		if (!PetHelper.CanExplorePetStatus(_petInfo))
		{
			UIHelper.EnableGameObject(FeedPercentLabel, enable: false);
			UIHelper.EnableGameObject(PetRemainTimeLabel, enable: true);
			if (num > 0)
			{
				_sleepRemainTimeSec = num;
				UILabel petRemainTimeLabel = PetRemainTimeLabel;
				int sleepRemainTimeSec = _sleepRemainTimeSec;
				int showSec = 1;
				int showMin = 1;
				int showHour = 1;
				int showDay = 1;
				int num2 = sleepRemainTimeSec;
				UIHelper.SetLabelText(petRemainTimeLabel, UIHelper.GetRemainTimeTextAll(num2, (byte)showDay != 0, (byte)showHour != 0, (byte)showMin != 0, (byte)showSec != 0));
			}
			else if (_petInfo.Status == PetStatusType.Explore)
			{
				int petExploreRemainSec = PetHelper.GetPetExploreRemainSec(_petInfo.PetIndex);
				long num3 = petExploreRemainSec;
				int num4 = 0;
				if (num3 > num4)
				{
					UIHelper.SetLabelText(PetRemainTimeLabel, UIHelper.GetRemainTimeTextAll(num3));
				}
				else
				{
					UIHelper.SetLabelText(PetRemainTimeLabel, LocalizationManager.GetStringDirect("EXPLORE_END"));
				}
			}
		}
		else
		{
			if (_sleepRemainTimeSec > 0)
			{
				_sleepRemainTimeSec = num;
				UpdateButtonState();
			}
			_sleepRemainTimeSec = num;
			UIHelper.EnableGameObject(FeedPercentLabel, enable: true);
			UIHelper.EnableGameObject(PetRemainTimeLabel, enable: false);
			SetGaugeLabel(_currFullPoint);
		}
		if (_petInfo.IsLinePet)
		{
			int maxFullPoint = _maxFullPoint;
			_currFullPoint = maxFullPoint;
			SetGauge(_currFullPoint);
			SetGaugeLabel(_currFullPoint);
		}
	}

	public void UpdateButtonState()
	{
		if (!PetHelper.CanExplorePetStatus(_petInfo))
		{
			UIHelper.EnableGameObject(DisableFeedButton, enable: true);
			UIHelper.EnableGameObject(FeedOnceButton, enable: false);
			UIHelper.EnableGameObject(FeedMaxButton, enable: false);
			UIHelper.EnableGameObject(GaugeSprite, enable: false);
			UIHelper.EnableGameObject(FullGaugeSprite, enable: false);
		}
		else
		{
			long remainFullPoint = _remainFullPoint;
			int num = 1;
			if (remainFullPoint < num)
			{
				UIHelper.EnableGameObject(GaugeSprite, enable: false);
				UIHelper.EnableGameObject(FullGaugeSprite, enable: true);
				UIHelper.EnableGameObject(DisableFeedButton, enable: true);
				UIHelper.EnableGameObject(FeedOnceButton, enable: false);
				UIHelper.EnableGameObject(FeedMaxButton, enable: false);
			}
			else
			{
				UIHelper.EnableGameObject(GaugeSprite, enable: true);
				UIHelper.EnableGameObject(FullGaugeSprite, enable: false);
				UIHelper.EnableGameObject(DisableFeedButton, enable: false);
				UIHelper.EnableGameObject(FeedOnceButton, enable: true);
				UIButton feedOnceButton = FeedOnceButton;
				long feedAmount = _feedAmount;
				int num2 = 0;
				UIHelper.EnableButton(feedOnceButton, feedAmount > num2);
				UIButton feedMaxButton = FeedMaxButton;
				long feedAmount2 = _feedAmount;
				int num3 = 0;
				UIHelper.EnableGameObject(feedMaxButton, feedAmount2 > num3);
			}
		}
		UpdatePetStatus();
	}

	private void UpdatePetStatus()
	{
		if (_sleepRemainTimeSec > 0)
		{
			PetStatusIconSelector.Select(EStatusIcon.Sleep.ToString());
			DisableFeedButtonLabel.text = LocalizationManager.GetStringDirect("PET_MOTION_SLEEP");
		}
		else if (_petInfo.Status == PetStatusType.Explore)
		{
			PetStatusIconSelector.Select(EStatusIcon.Explore.ToString());
			DisableFeedButtonLabel.text = LocalizationManager.GetStringDirect("EXPLORE_DOING2");
		}
		else
		{
			PetStatusIconSelector.Select(EStatusIcon.Food.ToString());
			DisableFeedButtonLabel.text = LocalizationManager.GetStringDirect("PET_BUTTON_FEED");
		}
	}

	private void SetGauge(float val)
	{
		UIHelper.SetSpriteFillAmount(GaugeSprite, val / (float)_maxFullPoint);
	}

	private void SetGaugeLabel(long val)
	{
		string text = $"{val}/{_maxFullPoint}";
		UIHelper.SetLabelText(FeedPercentLabel, text);
	}

	private void StartGaugeAnimation(long feedAmount)
	{
		if (_gaugeAni != EGaugeAni.RUN)
		{
			long currFullPoint = _currFullPoint;
			int maxFullPoint = _maxFullPoint;
			if (currFullPoint < maxFullPoint)
			{
				_runtimeCo = VespaCoroutine.Start(coStartGaugeAnimation(feedAmount), null, null);
			}
		}
	}

	private void PlayGaugeEffect()
	{
		long currFullPoint = _currFullPoint;
		int maxFullPoint = _maxFullPoint;
		if (currFullPoint == maxFullPoint)
		{
			PlayGageEffectMax();
		}
		else
		{
			PlayGageDefaultEffect();
		}
		PlayEatSound();
	}

	private void PlayEatSound()
	{
		GaugeRootEffect.PlayInstantSound("Pet.System.Eat", null);
	}

	private void PlayGageDefaultEffect()
	{
		if (!UIHelper.IsEnabledGameObjet(FeedGageParticle))
		{
			UIHelper.EnableGameObject(FeedGageParticle, enable: true);
		}
		else
		{
			FeedGageParticle.Play();
		}
	}

	private void PlayGageEffectMax()
	{
		if (!UIHelper.IsEnabledGameObjet(FeedMaxParticle))
		{
			UIHelper.EnableGameObject(FeedMaxParticle, enable: true);
		}
		else
		{
			FeedMaxParticle.Play();
		}
		_lastCheckedFeedEffectTime = Time.time;
	}

	private long GetRemainMaxFeedCount()
	{
		long num = (long)Mathf.Ceil((float)_remainFullPoint / (float)_onceFeedPoint);
		if (num <= _feedAmount)
		{
			return num;
		}
		return _feedAmount;
	}

	private IEnumerator coStartGaugeAnimation(long feedAmount, Action onComplete = null)
	{
		_gaugeAni = EGaugeAni.RUN;
		float elapsTime = 0f;
		PetFeedComponent petFeedComponent = this;
		long currFullPoint = _currFullPoint;
		int onceFeedPoint = _onceFeedPoint;
		checked
		{
			petFeedComponent._nextFullPoint = currFullPoint + onceFeedPoint * feedAmount;
			PetFeedComponent petFeedComponent2 = this;
			long nextFullPoint = _nextFullPoint;
			int maxFullPoint = _maxFullPoint;
			petFeedComponent2._nextFullPoint = Math.Min(nextFullPoint, maxFullPoint);
			SetPetFeedAmount(feedAmount);
			if (_isPress)
			{
				PetFeedComponent petFeedComponent3 = this;
				long usedCount = _usedCount;
				int num = 1;
				petFeedComponent3._usedCount = usedCount + num;
			}
		}
		while (true)
		{
			float num2 = Mathf.Lerp(_currFullPoint, _nextFullPoint, elapsTime / (GaugeAniTime * (float)feedAmount));
			SetGauge(num2);
			SetGaugeLabel((long)num2);
			elapsTime += Time.deltaTime;
			if (elapsTime >= GaugeAniTime)
			{
				break;
			}
			yield return null;
		}
		_currFullPoint = _nextFullPoint;
		SetGauge(_currFullPoint);
		SetGaugeLabel(_currFullPoint);
		if (onComplete != null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		_gaugeAni = EGaugeAni.IDLE;
	}

	public void SetPetFeedAmount(long amount)
	{
		if (_listener != null)
		{
			_listener.SetPetFeedAmount(amount);
		}
	}

	public void UpdatePetFeepAmountPopup()
	{
		if (_listener != null)
		{
			_listener.UpdatePetFeedAmountPopup();
		}
	}

	public void SyncPetFullPoint()
	{
		UpdatePetFeepAmountPopup();
		SetGauge(_currFullPoint);
		SetGaugeLabel(_currFullPoint);
	}

	public void OnClickFeedMaxButton()
	{
		if (SprkCanFeed())
		{
			_usedCount = GetRemainMaxFeedCount();
			_isPress = true;
			OnReleaseFeedButton();
		}
	}

	public void OnPressFeedButton()
	{
		if (SprkCanFeed())
		{
			int num = 0;
			_usedCount = num;
			_isPress = true;
			UpdateComp();
		}
	}

	public void OnReleaseFeedButton()
	{
		if (!_isPress)
		{
			return;
		}
		if (_listener != null)
		{
			_listener.OnClickPetFeed(_petInfo, _usedCount, delegate(bool result)
			{
				if (result)
				{
					PlayGaugeEffect();
				}
				SyncPetFullPoint();
			});
		}
		_isPress = false;
		int num = 0;
		_usedCount = num;
	}

	private bool SprkCanFeed()
	{
		if (_listener != null && _petInfo != null && !_isPress && _gaugeAni == EGaugeAni.IDLE)
		{
			long feedAmount = _feedAmount;
			int num = 0;
			if (feedAmount > num)
			{
				long remainFullPoint = _remainFullPoint;
				int num2 = 0;
				if (remainFullPoint > num2)
				{
					return PetHelper.CanExplorePetStatus(_petInfo);
				}
			}
		}
		return false;
	}
}
