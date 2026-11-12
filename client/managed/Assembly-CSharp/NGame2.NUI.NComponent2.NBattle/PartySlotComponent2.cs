using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUtil;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class PartySlotComponent2 : LinkFactoryComponent, SkillTrigger.IListener, IInspectorGuiDrawer, NVespa.NPrefab2.InitOnCreate
{
	public interface Listener
	{
		ReplaceSkillData GetReplacedSkillData(int creatureUid, int slotIndex);

		void OnClickSlot(PartySlotComponent2 partySlotComponent);
	}

	public enum ManaGaugeMode
	{
		Normal,
		Warning1,
		Warning2,
		Warning3
	}

	[Serializable]
	public class GlowUiData
	{
		public UISprite Glow;

		public TweenAlpha GlowTween;

		public UISpriteSelector SpriteMode;
	}

	[Serializable]
	public class AutoEffectUiData
	{
		public GameObject Root;

		public List<ParticleSystem> ParticleSystems = new List<ParticleSystem>();
	}

	public const string PrefabPath = "Prefab2/Component/Battle/PartySlotComponent2";

	public static PrefabAuto<PartySlotComponent2> PrefabAuto = new PrefabAuto<PartySlotComponent2>("Prefab2/Component/Battle/PartySlotComponent2");

	private const int GaugeSlotCount = 6;

	private const int MANA_GAUGE_TURNBACK_LIMIT = 1000;

	public UIWidget RootWidget;

	public SkillTrigger BaseSkillTrigger;

	public UITweener TweenDead;

	public UIBundleLoader HeroBundleTex;

	public UIGame3DWidget PortraitEffect;

	public UISprite HpGauge;

	public Transform RootSoulWeaponSkill;

	public UISimpleGrid BeforeSoulWeaponBuffGrid;

	public UISimpleGrid AfterSoulWeaponBuffGrid;

	public UISimpleGrid SkillSlotGrid;

	public int MaxSkillSlot = 3;

	public float FullGaugeBlinkTime;

	public AnimationCurve FullGaugeBlinkCurve = new AnimationCurve();

	[Header("Mana Gauge")]
	public UISprite Gauge_Normal;

	public UISprite Gauge_Full;

	public UISprite GaugeFullEffect;

	[Space(10f)]
	public UISpriteSelector Gauge_Background_SpriteMode;

	public UISpriteSelector Gauge_Normal_SpriteMode;

	public UISpriteSelector Gauge_Full_SpriteMode;

	public UISpriteSelector GaugeFullEffect_SpriteMode;

	[HideInInspector]
	public List<GlowUiData> GlowUiList = new List<GlowUiData>();

	[Space(20f)]
	public UILabel LabelAuto;

	public ParticleSystem AutoEffect;

	public AutoEffectUiData AutoEffectUI = new AutoEffectUiData();

	public UISprite SpriteAutoFilter;

	public TweenScale TweenPortrait;

	public TweenScale TweenAuto;

	public UIButton ButtonPortrait;

	public Animator TpsAnimator;

	public UIWidget TpsEffectWidget;

	private ManaGaugeMode _currentManaGaugeState;

	private bool _manaAddictMode;

	private int _creatureUid;

	private bool _autoPlayerable;

	private int _groupIndex;

	private List<BaseSkillComponent> _skillList = new List<BaseSkillComponent>();

	private bool _isDead;

	private int _lastFullGauge;

	private BattleStateUiController _stateUiController = new BattleStateUiController();

	private BaseSkillComponent.ClickIconCallback _onClickSkill;

	private Listener _listener;

	private Dictionary<int, Helper.SkillBasicInfo> _cachedSkillInfo = new Dictionary<int, Helper.SkillBasicInfo>();

	private bool _isOpenedSoulWeaponSkill;

	private int _manameterUnitTime = 10;

	private long mpsLastUpdateTime;

	private LocalAccountBool _enableAuto;

	public bool IsAuto
	{
		get
		{
			if (_enableAuto == null)
			{
				return false;
			}
			return _enableAuto.Value;
		}
	}

	public int CreatureUid => _creatureUid;

	public BattleFacade.CreatureCacheData CreatureCache => SingletonAuto<BattleFacade>.instance.GetCreatureCache(_creatureUid);

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		BuffIconComponent.PrefabAuto.Factory.ReserveComponent(10);
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
	}

	void SkillTrigger.IListener.OnDrag(Vector3 posOnViewport)
	{
		if (CreatureCache.Uid != 0)
		{
			CreatureCache.IsDeadOrResting();
		}
	}

	void SkillTrigger.IListener.OnRelease()
	{
	}

	public bool Open(int creatureUid, BaseSkillComponent.ClickIconCallback onClickSkill, Listener listener, bool autoPlayerable, int groupIndex = 0, bool manaAddictMode = false)
	{
		Reset();
		try
		{
			return OpenInternal(creatureUid, onClickSkill, listener, autoPlayerable, groupIndex, manaAddictMode);
		}
		catch (Exception)
		{
			return false;
		}
	}

	private bool OpenInternal(int creatureUid, BaseSkillComponent.ClickIconCallback onClickSkill, Listener listener, bool autoPlayerable, int groupIndex, bool manaAddictMode = false)
	{
		UIHelper.EnableGameObject(TpsEffectWidget, enable: false);
		UIHelper.EnableGameObject(LabelAuto, autoPlayerable);
		UIHelper.EnableButton(ButtonPortrait, autoPlayerable);
		if (autoPlayerable)
		{
			TweenPortrait.ResetToBeginning();
			TweenAuto.ResetToBeginning();
			TweenPortrait.PlayForwardReset();
			TweenAuto.PlayForwardReset();
		}
		base.gameObject.SetActive(value: true);
		_creatureUid = creatureUid;
		_onClickSkill = onClickSkill;
		_listener = listener;
		_cachedSkillInfo.Clear();
		_autoPlayerable = autoPlayerable;
		_groupIndex = groupIndex;
		_isOpenedSoulWeaponSkill = GetIsOpendSoulWeapon();
		_manameterUnitTime = GameRoot.Instance.ConstantDataContainer.GetValueInt("ManaMeterUnitTime", 10);
		_manaAddictMode = manaAddictMode;
		if (_isOpenedSoulWeaponSkill)
		{
			_stateUiController.Open(_creatureUid, AfterSoulWeaponBuffGrid);
		}
		else
		{
			_stateUiController.Open(_creatureUid, BeforeSoulWeaponBuffGrid);
		}
		UIHelper.SetSpriteFillAmount(Gauge_Normal, 0f);
		UIHelper.SetSpriteFillAmount(Gauge_Full, 0f);
		UIHelper.SetSpriteFillAmount(GaugeFullEffect, 0f);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GlowUiList); i++)
		{
			GlowUiData glowUiData = GlowUiList[i];
			UIHelper.EnableGameObject(glowUiData.Glow, enable: true);
			glowUiData.Glow.alpha = 0f;
		}
		int baseSkillIndex = CreatureCache.CreatureData.BaseSkillIndex;
		for (int j = 1; j < 10; j++)
		{
			if (__Extension_Method_ICollection__.COUNT(_skillList) >= MaxSkillSlot)
			{
				break;
			}
			Helper.SkillBasicInfo skillBasicInfo = Helper.GetSkillBasicInfo(CreatureCache.CreateInfo.HeroInfo, j);
			if (skillBasicInfo.TableIndex != 0 && skillBasicInfo.TableIndex != baseSkillIndex && skillBasicInfo.TableData != null && skillBasicInfo.TableData.TriggerType == SkillTriggerType.None)
			{
				int equipOptionAddedSkillLevel = StatCalculator.GetEquipOptionAddedSkillLevel(CreatureCache.CreateInfo.StatData, j);
				skillBasicInfo.Level += equipOptionAddedSkillLevel;
				NormalSkillComponent normalSkillComponent = UIHelper.AddGridItem(SkillSlotGrid, NormalSkillComponent.PrefabAuto);
				normalSkillComponent.name = $"SkillComponent.{j}";
				normalSkillComponent.Open(_creatureUid, j, skillBasicInfo, _onClickSkill);
				_skillList.Add(normalSkillComponent);
			}
		}
		if (_isOpenedSoulWeaponSkill)
		{
			Helper.SkillBasicInfo soulWeaponBasicSkillInfo = Helper.GetSoulWeaponBasicSkillInfo(CreatureCache.CreateInfo.HeroInfo.HeroIndex, CreatureCache.CreateInfo.HeroInfo.GetSoulWeaponInfo());
			soulWeaponBasicSkillInfo.IsLocked = !CreatureCache.IsEquippedSoulWeapon;
			if (soulWeaponBasicSkillInfo.TableIndex != 0 && soulWeaponBasicSkillInfo.TableData != null)
			{
				SoulWeaponSkillComponent soulWeaponSkillComponent = SoulWeaponSkillComponent.PrefabAuto.Factory.Create(RootSoulWeaponSkill);
				if (soulWeaponSkillComponent != null)
				{
					soulWeaponSkillComponent.name = "SkillComponent.SoulWeaponSkill";
					soulWeaponSkillComponent.Open(_creatureUid, 10, soulWeaponBasicSkillInfo, _onClickSkill);
					_skillList.Add(soulWeaponSkillComponent);
				}
			}
		}
		UIHelper.RepositionGrid(SkillSlotGrid);
		string battleHeroPortraitPath = UIHelper.GetBattleHeroPortraitPath(CreatureCache.CreatureData.CodeName, CreatureCache.CreateInfo.HeroInfo.Star, CreatureCache.CreateInfo.HeroInfo.CostumeIndex);
		UIHelper.SetBundleTexture(HeroBundleTex, battleHeroPortraitPath);
		if (BaseSkillTrigger != null)
		{
			BaseSkillTrigger.Listener = this;
		}
		if (PortraitEffect != null)
		{
			string battleHeroPortraitEffectPath = UIHelper.GetBattleHeroPortraitEffectPath(CreatureCache.CreatureData.CodeName, CreatureCache.CreateInfo.HeroInfo.Star, CreatureCache.CreateInfo.HeroInfo.CostumeIndex);
			if (!String_Extension_Method.IsNullOrEmpty(battleHeroPortraitEffectPath))
			{
				TransformData transformData = new TransformData();
				transformData.Position = Vector3.zero;
				transformData.RotationEuler = Vector3.zero;
				transformData.Scale = 100f;
				UIHelper.EnableGameObject(PortraitEffect, enable: true);
				PortraitEffect.PlayInstantEffect(battleHeroPortraitEffectPath, transformData, null);
			}
		}
		return true;
	}

	private bool GetIsOpendSoulWeapon()
	{
		if (AccountManager.IsLogined())
		{
			return SoulWeaponManagement.IsOpenSoulweapon();
		}
		return false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_stateUiController.Reset();
	}

	public void Close()
	{
		Reset();
	}

	public bool IsShowingTooltip()
	{
		for (int i = 0; i < _skillList.Count; i++)
		{
			if (_skillList[i].IsShowingTooltip())
			{
				return true;
			}
		}
		return false;
	}

	public BaseSkillComponent FindSkillComponent(Func<BaseSkillComponent, bool> func)
	{
		if (_skillList == null || _skillList.Count == 0)
		{
			return null;
		}
		for (int i = 0; i < _skillList.Count; i++)
		{
			BaseSkillComponent baseSkillComponent = _skillList[i];
			if (baseSkillComponent != null && Vespa_System_Action_Extension_Method.fire(func, baseSkillComponent))
			{
				return baseSkillComponent;
			}
		}
		return null;
	}

	public void SetSlotIndex(int slotIndex, CampaignDungeonData data)
	{
		if (_autoPlayerable)
		{
			BattleType battleType = data.BattleType;
			int chapterIndex = data.ChapterIndex;
			int dungeonIndex = data.DungeonIndex;
			string text = "";
			int num = slotIndex;
			if (_groupIndex != 0)
			{
				int num2 = 4;
				num += _groupIndex * num2;
			}
			AutoSeparationData data2 = GameRoot.Instance.AutoSeparationDataContainer.GetData(battleType, chapterIndex, dungeonIndex);
			if (data2 != null && !String_Extension_Method.IsNullOrEmpty(data2.LocalAutoKey))
			{
				string localAutoKey = data2.LocalAutoKey;
				text = ((data2.ChapterIndex <= 0 || data2.DungeonIndex <= 0) ? $"LocalAutoSave_K={localAutoKey}_S={num}" : $"LocalAutoSave_K={localAutoKey}_S={num}_C={chapterIndex}_D={dungeonIndex}");
			}
			else if (battleType == BattleType.Eclipse && GlobalAutoBehaviour<AccountManager>.isValidInstance && GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo() != null)
			{
				int onlineGameSpeedRatio = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo().OnlineGameSpeedRatio;
				text = $"LocalAutoSave_Default_B={battleType}_S={num}_GS={onlineGameSpeedRatio}_200609_0527";
			}
			else
			{
				text = $"LocalAutoSave_Default_S={num}";
			}
			_enableAuto = new LocalAccountBool(i_default: false, text);
			UIHelper.SetWidgetGrayScale(LabelAuto, !_enableAuto.Value);
			UIHelper.EnableGameObject(SpriteAutoFilter, !_enableAuto.Value);
		}
	}

	private void Reset()
	{
		_isDead = false;
		RootWidget.SetGrayscaleRecursive(isGrayscale: false);
		TweenDead.ResetToBeginning();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_skillList); i++)
		{
			BaseSkillComponent baseSkillComponent = _skillList[i];
			baseSkillComponent.Close();
			FactoryBase.ReleaseComponent(baseSkillComponent);
		}
		_skillList.Clear();
		_stateUiController.Reset();
		_cachedSkillInfo.Clear();
		_enableAuto = null;
	}

	public void ChainUpdate()
	{
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return;
		}
		BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(_creatureUid);
		if (creatureCache.Uid == 0)
		{
			return;
		}
		UpdateHP(creatureCache);
		UpdateMP(creatureCache);
		bool isDead = creatureCache.IsDead;
		if (_isDead != isDead)
		{
			TweenDead.PlayReset(isDead);
			if (isDead)
			{
				UIHelper.EnableGameObject(TpsEffectWidget, enable: false);
			}
		}
		_isDead = isDead;
		RootWidget.SetGrayscaleRecursive(_isDead);
		UIHelper.EnableGameObject(PortraitEffect, !_isDead);
		for (int i = 0; i < _skillList.Count; i++)
		{
			_skillList[i].ChainUpdate();
		}
		_stateUiController.ChainUpdate();
	}

	private void UpdateHP(BattleFacade.CreatureCacheData creatureCache)
	{
		UIHelper.SetSpriteFillAmount(HpGauge, (float)creatureCache.HpRatioM * 0.001f);
	}

	public static int GetCurrentMpGaugeCount(int currentMp, int maxMp, int maxGaugeCount)
	{
		int num = Mathf.Min(maxGaugeCount, maxMp / 100);
		return currentMp * num / Mathf.Max(1, maxMp);
	}

	private void UpdateMP(BattleFacade.CreatureCacheData creatureCache)
	{
		int num = creatureCache.Mp;
		if (creatureCache.IsDead)
		{
			num = 0;
		}
		if (_manaAddictMode)
		{
			UpdateManaGuageUI(creatureCache);
		}
		long maxMp = creatureCache.MaxMp;
		int currentMpGaugeCount = GetCurrentMpGaugeCount(num, (int)maxMp, 6);
		UIHelper.SetSpriteFillAmount(Gauge_Normal, (float)num / (float)maxMp);
		UIHelper.SetSpriteFillAmount(Gauge_Full, (float)currentMpGaugeCount / 6f);
		UIHelper.SetSpriteFillAmount(GaugeFullEffect, (float)currentMpGaugeCount / 6f);
		if (currentMpGaugeCount > _lastFullGauge)
		{
			for (int i = _lastFullGauge; i < currentMpGaugeCount && i < __Extension_Method_ICollection__.COUNT(GlowUiList); i++)
			{
				GlowUiList[i].GlowTween.PlayForwardReset();
			}
		}
		_lastFullGauge = currentMpGaugeCount;
	}

	public void UpdateManaGuageUI(BattleFacade.CreatureCacheData creatureCache)
	{
		_ = creatureCache.TotalManaRestoredInUnitTime / _manameterUnitTime;
		ManaGaugeMode currentManaGaugeState = _currentManaGaugeState;
		ManaGaugeMode manaGaugeMode = ManaGaugeMode.Normal;
		manaGaugeMode = creatureCache.ManaAddictLevel switch
		{
			1 => ManaGaugeMode.Warning1, 
			2 => ManaGaugeMode.Warning2, 
			3 => ManaGaugeMode.Warning3, 
			_ => ManaGaugeMode.Normal, 
		};
		bool flag = false;
		if (currentManaGaugeState != manaGaugeMode)
		{
			long num = SingletonAuto<BattleFacade>.instance.GetBattleCache().GlobalCurrentTimeMs - mpsLastUpdateTime;
			if (currentManaGaugeState < manaGaugeMode || num > 1000)
			{
				flag = true;
				_currentManaGaugeState = manaGaugeMode;
			}
		}
		if (flag)
		{
			mpsLastUpdateTime = SingletonAuto<BattleFacade>.instance.GetBattleCache().GlobalCurrentTimeMs;
			Gauge_Normal_SpriteMode.SelectEnum(_currentManaGaugeState);
			Gauge_Full_SpriteMode.SelectEnum(_currentManaGaugeState);
			Gauge_Background_SpriteMode.SelectEnum(_currentManaGaugeState);
			GaugeFullEffect_SpriteMode.SelectEnum(_currentManaGaugeState);
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GlowUiList); i++)
			{
				GlowUiList[i].SpriteMode.SelectEnum(_currentManaGaugeState);
			}
		}
	}

	public void UpdateSkillIcon()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_skillList); i++)
		{
			BaseSkillComponent baseSkillComponent = _skillList[i];
			int skillSlotIndex = baseSkillComponent.SkillSlotIndex;
			ReplaceSkillData replacedSkillData = _listener.GetReplacedSkillData(CreatureCache.Uid, skillSlotIndex);
			if (!_cachedSkillInfo.TryGetValue(replacedSkillData.SkillIndex, out var value))
			{
				value = Helper.CreateSkillBasicInfo(CreatureCache.CreateInfo.HeroInfo, skillSlotIndex, replacedSkillData.SkillIndex);
				int equipOptionAddedSkillLevel = StatCalculator.GetEquipOptionAddedSkillLevel(CreatureCache.CreateInfo.StatData, skillSlotIndex);
				value.Level += equipOptionAddedSkillLevel;
				_cachedSkillInfo[replacedSkillData.SkillIndex] = value;
			}
			baseSkillComponent.SetReplaceSkillData(replacedSkillData, value);
		}
	}

	public void EnableAuto(bool auto)
	{
		_enableAuto.Value = auto;
		UIHelper.SetWidgetGrayScale(LabelAuto, !auto);
		UIHelper.EnableGameObject(SpriteAutoFilter, !auto);
		if (AutoEffectUI.Root == null)
		{
			UIHelper.EnableGameObject(AutoEffect, auto);
			if (auto)
			{
				AutoEffect.Play();
			}
		}
		UIHelper.EnableGameObject(AutoEffectUI.Root, auto);
		if (auto)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(AutoEffectUI.ParticleSystems); i++)
			{
				ParticleSystem particleSystem = AutoEffectUI.ParticleSystems[i];
				if (particleSystem != null)
				{
					particleSystem.Play();
				}
			}
		}
		TweenPortrait.PlayForwardReset();
		TweenAuto.PlayForwardReset();
	}

	public void SetTpsAnimation(bool enable)
	{
		if (_isDead)
		{
			enable = false;
		}
		else if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			enable = false;
		}
		else if (SingletonAuto<BattleFacade>.instance.GetCreatureCache(_creatureUid).Uid == 0)
		{
			enable = false;
		}
		if (UIHelper.IsEnabledGameObjet(TpsEffectWidget) != enable)
		{
			UIHelper.EnableGameObject(TpsEffectWidget, enable);
			if (enable)
			{
				TpsAnimator.Play("Anim_Punishment_TPSStart");
			}
		}
	}

	public void OnClickSlot()
	{
		Debug.LogWarning("PartySlotComponent OnClickSlot");
		EnableAuto(!_enableAuto.Value);
		_listener.OnClickSlot(this);
	}
}
