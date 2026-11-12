using System;
using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NUI.NWindow.NBattle;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class DamageMeterComponent : LinkFactoryComponent, InitOnCreate
{
	public enum DisplayType
	{
		Default,
		Status,
		Tps
	}

	[Serializable]
	public struct ManaGaugeComp
	{
		public GameObject Root;

		public UISprite Gauge_Normal;

		public UISprite Gauge_Full;
	}

	private enum ManaAddictState
	{
		Normal,
		Level1,
		Level2,
		Level3,
		Addict4
	}

	public const string PrefabPath = "Prefab2/Component/Battle/DamageMeterComponent";

	public static PrefabAuto<DamageMeterComponent> PrefabAuto = new PrefabAuto<DamageMeterComponent>("Prefab2/Component/Battle/DamageMeterComponent");

	private const int MANA_GAUGE_SLOT_COUNT = 6;

	public UITexture PortraitTex;

	public UIBundleLoader BundleTexPortrait;

	public UISprite DamageTotalGaugeBg;

	public UISprite DamageTotalGauge;

	public UILabel DamageTotalLabel;

	public UILabel DpsPrefixLabel;

	public UILabel DpsAmountLabel;

	public UILabel TpsTotalLabel;

	public UISprite TpsTotalGauge;

	public UIWidget TpsDangerWidget;

	public Color Color_Damage = Colors.orange;

	public Color Color_DpsPrefix = Color.yellow;

	public Color Color_Heal = Colors.skyblue;

	public Color Color_HpsPrefix = Colors.skyblue;

	public Color Color_Mana = Colors.skyblue;

	public Color Color_ManaPrefix = Colors.skyblue;

	public UIWidget ParentWidget;

	[Header("DPS HPS MPS Meter")]
	public UIWidget StatusGuageWidget;

	public UIWidget DpsGuageWidget;

	public UIWidget DpsLabelWidget;

	[Header("TPS Meter")]
	public UIWidget TpsGaugeWidget;

	public UIWidget TpsLabelWidget;

	[Header("Status HP Gauge")]
	public UISprite HpTotalGauge;

	public UISprite HpWarningAlarm;

	public UILabel StatusLabel;

	public UISprite Sprite_InavlidStatus;

	[Header("Status Mana Gauge")]
	public GameObject Gauge_Root;

	public UISprite Mana_Gauge_Normal;

	public UISprite Mana_Gauge_Full;

	public ManaGaugeComp ManaAddict0;

	public ManaGaugeComp ManaAddict1;

	public ManaGaugeComp ManaAddict2;

	public ManaGaugeComp ManaAddict3;

	private ManaAddictState _currentManaState;

	private EMeterType _currentMeterType;

	private ShowDpsType _showDpsType;

	private Holder _receiverHolder = new Holder();

	private IncreaseDic<string, long> _ccStateDic = new IncreaseDic<string, long>(16);

	private int MpsUnitTime;

	private int TpsUnitTime;

	private long _totalElapsedTick;

	private int _waveIndex;

	private long _prevBattleTimeMs;

	private bool _isRefreshWidget;

	private bool _prevCcState;

	private bool _enableSqrt;

	public long TotalDamage
	{
		get
		{
			BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(CreatureUid);
			if (creatureCache.Uid != 0)
			{
				return creatureCache.TotalDamage;
			}
			return 0L;
		}
	}

	public long TotalHeal
	{
		get
		{
			BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(CreatureUid);
			if (creatureCache.Uid != 0)
			{
				return creatureCache.TotalHeal;
			}
			return 0L;
		}
	}

	public long TotalThreatInUnitTime
	{
		get
		{
			BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(CreatureUid);
			if (creatureCache.Uid != 0)
			{
				return creatureCache.TotalThreatInUnitTime;
			}
			return 0L;
		}
	}

	public long TotalManaRestored
	{
		get
		{
			BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(CreatureUid);
			if (creatureCache.Uid != 0)
			{
				return creatureCache.TotalManaRestored;
			}
			return 0L;
		}
	}

	public long TotalManaRestoreInUnitTime
	{
		get
		{
			BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(CreatureUid);
			if (creatureCache.Uid != 0)
			{
				return creatureCache.TotalManaRestoredInUnitTime;
			}
			return 0L;
		}
	}

	public float CurrentHPRatio
	{
		get
		{
			BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(CreatureUid);
			if (creatureCache.Uid != 0)
			{
				return (float)creatureCache.HpRatioM * 0.001f;
			}
			return 0f;
		}
	}

	public int CurrentMP
	{
		get
		{
			BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(CreatureUid);
			if (creatureCache.Uid != 0)
			{
				return creatureCache.Mp;
			}
			return 0;
		}
	}

	public long MaxMp
	{
		get
		{
			BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(CreatureUid);
			if (creatureCache.Uid != 0)
			{
				return creatureCache.MaxMp;
			}
			return 0L;
		}
	}

	public int CreatureUid { get; private set; }

	public long PositionScore { get; private set; }

	public bool IsDead { get; private set; }

	public long CurrentTimeMs { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ParentWidget.isGrayscale = false;
		_currentMeterType = EMeterType.None;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_receiverHolder.Clear();
	}

	public static AssetTexture GetHeroIcon(HeroInfo heroInfo)
	{
		return UIHelper.GetHeroIcon2(heroInfo);
	}

	public bool Open(BattleFacade.CreatureCacheData creatureCache, long playerId, ShowDpsType showDpsType)
	{
		if (creatureCache == null || creatureCache.Uid == 0 || creatureCache.HeroInfo == null)
		{
			return false;
		}
		_showDpsType = showDpsType;
		try
		{
			PositionScore = StatCalculator.GetCreatureWavePositionScore(creatureCache.HeroInfo);
		}
		catch (Exception)
		{
			return false;
		}
		IsDead = false;
		_isRefreshWidget = false;
		SetGrayScaleWidget(grayScale: false, ParentWidget);
		CreatureUid = creatureCache.Uid;
		_waveIndex = creatureCache.EnterWaveIndex;
		_prevBattleTimeMs = creatureCache.EnterTimeMs;
		CreatureVerifier verifier = new CreatureVerifier(CreatureUid);
		_receiverHolder.AddByCreatureUid<CallbackMessage<ChangeState>>(verifier, OnMessageFast, CreatureUid);
		MpsUnitTime = BattleConstant.ManaMeterUnitTime;
		TpsUnitTime = BattleConstant.ThreatMeterUnitTime;
		UIHelper.SetLabelText(DpsAmountLabel, string.Empty);
		UIHelper.SetLabelText(DamageTotalLabel, string.Empty);
		UIHelper.SetLabelText(TpsTotalLabel, string.Empty);
		UIHelper.EnableGameObject(TpsDangerWidget, enable: false);
		SetPortraitTexture(creatureCache.HeroInfo);
		SetGaugeWidget(EMeterType.Damage);
		_enableSqrt = GameRoot.Instance.ConstantDataContainer.GetValueBool("ThreatMeterSqrt");
		return true;
	}

	private void SetPortraitTexture(CachedHeroInfo heroInfo)
	{
		string heroIconPath = UIHelper.GetHeroIconPath(heroInfo);
		UIHelper.SetBundleTexture(BundleTexPortrait, heroIconPath);
	}

	public void Close()
	{
		_receiverHolder.Clear();
	}

	public void UpdateTick()
	{
		BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(CreatureUid);
		if (creatureCache.Uid == 0)
		{
			return;
		}
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (IsDead != creatureCache.IsDead)
		{
			_isRefreshWidget = true;
			IsDead = creatureCache.IsDead;
		}
		if (!IsDead)
		{
			CurrentTimeMs = battleCache.MainCurrentTimeMs - creatureCache.CreatedTime;
			if (CurrentTimeMs < 0)
			{
				CurrentTimeMs = 0L;
			}
		}
		if (!battleCache.IsBattleEnabled)
		{
			_prevBattleTimeMs = 0L;
		}
		else
		{
			if (battleCache.WaveIndex < _waveIndex)
			{
				return;
			}
			if (battleCache.WaveIndex == _waveIndex)
			{
				if (battleCache.ElapsedBattleTimeMs <= _prevBattleTimeMs)
				{
					return;
				}
			}
			else
			{
				_waveIndex = battleCache.WaveIndex;
				_prevBattleTimeMs = 0L;
			}
			long num = battleCache.ElapsedBattleTimeMs - _prevBattleTimeMs;
			_ = 0;
			_totalElapsedTick += num;
			_waveIndex = battleCache.WaveIndex;
			_prevBattleTimeMs = battleCache.ElapsedBattleTimeMs;
		}
	}

	public void UpdateWidget(EMeterType meterType, long maxDamage, long maxHeal, long maxThreat, long maxMana)
	{
		if (_isRefreshWidget)
		{
			SetPortraitCcState(visible: false);
			SetGrayScaleWidget(IsDead, ParentWidget);
			SetTpsDanger(dangerEnable: false);
			_isRefreshWidget = false;
		}
		if (_currentMeterType != meterType)
		{
			_currentMeterType = meterType;
			SetGaugeWidget(meterType);
		}
		switch (meterType)
		{
		case EMeterType.Damage:
		{
			float fillAmount = Mathf.Clamp01((float)TotalDamage / (float)maxDamage);
			long totalDamage = TotalDamage;
			long num = ((CurrentTimeMs > 1000) ? (CurrentTimeMs / 1000) : 1);
			long dpsAmount = totalDamage / num;
			SetDpsGauge(fillAmount, totalDamage, dpsAmount);
			break;
		}
		case EMeterType.Heal:
		{
			float fillAmount2 = Mathf.Clamp01((float)TotalHeal / (float)maxHeal);
			long totalHeal = TotalHeal;
			long num2 = ((CurrentTimeMs > 1000) ? (CurrentTimeMs / 1000) : 1);
			long dpsAmount2 = totalHeal / num2;
			SetDpsGauge(fillAmount2, totalHeal, dpsAmount2);
			break;
		}
		case EMeterType.Threat:
		{
			long num3 = TotalThreatInUnitTime;
			if (_enableSqrt)
			{
				num3 = Convert.ToInt64(System.Math.Sqrt(num3));
			}
			float fillAmount3 = Mathf.Clamp01((float)num3 / (float)maxThreat);
			SetTpsGauge(fillAmount3, num3);
			break;
		}
		case EMeterType.ManaRestored:
		{
			float fillAmount4 = Mathf.Clamp01((float)TotalManaRestoreInUnitTime / (float)maxMana);
			long totalManaRestoreInUnitTime = TotalManaRestoreInUnitTime;
			long dpsAmount3 = totalManaRestoreInUnitTime / MpsUnitTime;
			SetDpsGauge(fillAmount4, totalManaRestoreInUnitTime, dpsAmount3);
			break;
		}
		case EMeterType.Status:
		{
			bool flag = __Extension_Method_ICollection__.COUNT(_ccStateDic) > 0;
			if (!IsDead && _prevCcState != flag)
			{
				SetPortraitCcState(flag);
				_prevCcState = flag;
			}
			float currentHPRatio = CurrentHPRatio;
			SetHpWarningAlarm(currentHPRatio);
			UpdateManaAddictState();
			SetStatusGauge(currentHPRatio, CurrentMP, MaxMp);
			break;
		}
		}
	}

	private void SetGaugeWidget(EMeterType meterType)
	{
		DisplayType displayType = DisplayType.Default;
		switch (meterType)
		{
		case EMeterType.Status:
			displayType = DisplayType.Status;
			break;
		case EMeterType.Threat:
			displayType = DisplayType.Tps;
			break;
		}
		UIHelper.EnableGameObject(DpsGuageWidget, displayType == DisplayType.Default);
		UIHelper.EnableGameObject(DpsLabelWidget, displayType == DisplayType.Default);
		UIHelper.EnableGameObject(StatusGuageWidget, displayType == DisplayType.Status);
		UIHelper.EnableGameObject(TpsGaugeWidget, displayType == DisplayType.Tps);
		UIHelper.EnableGameObject(TpsLabelWidget, displayType == DisplayType.Tps);
		switch (meterType)
		{
		case EMeterType.Damage:
			UIHelper.SetWidgetColor(DamageTotalGaugeBg, Color_Damage);
			UIHelper.SetWidgetColor(DamageTotalGauge, Color_Damage);
			UIHelper.SetLabelText(DpsPrefixLabel, "Dps");
			UIHelper.SetLabelColor(DpsPrefixLabel, Color_DpsPrefix);
			break;
		case EMeterType.Heal:
			UIHelper.SetWidgetColor(DamageTotalGaugeBg, Color_Heal);
			UIHelper.SetWidgetColor(DamageTotalGauge, Color_Heal);
			UIHelper.SetLabelText(DpsPrefixLabel, "Hps");
			UIHelper.SetLabelColor(DpsPrefixLabel, Color_HpsPrefix);
			break;
		case EMeterType.ManaRestored:
			UIHelper.SetWidgetColor(DamageTotalGaugeBg, Color_Mana);
			UIHelper.SetWidgetColor(DamageTotalGauge, Color_Mana);
			UIHelper.SetLabelText(DpsPrefixLabel, "Mps");
			UIHelper.SetLabelColor(DpsPrefixLabel, Color_ManaPrefix);
			break;
		}
		if (meterType != EMeterType.Status)
		{
			SetPortraitCcState(visible: false);
			UIHelper.EnableGameObject(HpWarningAlarm, enable: false);
		}
	}

	private void SetGrayScaleWidget(bool grayScale, UIWidget reWidget)
	{
		ParentWidget.isGrayscale = grayScale;
		UIHelper.EnableGameObject(reWidget, enable: false);
		UIHelper.EnableGameObject(reWidget, enable: true);
		UIHelper.EnableGameObject(HpWarningAlarm, enable: false);
	}

	private void SetPortraitCcState(bool visible)
	{
		UIHelper.EnableGameObject(Sprite_InavlidStatus, visible);
		UIHelper.EnableGameObject(StatusLabel, visible);
	}

	private void SetDpsGauge(float fillAmount, long totalAmount, long dpsAmount)
	{
		UIHelper.SetSpriteFillAmount(DamageTotalGauge, fillAmount);
		UIHelper.SetLabelText(DamageTotalLabel, Vespa_Long_Extension_Method.WithDamageUnit(totalAmount));
		UIHelper.SetLabelText(DpsAmountLabel, Vespa_Long_Extension_Method.WithDamageUnit(dpsAmount));
	}

	private void SetTpsGauge(float fillAmount, long totalAmount)
	{
		UIHelper.SetSpriteFillAmount(TpsTotalGauge, fillAmount);
		UIHelper.SetLabelText(TpsTotalLabel, Vespa_Long_Extension_Method.WithDamageUnit(totalAmount));
	}

	private void SetHpWarningAlarm(float currentHpRatio)
	{
		if (IsDead)
		{
			return;
		}
		if (currentHpRatio < 0.5f)
		{
			if (!UIHelper.IsEnabledGameObjet(HpWarningAlarm))
			{
				UIHelper.EnableGameObject(HpWarningAlarm, enable: true);
			}
		}
		else if (UIHelper.IsEnabledGameObjet(HpWarningAlarm))
		{
			UIHelper.EnableGameObject(HpWarningAlarm, enable: false);
		}
	}

	private void SetStatusGauge(float currentHpRatio, int currentMp, long maxMp)
	{
		UIHelper.SetSpriteFillAmount(HpTotalGauge, currentHpRatio);
		int num = Mathf.Min(6, (int)maxMp / 100);
		int num2 = currentMp * num / Mathf.Max(1, (int)maxMp);
		UIHelper.SetSpriteFillAmount(Mana_Gauge_Normal, (float)currentMp / (float)maxMp);
		UIHelper.SetSpriteFillAmount(Mana_Gauge_Full, (float)num2 / 6f);
	}

	public void SetTpsDanger(bool dangerEnable)
	{
		if (_currentMeterType != EMeterType.Threat)
		{
			return;
		}
		if (IsDead)
		{
			UIHelper.EnableGameObject(TpsDangerWidget, enable: false);
		}
		else if (dangerEnable)
		{
			if (!UIHelper.IsEnabledGameObjet(TpsDangerWidget))
			{
				UIHelper.EnableGameObject(TpsDangerWidget, enable: true);
			}
		}
		else if (UIHelper.IsEnabledGameObjet(TpsDangerWidget))
		{
			UIHelper.EnableGameObject(TpsDangerWidget, enable: false);
		}
	}

	private void UpdateManaAddictState()
	{
		if (_showDpsType != ShowDpsType.MPS || Gauge_Root == null || IsDead)
		{
			return;
		}
		long num = TotalManaRestoreInUnitTime / MpsUnitTime;
		ManaAddictState currentManaState = _currentManaState;
		if (num < BattleConstant.ManaAddictLimit1)
		{
			_currentManaState = ManaAddictState.Normal;
		}
		else if (num >= BattleConstant.ManaAddictLimit1 && num < BattleConstant.ManaAddictLimit2)
		{
			_currentManaState = ManaAddictState.Level1;
		}
		else if (num >= BattleConstant.ManaAddictLimit2 && num < BattleConstant.ManaAddictLimit3)
		{
			_currentManaState = ManaAddictState.Level2;
		}
		else
		{
			_currentManaState = ManaAddictState.Level3;
		}
		if (currentManaState != _currentManaState)
		{
			switch (_currentManaState)
			{
			case ManaAddictState.Level1:
				UIHelper.EnableGameObject(Gauge_Root, enable: false);
				Gauge_Root = ManaAddict1.Root;
				Mana_Gauge_Normal = ManaAddict1.Gauge_Normal;
				Mana_Gauge_Full = ManaAddict1.Gauge_Full;
				UIHelper.EnableGameObject(Gauge_Root, base.enabled);
				break;
			case ManaAddictState.Level2:
				UIHelper.EnableGameObject(Gauge_Root, enable: false);
				Gauge_Root = ManaAddict2.Root;
				Mana_Gauge_Normal = ManaAddict2.Gauge_Normal;
				Mana_Gauge_Full = ManaAddict2.Gauge_Full;
				UIHelper.EnableGameObject(Gauge_Root, base.enabled);
				break;
			case ManaAddictState.Level3:
				UIHelper.EnableGameObject(Gauge_Root, enable: false);
				Gauge_Root = ManaAddict3.Root;
				Mana_Gauge_Normal = ManaAddict3.Gauge_Normal;
				Mana_Gauge_Full = ManaAddict3.Gauge_Full;
				UIHelper.EnableGameObject(Gauge_Root, base.enabled);
				break;
			default:
				UIHelper.EnableGameObject(Gauge_Root, enable: false);
				Gauge_Root = ManaAddict0.Root;
				Mana_Gauge_Normal = ManaAddict0.Gauge_Normal;
				Mana_Gauge_Full = ManaAddict0.Gauge_Full;
				UIHelper.EnableGameObject(Gauge_Root, base.enabled);
				break;
			}
		}
	}

	private void OnMessageFast(CallbackMessage<ChangeState> message)
	{
		if (!message.IsValidMessage() || message.Callback.CreatureUid != CreatureUid)
		{
			return;
		}
		StateData data = GameRoot.Instance.StateDataContainer.GetData(message.Callback.ChangedStateType);
		if (data != null)
		{
			if (message.Callback.StateAdded)
			{
				AddState(message.Callback, data);
			}
			else
			{
				RemoveState(message.Callback);
			}
		}
	}

	private void AddState(ChangeState callback, StateData stateData)
	{
		if (_ccStateDic.GETVALUE(callback.ChangedStateType, 0L) == 0L && stateData.IsCc)
		{
			_ccStateDic.Add(callback.ChangedStateType, callback.OperationUid);
		}
	}

	private void RemoveState(ChangeState callback)
	{
		long vALUE = _ccStateDic.GETVALUE(callback.ChangedStateType, 0L);
		if (vALUE != 0L && vALUE == callback.OperationUid)
		{
			_ccStateDic.RemoveSlow(callback.ChangedStateType);
		}
	}
}
