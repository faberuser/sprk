using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NStringProxy;
using NGame2.NUtil.NStringProxy.NTypeText;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroSkillSubComponent : UIScrollItemBehaviour, InitOnCreate, SkillSubComponent
{
	public interface Listener
	{
		void OnSelectSkillComponent(HeroSkillSubComponent comp);

		void OnClickSwitchLinkedSkill(HeroSkillSubComponent comp);
	}

	[Serializable]
	public class ExtendSkillGroup
	{
		public UISprite[] IconBorders;

		public UITexture Texture_SkillIcon;

		public UISprite Sprite_CannotLearn;

		public UISprite Sprite_CanLearn;
	}

	public const string PrefabPath = "Prefab2/Component/Hero/HeroSkillSubComponent";

	public static PrefabAuto<HeroSkillSubComponent> PrefabAuto = new PrefabAuto<HeroSkillSubComponent>("Prefab2/Component/Hero/HeroSkillSubComponent");

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UITexture _iconTexture;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _typeLabel;

	[SerializeField]
	private UISprite _levelUpAlarm;

	[SerializeField]
	private UISprite[] CoreSkillIconBorders;

	[SerializeField]
	private UISprite _Sprite_Arrow_2to3_on;

	[SerializeField]
	private UISprite _Sprite_Arrow_3to4_on;

	[SerializeField]
	private UISprite _Sprite_SkillFrame;

	[SerializeField]
	private UISprite _Sprite_SkillFrame_NotLearned;

	[SerializeField]
	private UIGame3DWidget _effectSkillLevelUp;

	public ExtendSkillGroup[] ExtendSkills;

	public UISprite Sprite_Selected;

	public SkillTooltipTextInfo TooltipTextInfo;

	public UIButton SwitchLinkedSkillButton;

	private Listener _listener;

	private bool _isMine;

	private CreatureData _creatureData;

	public HeroInfo HeroInfo { get; set; }

	public int SkillSlotIndex { get; set; }

	public int LinkedIndex { get; private set; }

	void InitOnCreate.OnCreate()
	{
		LinkedIndex = 0;
	}

	void InitOnCreate.OnRelease()
	{
		_ = _effectSkillLevelUp != null;
		LinkedIndex = 0;
		HeroInfo = null;
		SkillSlotIndex = 0;
		_creatureData = null;
	}

	public bool Open(HeroInfo heroInfo, int skillSlotIndex, Listener listener, bool isMine = true)
	{
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(heroInfo, skillSlotIndex);
		return Open(heroInfo, skillDetailInfo, skillSlotIndex, listener, isMine);
	}

	public bool Open(HeroInfo heroInfo, Helper.SkillDetailInfo skillInfo, int skillSlotIndex, Listener listener, bool isMine = true)
	{
		_listener = listener;
		_isMine = isMine;
		HeroInfo = heroInfo;
		SkillSlotIndex = skillSlotIndex;
		_creatureData = GameRoot.Instance.CreatureDataContainer.GetData(HeroInfo.HeroIndex);
		if (skillInfo.BasicInfo.TableData == null)
		{
			return false;
		}
		LinkedIndex = 0;
		UIHelper.EnableGameObject(_Sprite_SkillFrame, !skillInfo.BasicInfo.IsLocked);
		UIHelper.EnableGameObject(_Sprite_SkillFrame_NotLearned, skillInfo.BasicInfo.IsLocked);
		for (int i = 0; i < CoreSkillIconBorders.Length; i++)
		{
			UIHelper.EnableGameObject(CoreSkillIconBorders[i], i + 1 == SkillSlotIndex);
		}
		UIHelper.SetLabelText(_typeLabel, BaseSkillComponent.GetSkillTypeText(skillInfo.BasicInfo));
		bool enable = _creatureData.HasLinkedSkill(skillSlotIndex);
		UIHelper.EnableGameObject(SwitchLinkedSkillButton, enable);
		SetSkillInfo(skillInfo);
		SetSkillExtend(skillInfo);
		SetSkillLevel(skillInfo);
		SetupTooltipText();
		SetSelected(Select: false);
		return true;
	}

	private void SetSkillInfo(Helper.SkillDetailInfo info)
	{
		AssetTexture assetTexture = null;
		assetTexture = UIHelper.GetSkillIcon2(info.BasicInfo.TableIndex, HeroInfo);
		UIHelper.ResetPlaneAssetTextureAsync(this, _iconTexture, assetTexture);
		string skillName = SkillDescContainer.GetSkillName(HeroInfo, info.BasicInfo.TableData);
		UIHelper.SetLabelText(_nameLabel, skillName);
	}

	public void ShowSkillLevel(bool isShow)
	{
		UIHelper.EnableGameObject(_levelLabel, isShow);
	}

	public void SetSelected(bool Select)
	{
		UIHelper.EnableGameObject(Sprite_Selected, Select);
	}

	private void SetArrowSprite(UISprite SpriteArrowOn, bool CanLearn, bool AlreadyLearn)
	{
		if (SpriteArrowOn == null)
		{
			return;
		}
		bool enable = CanLearn || AlreadyLearn;
		UIHelper.EnableGameObject(SpriteArrowOn, enable);
		TweenAlpha component = SpriteArrowOn.GetComponent<TweenAlpha>();
		if (component != null)
		{
			component.enabled = CanLearn;
			if (!CanLearn && AlreadyLearn)
			{
				SpriteArrowOn.alpha = 1f;
			}
		}
	}

	private void SetupExtendSkill(ExtendSkillGroup SkillGroup, Helper.SkillExtendInfo? info, bool textureChange = true)
	{
		if (SkillGroup == null || !info.HasValue)
		{
			return;
		}
		Helper.SkillExtendInfo valueOrDefault = info.GetValueOrDefault();
		if (valueOrDefault.TableData != null)
		{
			int index = valueOrDefault.TableData.Index;
			if (textureChange)
			{
				AssetTexture skillIcon = UIHelper.GetSkillIcon2(index, HeroInfo);
				UIHelper.ResetPlaneAssetTextureAsync(this, SkillGroup.Texture_SkillIcon, skillIcon);
			}
			bool enable = _isMine && valueOrDefault.CanLearn;
			UIHelper.EnableGameObject(SkillGroup.Sprite_CanLearn, enable);
			if (_isMine)
			{
				UIHelper.EnableGameObject(SkillGroup.Sprite_CannotLearn, valueOrDefault.CannotLearnNoExtended);
			}
			else
			{
				UIHelper.EnableGameObject(SkillGroup.Sprite_CannotLearn, valueOrDefault.ExtendStatus != Helper.SkillExtendInfo.EExtendStatus.AlreadyExtended);
			}
		}
	}

	public void ActiveForceSkillGroup()
	{
		ExtendSkillGroup[] extendSkills = ExtendSkills;
		for (int i = 0; i < extendSkills.Length; i++)
		{
			UIHelper.EnableGameObject(extendSkills[i].Sprite_CannotLearn, enable: false);
		}
	}

	public void PlayLevelUpEffect()
	{
		if (_effectSkillLevelUp != null)
		{
			_effectSkillLevelUp.Play(null);
		}
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnSelectSkillComponent(this);
		}
	}

	public void OnShowTooltip()
	{
		SkillIconTooltip.Open(_iconTexture, TooltipTextInfo);
	}

	public void OnHideTooltip()
	{
		SkillIconTooltip.Hide(_iconTexture);
	}

	private void SetupTooltipText()
	{
		Helper.SkillBasicInfo skillBasicInfo = Helper.GetSkillBasicInfo(HeroInfo, SkillSlotIndex);
		if (skillBasicInfo.TableIndex != 0)
		{
			if (TooltipTextInfo == null)
			{
				TooltipTextInfo = new SkillTooltipTextInfo();
			}
			TooltipTextInfo.Reset();
			TooltipTextInfo.Name.Value = SkillDescContainer.GetSkillName(HeroInfo, skillBasicInfo.TableData);
			TooltipTextInfo.Desc.Assign(new SkillDescBuilder(skillBasicInfo.TableIndex, HeroInfo, appendLine: false));
			TooltipTextInfo.TypeText.Assign(new SkillBuilder(skillBasicInfo, appendLine: false));
			TooltipTextInfo.SkillLevel = skillBasicInfo.Level;
			TooltipTextInfo.CooltimeMs = ((skillBasicInfo.TableData != null) ? skillBasicInfo.TableData.CoolTimeMs : 0);
			TooltipTextInfo.isActiveSkill = skillBasicInfo.TableData.TriggerType == SkillTriggerType.None;
			StatData heroStat = StatCalculator.GetHeroStat(HeroInfo, new StatType[1] { StatType.MaxMp });
			int reqMp = ((skillBasicInfo.TableData != null) ? skillBasicInfo.TableData.Mp : 0);
			TooltipTextInfo.InitReqManaGauge(reqMp, heroStat.MaxMp);
		}
	}

	public void SetSkillExtend(int skillExtend)
	{
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(HeroInfo, SkillSlotIndex);
		SetSkillExtend(skillDetailInfo, textureChange: false);
	}

	private void SetSkillExtend(Helper.SkillDetailInfo skillInfo, bool textureChange = true)
	{
		GameRoot.Instance.CreatureDataContainer.GetData(HeroInfo.HeroIndex);
		int skillExtend = HeroInfo.GetSkillExtend(SkillSlotIndex);
		if (skillInfo.ExtendInfos != null)
		{
			int num = Math.Min(skillInfo.ExtendInfos.Length, ExtendSkills.Length);
			for (int i = 0; i < num; i++)
			{
				SetupExtendSkill(ExtendSkills[i], skillInfo.ExtendInfos[i], textureChange);
			}
		}
		bool canLearn = skillExtend == 1 && skillInfo.ExtendInfos[1].ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.Extendable;
		bool canLearn2 = skillExtend == 2 && skillInfo.ExtendInfos[2].ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.Extendable;
		SetArrowSprite(_Sprite_Arrow_2to3_on, canLearn, skillExtend >= 2);
		SetArrowSprite(_Sprite_Arrow_3to4_on, canLearn2, skillExtend >= 3);
	}

	public void SetSkillLevel(int skillLevel)
	{
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(HeroInfo, SkillSlotIndex);
		SetSkillLevel(skillDetailInfo);
	}

	private void SetSkillLevel(Helper.SkillDetailInfo skillInfo)
	{
		int num = skillInfo.BasicInfo.Level;
		if (HeroInfo is CachedHeroInfo heroInfo)
		{
			int equipOptionAddedSkillLevel = StatCalculator.GetEquipOptionAddedSkillLevel(heroInfo, SkillSlotIndex);
			num += equipOptionAddedSkillLevel;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("ReqHeroLevelSkillSlot" + SkillSlotIndex);
		string text = (skillInfo.BasicInfo.IsLocked ? ("[FF6600]" + LocalizationManager.GetString(HeroSkill.GET_SKILL_HERO_LEVEL, valueInt) + "[-]") : ("[FFFF66]" + LocalizationManager.GetString(HeroStat.STAT_LEVEL) + " " + num + "[-]"));
		UIHelper.SetLabelText(_levelLabel, text);
	}

	public void SetExternalSkillLevel(string skillLevel)
	{
		UIHelper.SetLabelText(_levelLabel, skillLevel);
	}

	public void Refresh()
	{
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(HeroInfo, SkillSlotIndex);
		Open(HeroInfo, skillDetailInfo, SkillSlotIndex, _listener, _isMine);
	}

	private void SetLevelUpAlarm(Helper.SkillDetailInfo skillInfo)
	{
		UIHelper.EnableGameObject(_levelUpAlarm, enable: false);
	}

	public void OnSwitchLinkedSkill()
	{
		if (HeroInfo == null)
		{
			return;
		}
		int[] linkedSkillIndexBySlot = _creatureData.GetLinkedSkillIndexBySlot(SkillSlotIndex);
		if (__Extension_Method_ICollection__.COUNT(linkedSkillIndexBySlot) > 0)
		{
			int linkedIndex = LinkedIndex;
			linkedIndex = ((__Extension_Method_ICollection__.COUNT(linkedSkillIndexBySlot) >= linkedIndex + 1) ? (linkedIndex + 1) : 0);
			SetSkillBasicInfo(linkedIndex);
			if (_listener != null)
			{
				_listener.OnClickSwitchLinkedSkill(this);
			}
		}
	}

	private void SetSkillBasicInfo(int linkedSkillIndex)
	{
		LinkedIndex = linkedSkillIndex;
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(HeroInfo, SkillSlotIndex, linkedSkillIndex);
		SetSkillInfo(skillDetailInfo);
		SetSkillExtend(skillDetailInfo);
	}
}
