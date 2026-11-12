using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NRender3D;
using NGameAsset.NCreature;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class Hero3DViewPanel : MonoBehaviour, IHeroPanel, HeroNotifyComponent.Listener, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserItemManager.ItemCountChangedInfo>, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IInspectorGuiDrawer, ClassTagIconComponent.Listener
{
	public enum EToggle
	{
		None,
		Info,
		Equip,
		Skill,
		Attribute,
		EquipStorage,
		Profile
	}

	public enum ENextTask
	{
		None,
		OpenRunehenge
	}

	public enum ECameraState
	{
		None,
		ZoomIn,
		ZoomOut
	}

	public interface Listener
	{
		void OnToggle(EToggle toggle);

		void OnClickEquipIcon(ItemIconComponent caller);

		void OnClickClassTagIcon(ClassTagIconComponent caller);

		void OnClickDetailView();

		void OnClickAutoEquip();

		void OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo heroInfo);

		void OnClickSelectFlask(Hero3DViewPanel caller);

		void OnClickShowUniqueWeaponOn(Hero3DViewPanel caller);

		void OnClickShowUniqueWeaponOff(Hero3DViewPanel caller);

		void OnClickCostume(int heroIndex);

		void OnClickProfile(int heroIndex);

		void OnClickPresetStorage();
	}

	[Serializable]
	public class EquipIconRootInfo
	{
		public EquipPartType PartType;

		public Transform Root;
	}

	public const string PrefabPath = "Resources/Prefab2/Component/Hero/Hero3DViewPanel";

	public static PrefabAuto<Hero3DViewPanel> PrefabAuto = new PrefabAuto<Hero3DViewPanel>("Resources/Prefab2/Component/Hero/Hero3DViewPanel");

	public GameObject[] Frames;

	public GameObject[] NameTags;

	public UITexture HeroPreviewTexture;

	public UILabel Label_HeroExpPercentage;

	public UIClipSprite Sprite_ExpGauge;

	public UISprite Sprite_ExpGaugeBack;

	public Transform UsingFlaskRoot;

	public Transform NoFlaskRoot;

	public UISprite Sprite_FlaskGauge;

	public UILabel Label_FlaskPercentage;

	public UIBundleLoader flaskSelector;

	public UISprite EquipFlaskSprite;

	public UIWidget NoHaveFlask;

	public UILabel NameLabel;

	public UILabel LevelLabel;

	public UILabel MaxLevelLabel;

	public UILabel TranscendLabel;

	public Transform LimitBreakLevelLabel;

	public Transform LimitBreakMaxLevelLabel;

	public Color LimitBreakLabelColor = Color.red;

	public GameObject StarParent;

	public List<UIWidget> StarList = new List<UIWidget>();

	public List<UIWidget> TranscendStarList;

	public UISimpleGrid ClassTagGrid;

	public UIToggle ToggleInfo;

	public UIToggle ToggleEquip;

	public UIToggle ToggleSkill;

	public UIToggle ToggleAttr;

	public UIToggle ToggleProfile;

	public UIWidget Disable_Equip;

	public Hero3DViewEquipImpl EquipImpl;

	public Transform SkillPopupRoot;

	public UILabel SkillNameLabel;

	public UILabel SkillDescLabel;

	public UISprite Sprite_CanTranscended;

	public UISprite Sprite_CanSkillLevelUp;

	public UISprite Sprite_CanLimitBreak;

	public UISprite Sprite_EquipNew;

	public UISprite Sprite_AttrNew;

	public Transform FightingPowerRoot;

	public UILabel FightingPowerLabel;

	public UIToggle Toggle_OpenEquipStorageSlot;

	[Header("Camera Zoom")]
	public UIToggle Toggle_CameraZoom_In_Out;

	public float ZoomSpeed;

	public UISpriteSelector ExpGaugeSelector;

	public CachedHeroInfo CachedHeroInfo;

	private EToggle _toggle;

	private Listener _listener;

	private List<ClassTagIconComponent> _classTagIconComponentList = new List<ClassTagIconComponent>();

	private List<HeroNotifyComponent> _notifyComponentList = new List<HeroNotifyComponent>();

	private HeroPreview _heroPreview;

	private float _skillPopupDestroyTime;

	private ECameraState _cameraState;

	public void Init(Listener listener, CachedHeroInfo cachedHeroInfo, EToggle toggle)
	{
		base.gameObject.SetActive(value: true);
		CachedHeroInfo = cachedHeroInfo;
		_listener = listener;
		EquipImpl.Init(CachedHeroInfo, listener.OnClickEquipIcon);
		bool flag = CachedHeroInfo != null;
		UIHelper.EnableGameObject(SkillPopupRoot, enable: false);
		UIHelper.EnableGameObject(NameTags, flag);
		UIHelper.EnableGameObject(NameLabel, flag);
		UIHelper.EnableGameObject(LevelLabel, flag);
		UIHelper.EnableGameObject(StarParent, flag);
		UIHelper.EnableGameObject(ClassTagGrid, flag);
		UIHelper.EnableGameObject(Sprite_EquipNew, enable: false);
		SingletonUpdater<Executer>.instance.NextRun(delegate
		{
			InitPreview();
			SetupCameraZoom();
		});
		if (flag)
		{
			SetupHeroInfo();
			SetupEquipItemIcon();
			SetupClassTagIconList();
			UpdateEquipNewState();
			SetTransendAttrNewTag();
			SetupHeroExp();
			SetupFlask();
			SetupStar();
		}
		else
		{
			ClearEquipItemIconList();
			ClearClassTagIconList();
			ClearNotifyList();
			ClearFlask();
		}
		SetToggle(toggle);
		GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.ObserverManager.AddObserver(this);
	}

	private void SetupHeroInfo()
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(CachedHeroInfo.HeroIndex);
		if (data == null)
		{
			SetupError("Cannot Find CreatureData: " + CachedHeroInfo.HeroIndex);
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StarList); i++)
		{
			if ((bool)StarList[i])
			{
				UIHelper.EnableGameObject(StarList[i], i + 1 == CachedHeroInfo.Star);
			}
		}
		UIHelper.EnableGameObject(TranscendLabel, CachedHeroInfo.Transcended > 0);
		UIHelper.SetLabelText(TranscendLabel, "+" + CachedHeroInfo.Transcended);
		HeroProfileData data2 = GameRoot.Instance.HeroProfileDataContainer.GetData(CachedHeroInfo.HeroIndex);
		if (data2 != null && !String_Extension_Method.IsNullOrEmpty(data2.HeroName))
		{
			UIHelper.SetLabelText(NameLabel, LocalizationManager.GetStringDirect(data2.HeroName));
		}
		else
		{
			string text = data.Name;
			UIHelper.SetLabelText(NameLabel, text);
		}
		int maxLevel = GameRoot.Instance.CreatureLevelDataContainer.MaxLevel;
		UIHelper.SetLabelText(LevelLabel, CachedHeroInfo.Level);
		UIHelper.SetLabelColor(LevelLabel, (CachedHeroInfo.Level > maxLevel) ? LimitBreakLabelColor : Color.white);
		UIHelper.EnableGameObject(LimitBreakLevelLabel, CachedHeroInfo.Level > maxLevel);
		int maxLevel2 = CachedHeroInfo.GetMaxLevel();
		UIHelper.SetLabelText(MaxLevelLabel, maxLevel2);
		UIHelper.SetLabelColor(MaxLevelLabel, (maxLevel2 > maxLevel) ? LimitBreakLabelColor : Color.white);
		UIHelper.EnableGameObject(LimitBreakMaxLevelLabel, maxLevel2 > maxLevel);
	}

	public void SetupCameraZoom()
	{
		if (Toggle_CameraZoom_In_Out != null)
		{
			if (_heroPreview == null)
			{
				Toggle_CameraZoom_In_Out.value = true;
				return;
			}
			_heroPreview.DefaultOrtho.ZoomOutRate = 0f;
			Toggle_CameraZoom_In_Out.value = _heroPreview.DefaultOrtho.ZoomOutRate == 0f;
			OnToggleCameraZoom();
		}
	}

	public void SetToggle(EToggle toggle)
	{
		if (!(ToggleInfo == null))
		{
			ToggleInfo.Set(toggle == EToggle.Info);
			ToggleEquip.Set(toggle == EToggle.Equip);
			ToggleSkill.Set(toggle == EToggle.Skill);
			ToggleAttr.Set(toggle == EToggle.Attribute);
			ToggleProfile.Set(toggle == EToggle.Profile);
			Toggle_OpenEquipStorageSlot.Set(toggle == EToggle.EquipStorage);
			_toggle = toggle;
			if (_listener != null)
			{
				_listener.OnToggle(toggle);
			}
		}
	}

	private void SetupHeroExp()
	{
		if (!(Sprite_ExpGaugeBack == null) && !(Sprite_ExpGauge == null) && !(Label_HeroExpPercentage == null))
		{
			int maxLevel = CachedHeroInfo.GetMaxLevel();
			bool num = CachedHeroInfo.Level >= maxLevel;
			int exp = CachedHeroInfo.Exp;
			Helper.GetHeroExpForNextLevel(CachedHeroInfo, out var expForNextLevel);
			if (num || expForNextLevel == 0L)
			{
				UIHelper.SetLabelText(Label_HeroExpPercentage, "Max");
				UIHelper.SetSpriteFillAmount(Sprite_ExpGauge, 1f);
			}
			else
			{
				float num2 = Mathf.Clamp(100f * (float)exp / (float)expForNextLevel, 0f, 100f);
				UIHelper.SetLabelText(Label_HeroExpPercentage, $"{(int)num2}%");
				UIHelper.SetSpriteFillAmount(Sprite_ExpGauge, num2 * 0.01f);
			}
		}
	}

	public void SetupStar()
	{
		if (CachedHeroInfo != null && !__Extension_Method_ICollection__.IsNullOrEmpty(StarList) && !__Extension_Method_ICollection__.IsNullOrEmpty(TranscendStarList))
		{
			int num = 0;
			int num2 = 0;
			num = ((CachedHeroInfo.Transcended <= 0) ? CachedHeroInfo.Star : 0);
			num2 = CachedHeroInfo.Transcended;
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(StarList); i++)
			{
				int num3 = i + 1;
				UIHelper.EnableGameObject(StarList[i], num == num3 && num2 == 0);
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(TranscendStarList); j++)
			{
				int num4 = j + 1;
				UIHelper.EnableGameObject(TranscendStarList[j], num2 == num4);
			}
		}
	}

	private void SetupFlask()
	{
		ClearFlask();
		if (CachedHeroInfo.FlaskItemIndex <= 0)
		{
			UIHelper.EnableGameObject(NoFlaskRoot, enable: true);
			if (GlobalAutoBehaviour<AccountManager>.instance.ItemManager.GetItemsByType(ItemType.Flask).Count == 0)
			{
				UIHelper.EnableGameObject(EquipFlaskSprite, enable: false);
				UIHelper.EnableGameObject(NoHaveFlask, enable: true);
			}
			else
			{
				UIHelper.EnableGameObject(EquipFlaskSprite, enable: true);
				UIHelper.EnableGameObject(NoHaveFlask, enable: false);
			}
			return;
		}
		int flaskItemIndex = CachedHeroInfo.FlaskItemIndex;
		ItemData<FlaskItemData> data = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(flaskItemIndex);
		if (data == null)
		{
			VespaDebug.LogError("FlaskItemData not found. index=" + flaskItemIndex);
			return;
		}
		int flaskExp = data.SubData.FlaskExp;
		if (flaskExp == 0)
		{
			VespaDebug.LogError("FlaskItemData FlaskExp is 0. index=" + flaskItemIndex);
			return;
		}
		ExpGaugeSelector.Select(data.SubData.Type.ToString());
		int num = Mathf.Clamp(CachedHeroInfo.FlaskExp * 100 / flaskExp, 0, 100);
		UIHelper.EnableGameObject(UsingFlaskRoot, enable: true);
		UIHelper.SetLabelText(Label_FlaskPercentage, $"{num}%");
		UIHelper.SetSpriteFillAmount(Sprite_FlaskGauge, (float)num / 100f);
		if (flaskSelector != null)
		{
			UIHelper.EnableGameObject(flaskSelector, enable: true);
			flaskSelector.AssetPath = UIHelper.GetItemIconPath(data.Code);
		}
	}

	private void ClearFlask()
	{
		UIHelper.EnableGameObject(UsingFlaskRoot, enable: false);
		UIHelper.EnableGameObject(NoFlaskRoot, enable: false);
	}

	private void SetupError(string msg)
	{
		GameObject anchor = null;
		NameLabel.SetAnchor(anchor);
		NameLabel.overflowMethod = UILabel.Overflow.ResizeFreely;
		UIHelper.SetLabelText(NameLabel, msg);
	}

	private void InitPreview()
	{
		if (!(HeroPreviewTexture == null) && CachedHeroInfo != null)
		{
			RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, CachedHeroInfo);
			renderFacadeBuilder.SetPreload(AnimationKeyData.EmptyPreloadSet);
			UIHelper.InitHero3dPreview(ref _heroPreview, HeroPreviewTexture, CachedHeroInfo.HeroIndex, CachedHeroInfo.CostumeIndex, renderFacadeBuilder, "Normal", perspective: false);
		}
	}

	private void ClosePreview()
	{
		UIHelper.CloseHero3dPreview(ref _heroPreview, HeroPreviewTexture);
	}

	public void ActivatePreview(bool isActive)
	{
		if (!isActive)
		{
			ClosePreview();
		}
		else
		{
			InitPreview();
		}
	}

	public void Reset()
	{
		if (!base.gameObject.activeSelf)
		{
			ClosePreview();
		}
		ClearClassTagIconList();
		ClearEquipItemIconList();
		ClearNotifyList();
		_listener = null;
	}

	public void Close()
	{
		base.gameObject.SetActive(value: false);
		GlobalAutoBehaviour<AccountManager>.instance.EquipPresetSotrageManger.ObserverManager.RemoveObserver(this);
		Reset();
	}

	public void OnToggleChanged()
	{
		EToggle eToggle = EToggle.None;
		if (ToggleInfo.value)
		{
			eToggle = EToggle.Info;
		}
		else if (ToggleEquip.value)
		{
			eToggle = EToggle.Equip;
		}
		else if (ToggleSkill.value)
		{
			eToggle = EToggle.Skill;
		}
		else if (ToggleAttr.value)
		{
			eToggle = EToggle.Attribute;
		}
		else if (ToggleProfile.value)
		{
			eToggle = EToggle.Profile;
		}
		else if (Toggle_OpenEquipStorageSlot.value)
		{
			eToggle = EToggle.EquipStorage;
		}
		if (eToggle != EToggle.None && _toggle != eToggle)
		{
			_toggle = eToggle;
			if (_listener != null)
			{
				_listener.OnToggle(_toggle);
			}
			SingletonUpdater<GCManager>.instance.IPhoneLowMemCollectLite();
		}
	}

	public void OnClickDetailView()
	{
		if (_listener != null)
		{
			_listener.OnClickDetailView();
		}
	}

	public void OnClickAutoEquip()
	{
		if (_listener != null)
		{
			_listener.OnClickAutoEquip();
		}
	}

	public void OnClickSelectFlask()
	{
		if (_listener != null)
		{
			_listener.OnClickSelectFlask(this);
		}
	}

	public void OnToggleCameraZoom()
	{
		if (_cameraState == ECameraState.None && !(_heroPreview == null) && !(Toggle_CameraZoom_In_Out == null))
		{
			if (!Toggle_CameraZoom_In_Out.value)
			{
				_cameraState = ECameraState.ZoomOut;
			}
			else
			{
				_cameraState = ECameraState.ZoomIn;
			}
		}
	}

	public void OnClickShowUniqueWeaponOn()
	{
		if (_listener != null)
		{
			_listener.OnClickShowUniqueWeaponOn(this);
		}
	}

	public void OnClickShowUniqueWeaponOff()
	{
		if (_listener != null)
		{
			_listener.OnClickShowUniqueWeaponOff(this);
		}
	}

	public void Update()
	{
		if (_skillPopupDestroyTime > 0f && Time.time >= _skillPopupDestroyTime)
		{
			_skillPopupDestroyTime = 0f;
			UIHelper.EnableGameObject(SkillPopupRoot, enable: false);
		}
		if (_heroPreview != null && _heroPreview.DefaultOrtho != null)
		{
			switch (_cameraState)
			{
			case ECameraState.ZoomIn:
				_heroPreview.DefaultOrtho.ZoomOutRate -= Time.deltaTime * ZoomSpeed;
				_heroPreview.DefaultOrtho.ZoomOutRate = System.Math.Max(0f, _heroPreview.DefaultOrtho.ZoomOutRate);
				break;
			case ECameraState.ZoomOut:
				_heroPreview.DefaultOrtho.ZoomOutRate += Time.deltaTime * ZoomSpeed;
				_heroPreview.DefaultOrtho.ZoomOutRate = System.Math.Min(1f, _heroPreview.DefaultOrtho.ZoomOutRate);
				break;
			}
			if (_heroPreview.DefaultOrtho.ZoomOutRate <= 0f || _heroPreview.DefaultOrtho.ZoomOutRate >= 1f)
			{
				_cameraState = ECameraState.None;
			}
		}
		UpdateNotify();
	}

	private void ClearClassTagIconList()
	{
		FactoryBase.ReleaseAndClearList(_classTagIconComponentList);
	}

	private void SetupClassTagIconList()
	{
		ClearClassTagIconList();
		if (ClassTagGrid == null)
		{
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(CachedHeroInfo.HeroIndex);
		if (data == null)
		{
			return;
		}
		CreatureTagType tagType = data.TagType;
		if (tagType != CreatureTagType.None)
		{
			ClassTagIconComponent classTagIconComponent = UIHelper.AddGridItem(ClassTagGrid, ClassTagIconComponent.PrefabAuto);
			if (classTagIconComponent != null)
			{
				_classTagIconComponentList.Add(classTagIconComponent);
				classTagIconComponent.Open(tagType, this);
			}
			UIHelper.RepositionGrid(ClassTagGrid);
		}
	}

	private void ClearNotifyList()
	{
		FactoryBase.ReleaseAndClearList(_notifyComponentList);
	}

	private bool CheckUpdateNeeded(IList<HeroNotifyComponent.ENotifyInfo> ListNotifyInfo)
	{
		if (_notifyComponentList.Count == ListNotifyInfo.Count)
		{
			for (int i = 0; i < _notifyComponentList.Count; i++)
			{
				if (_notifyComponentList[i].DisplayNotifyInfo != ListNotifyInfo[i])
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	private void UpdateNotify()
	{
		if (base.gameObject.activeSelf && CachedHeroInfo != null)
		{
			List<HeroNotifyComponent.ENotifyInfo> heroNotifyInfoList = SceneAutoBehaviour<HeroNotifyContainer>.instance.GetHeroNotifyInfoList(CachedHeroInfo.HeroIndex);
			UIHelper.EnableGameObject(Sprite_CanSkillLevelUp, heroNotifyInfoList.Contains(HeroNotifyComponent.ENotifyInfo.CanSkillExtend));
			UIHelper.EnableGameObject(Sprite_CanTranscended, heroNotifyInfoList.Contains(HeroNotifyComponent.ENotifyInfo.TrialChallengable));
			UIHelper.EnableGameObject(Sprite_CanLimitBreak, heroNotifyInfoList.Contains(HeroNotifyComponent.ENotifyInfo.CanLimitBreak));
		}
	}

	void ClassTagIconComponent.Listener.OnClick(ClassTagIconComponent caller)
	{
		if (_listener != null)
		{
			_listener.OnClickClassTagIcon(caller);
		}
	}

	public void SetupEquipItemIcon()
	{
		EquipImpl.SetupEquipItemIcon();
	}

	private void ClearEquipItemIconList()
	{
		EquipImpl.Clear();
	}

	public int GetHeroPartEquipSlotIndex(ItemIconComponent itemIconComponent)
	{
		return EquipImpl.GetHeroPartEquipSlotIndex(itemIconComponent);
	}

	public void SetupSkillIcon()
	{
		GameRoot.Instance.CreatureDataContainer.GetData(CachedHeroInfo.HeroIndex);
	}

	public void ShowSkillInfo(int slotIndex, SkillData skillData, int skillLevel, float duration)
	{
		UIHelper.EnableGameObject(SkillPopupRoot, enable: false);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo equipStatInfo)
	{
		UpdateEquipNewState();
		if (equipStatInfo == null)
		{
			return;
		}
		EquipItemInfo equipItemInfo = equipStatInfo.EquipItemInfo;
		if (equipItemInfo == null)
		{
			return;
		}
		ItemIconComponent itemIconComponent = EquipImpl.FindEquip((ItemIconComponent e) => e.EquipItemInfo != null && e.EquipItemInfo.SlotIndex == equipItemInfo.SlotIndex);
		if (itemIconComponent == null)
		{
			EquipItemInfo[] equipItemInfos = CachedHeroInfo.GetEquipItemInfos();
			if (equipItemInfos != null && equipItemInfos.Contains(equipItemInfo))
			{
				SetupEquipItemIcon();
			}
		}
		else
		{
			itemIconComponent.Refresh();
		}
	}

	void IUserObserver<NGame2.NAccount.UserItemManager.ItemCountChangedInfo>.OnChanged(NGame2.NAccount.UserItemManager.ItemCountChangedInfo info)
	{
		UpdateEquipNewState();
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		SetupHeroInfo();
		if (changedInfo.HeroIndex == CachedHeroInfo.HeroIndex)
		{
			if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.ResetAllTranscendSkill || changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.TranscendSkillSelected || changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.AddTranscendSkillPage)
			{
				SetTransendAttrNewTag();
			}
			if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.HeroExpup)
			{
				SetupHeroExp();
			}
		}
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.FlaskItemIndex || changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.FlaskExp)
		{
			SetupFlask();
		}
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Star || changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.GrowthStar)
		{
			SetupStar();
		}
	}

	private void UpdateEquipNewState()
	{
		bool enable = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.IsNewEquippableOnHero(CachedHeroInfo);
		UIHelper.EnableGameObject(Sprite_EquipNew, enable);
	}

	private void SetTransendAttrNewTag()
	{
		UIHelper.EnableGameObject(Sprite_AttrNew, CachedHeroInfo != null && CanLearnTranscendSkill());
	}

	private bool CanLearnTranscendSkill()
	{
		for (int i = 1; i <= 3; i++)
		{
			if (CachedHeroInfo.GetTranscendSkillPageInfo(i) == null)
			{
				continue;
			}
			int usableTranscendSkillPoint = CachedHeroInfo.GetUsableTranscendSkillPoint(i);
			if (usableTranscendSkillPoint == 0)
			{
				continue;
			}
			CreatureData creatureData = CachedHeroInfo.CreatureData;
			int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxTranscend");
			int num = System.Math.Min(CachedHeroInfo.Transcended, valueInt);
			for (int j = 1; j <= num; j++)
			{
				int[] transcendSkill = creatureData.GetTranscendSkill(j);
				if (transcendSkill == null)
				{
					continue;
				}
				for (int k = 0; k < __Extension_Method_ICollection__.COUNT(transcendSkill); k++)
				{
					int skillIndex = transcendSkill[k];
					if (!CachedHeroInfo.IsLearnTranscendSkill(i, skillIndex))
					{
						CreatureData.TranscendSkill transcendSkillData = creatureData.GetTranscendSkillData(j, k);
						if (transcendSkillData != null && transcendSkillData.ReqSKillPoint <= usableTranscendSkillPoint)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	void HeroNotifyComponent.Listener.OnClickComponent(HeroNotifyComponent.ENotifyInfo notifyInfo)
	{
		if (_listener != null)
		{
			_listener.OnClickHeroNotify(notifyInfo, CachedHeroInfo);
		}
	}

	public void OnClickCostumeShortCut()
	{
		int heroIndex = ((CachedHeroInfo == null) ? 1 : CachedHeroInfo.HeroIndex);
		if (_listener != null)
		{
			_listener.OnClickCostume(heroIndex);
		}
	}

	public void OnClickProfile()
	{
		if (_listener != null)
		{
			int heroIndex = ((CachedHeroInfo == null) ? 1 : CachedHeroInfo.HeroIndex);
			_listener.OnClickProfile(heroIndex);
		}
	}

	public void OnClickPresetStorage()
	{
		if (_listener != null)
		{
			_listener.OnClickPresetStorage();
		}
	}
}
