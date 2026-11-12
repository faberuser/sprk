using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NDescription;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[UIEvent(AllowDuringPatchDownloading = true)]
public class HeroSkillDesc : BaseWindow, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<ItemInfo>
{
	public interface Listener
	{
		void LearnSkill(int SkillSlotIndex);

		void ExtendSkill(int SkillSlotIndex, int ExtendSlot, bool isEnoughItem);
	}

	[Serializable]
	public class SkillManaUiGroup
	{
		public UIWidget TriggerSkillRoot;

		public UILabel TriggerSkillType;

		public UIWidget ReqManaRoot;

		public GameObject ManaGaugeTemplate;

		public UIGrid ReqManaGrid;

		public UIWidget NoManaRoot;

		private List<GameObject> _manaGaugeList = new List<GameObject>();

		public void Setup(HeroInfo heroInfo, Helper.SkillDetailInfo skillInfo)
		{
			ClearManaGauge();
			bool flag = skillInfo.BasicInfo.TableData.TriggerType != SkillTriggerType.None;
			UIHelper.EnableGameObject(TriggerSkillRoot, flag);
			if (flag)
			{
				UIHelper.EnableGameObject(ReqManaRoot, enable: false);
				UIHelper.EnableGameObject(NoManaRoot, enable: false);
				UIHelper.SetLabelText(TriggerSkillType, BaseSkillComponent.GetSkillTypeText(skillInfo.BasicInfo));
				return;
			}
			StatData heroStat = StatCalculator.GetHeroStat(heroInfo, new StatType[1] { StatType.MaxMp });
			int reqManaGauge = SkillTooltipTextInfo.GetReqManaGauge((skillInfo.BasicInfo.TableData != null) ? skillInfo.BasicInfo.TableData.Mp : 0, heroStat.MaxMp);
			if (reqManaGauge == 0)
			{
				UIHelper.EnableGameObject(ReqManaRoot, enable: false);
				UIHelper.EnableGameObject(NoManaRoot, enable: true);
				return;
			}
			UIHelper.EnableGameObject(ReqManaRoot, enable: true);
			UIHelper.EnableGameObject(NoManaRoot, enable: false);
			for (int i = 0; i < reqManaGauge; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(ManaGaugeTemplate);
				Vespa_Unity_Transform_Extension_Method.ParentResetTM(gameObject.transform, ReqManaGrid.transform);
				gameObject.SetActive(value: true);
				_manaGaugeList.Add(gameObject);
			}
			ManaGaugeTemplate.SetActive(value: false);
			ReqManaGrid.Reposition();
		}

		public void ClearManaGauge()
		{
			foreach (GameObject manaGauge in _manaGaugeList)
			{
				VespaObject.DestroyGoAndDetachFromParent(manaGauge.gameObject);
			}
			_manaGaugeList.Clear();
		}
	}

	[Serializable]
	public class SkillBookUiGroup
	{
		public UISimpleGrid BookGrid;

		private List<UpgradeBookItemIconComponent> _bookComponentList = new List<UpgradeBookItemIconComponent>();

		public void Setup(HeroInfo heroInfo)
		{
			for (int i = 0; i < 4; i++)
			{
				UpgradeBookItemIconComponent upgradeBookItemIconComponent = UIHelper.AddGridItem(BookGrid, UpgradeBookItemIconComponent.PrefabAuto);
				Helper.SkillBookInfo skillBookInfo = Helper.GetSkillBookInfo(heroInfo, i + 1);
				upgradeBookItemIconComponent.Open(skillBookInfo.BookItemData, skillBookInfo.CurItemCount, skillBookInfo.CreatureTagType, i + 1);
				_bookComponentList.Add(upgradeBookItemIconComponent);
			}
			UIHelper.RepositionGrid(BookGrid);
		}

		public void Clear()
		{
			FactoryBase.ReleaseAndClearList(_bookComponentList);
		}

		public void OnChanged(ItemInfo itemInfo)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemInfo.ItemIndex);
			if (data == null || data.Type != ItemType.UpgradeBook)
			{
				return;
			}
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_bookComponentList); i++)
			{
				UpgradeBookItemIconComponent upgradeBookItemIconComponent = _bookComponentList[i];
				if (Vespa_Common_Class_Extension_Method.EQUALS(data, _bookComponentList[i].ItemData))
				{
					upgradeBookItemIconComponent.SetItemCount(itemInfo.Count, CountLabelTween: true);
				}
			}
		}
	}

	[Serializable]
	public class ExtendSkillUiGroup
	{
		public UIBundleLoader SkillIconBundleTex;

		public UISprite Sprite_CanLearn;

		public UISprite Sprite_CannotLearn;

		public UIGame3DWidget Widget_Effect;

		public UILabel Label_Desc;

		public UIButton Button_Learn;

		public UIWidget WidgetLearn;

		public UIBundleLoader BookIconBundleTex;

		public UILabel Label_ItemCount;

		public UILabel Label_CannotLearn;

		public UILabel Label_AlreadyLearned;

		public UISprite Sprite_Off;

		public Helper.SkillExtendInfo ExtendInfo { get; private set; }

		public void Setup(HeroInfo heroInfo, Helper.SkillExtendInfo extendInfo, string bookIconPath)
		{
			ExtendInfo = extendInfo;
			if (extendInfo.TableData != null)
			{
				int index = extendInfo.TableData.Index;
				string skillIconPath = UIHelper.GetSkillIconPath(index, heroInfo);
				UIHelper.SetBundleTexture(SkillIconBundleTex, skillIconPath);
				string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(index, heroInfo);
				UIHelper.SetLabelText(Label_Desc, byHeroInfo);
				if (extendInfo.ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.Extendable)
				{
					UIHelper.EnableGameObject(Sprite_CannotLearn, enable: false);
					UIHelper.EnableGameObject(Sprite_Off, enable: false);
					UIHelper.EnableGameObject(WidgetLearn, enable: true);
					UIHelper.EnableGameObject(Label_CannotLearn, enable: false);
					UIHelper.EnableButton(Button_Learn, enable: true);
					SetupSkillBook(extendInfo, bookIconPath);
				}
				else if (extendInfo.ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.AlreadyExtended)
				{
					UIHelper.EnableGameObject(Sprite_CannotLearn, enable: false);
					UIHelper.EnableGameObject(Sprite_Off, enable: false);
					UIHelper.EnableGameObject(WidgetLearn, enable: false);
					UIHelper.EnableGameObject(Label_CannotLearn, enable: false);
					UIHelper.EnableButton(Button_Learn, enable: false);
				}
				else if (extendInfo.ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.PrevExtendNeeded)
				{
					UIHelper.EnableGameObject(Sprite_CannotLearn, enable: true);
					UIHelper.EnableGameObject(Sprite_Off, enable: true);
					UIHelper.EnableGameObject(WidgetLearn, enable: false);
					UIHelper.EnableGameObject(Label_CannotLearn, enable: true);
					UIHelper.EnableButton(Button_Learn, enable: false);
				}
				else if (extendInfo.ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.NotEnoughItem)
				{
					UIHelper.EnableGameObject(Sprite_CannotLearn, enable: true);
					UIHelper.EnableGameObject(Sprite_Off, enable: false);
					UIHelper.EnableGameObject(WidgetLearn, enable: true);
					UIHelper.EnableGameObject(Label_CannotLearn, enable: false);
					UIHelper.EnableButton(Button_Learn, enable: true);
					SetupSkillBook(extendInfo, bookIconPath);
				}
				else if (extendInfo.ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.NotLearnedCoreSkill)
				{
					UIHelper.EnableGameObject(Sprite_CannotLearn, enable: true);
					UIHelper.EnableGameObject(Sprite_Off, enable: true);
					UIHelper.EnableGameObject(WidgetLearn, enable: false);
					UIHelper.EnableGameObject(Label_CannotLearn, enable: true);
					UIHelper.EnableButton(Button_Learn, enable: false);
				}
				else
				{
					VespaDebug.LogError("Not Implemented Yet: " + Vespa_String_Extension_Method.GetEnumString(extendInfo.ExtendStatus));
				}
				UIHelper.EnableGameObject(Sprite_CanLearn, extendInfo.CanLearn);
				UIHelper.EnableGameObject(Label_AlreadyLearned, extendInfo.ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.AlreadyExtended);
			}
		}

		private void SetupSkillBook(Helper.SkillExtendInfo extendInfo, string bookIconPath)
		{
			UIHelper.SetBundleTexture(BookIconBundleTex, bookIconPath);
			string text = ((extendInfo.ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.NotEnoughItem) ? "[FF6464]" : "[FFC800]") + extendInfo.ExtendItemCount + "[-]";
			UIHelper.SetLabelText(Label_ItemCount, text);
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Hero/HeroSkillDesc";

	public UILabel Label_SkillName;

	public UILabel Label_SkillLevel;

	public UILabel Label_SkillDesc;

	public UIScrollView ScrollView_SkillDesc;

	public UIScreenEffectPlayer SkillLevelUpEffect;

	public SkillManaUiGroup SkillManaUi = new SkillManaUiGroup();

	public UILabel Label_CoolTime;

	public UISimpleGrid SkillIconGrid;

	public SkillBookUiGroup SkillBookUi = new SkillBookUiGroup();

	public UIWidget TalentSkill;

	public ExtendSkillUiGroup[] ExtendSkills;

	public UIButton Button_LinkedSkill;

	private Listener _listener;

	private HeroSkillIconComponent _SkillComponent;

	private int _reqItemCount;

	private CreatureData _creatureData;

	private int _linkedIndex;

	public HeroInfo HeroInfo { get; private set; }

	public int SkillSlotIndex { get; private set; }

	public void Open(HeroInfo heroInfo, int skillSlotIndex, int linkedIndex, Listener listener)
	{
		base.Open();
		HeroInfo = heroInfo;
		SkillSlotIndex = skillSlotIndex;
		_listener = listener;
		_linkedIndex = linkedIndex;
		_creatureData = GameRoot.Instance.CreatureDataContainer.GetData(HeroInfo.HeroIndex);
		Refresh();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _SkillComponent);
		if ((bool)SkillLevelUpEffect)
		{
			SkillLevelUpEffect.Stop(immediatelyClear: true);
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ExtendSkills); i++)
		{
			ExtendSkillUiGroup extendSkillUiGroup = ExtendSkills[i];
			if (extendSkillUiGroup.Widget_Effect != null)
			{
				extendSkillUiGroup.Widget_Effect.Stop(immediatelyClear: true);
			}
		}
		SkillManaUi.ClearManaGauge();
		SkillBookUi.Clear();
	}

	public void SetHeroInfo(HeroInfo heroInfo)
	{
		HeroInfo = heroInfo;
	}

	public void Refresh(bool levelUp = false)
	{
		if (!IsOpen())
		{
			return;
		}
		Reset();
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(HeroInfo, SkillSlotIndex, _linkedIndex);
		if (skillDetailInfo.BasicInfo.TableData != null)
		{
			int num = skillDetailInfo.BasicInfo.Level;
			if (HeroInfo is CachedHeroInfo heroInfo)
			{
				num += StatCalculator.GetEquipOptionAddedSkillLevel(heroInfo, SkillSlotIndex);
			}
			bool enable = _creatureData.HasLinkedSkill(SkillSlotIndex);
			UIHelper.EnableGameObject(Button_LinkedSkill, enable);
			SetupSkillName(skillDetailInfo);
			NGame2.NUI.Common.SetupSkillTypeInfo(Label_CoolTime, SkillManaUi, HeroInfo, skillDetailInfo);
			SetupSkillIcon(skillDetailInfo);
			SetupSkillLevel(num);
			SetupCurSkillLevelDesc(skillDetailInfo);
			SetupButtons(skillDetailInfo);
			SkillBookUi.Setup(HeroInfo);
			SetupExtendSkills(skillDetailInfo);
			if (levelUp && _SkillComponent != null)
			{
				_SkillComponent.PlayLevelUpEffect();
			}
		}
	}

	private void SetMainSkill()
	{
	}

	private void SetLinkedSkill()
	{
	}

	private void SetupSkillName(Helper.SkillDetailInfo skillInfo)
	{
		string skillName = SkillDescContainer.GetSkillName(HeroInfo, skillInfo.BasicInfo.TableData);
		UIHelper.SetLabelText(Label_SkillName, skillName);
	}

	private void SetupSkillTypeInfo(Helper.SkillDetailInfo skillInfo)
	{
		int num = ((skillInfo.BasicInfo.TableData != null) ? skillInfo.BasicInfo.TableData.CoolTimeMs : 0);
		if (num != 0)
		{
			int num2 = num / 1000;
			int num3 = (num - num2 * 1000) / 100;
			string text = ((num3 == 0) ? num2.ToString() : (num2 + "." + num3));
			UIHelper.EnableGameObject(Label_CoolTime, enable: true);
			UIHelper.SetLabelText(Label_CoolTime, LocalizationManager.GetString(HeroSkill.SKILL_COOLTIME_DESC, text));
		}
		else
		{
			UIHelper.EnableGameObject(Label_CoolTime, enable: false);
		}
		SkillManaUi.Setup(HeroInfo, skillInfo);
	}

	private void SetupSkillIcon(Helper.SkillDetailInfo skillInfo)
	{
		_SkillComponent = UIHelper.AddGridItem(SkillIconGrid, HeroSkillIconComponent.PrefabAuto);
		_SkillComponent.Open(HeroInfo, skillInfo, SkillSlotIndex, null);
	}

	private void SetupSkillLevel(int SkillLevel)
	{
		UIHelper.SetLabelText(Label_SkillLevel, (SkillLevel > 0) ? ("Lv " + SkillLevel) : Vespa_String_Extension_Method.NguiColor(LocalizationManager.GetString(HeroSkill.NOT_LEARNED_SKILL), Color.red));
	}

	private void SetupCurSkillLevelDesc(Helper.SkillDetailInfo skillInfo)
	{
		string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(skillInfo.BasicInfo.TableIndex, HeroInfo);
		UIHelper.SetLabelText(Label_SkillDesc, byHeroInfo);
		if (ScrollView_SkillDesc != null)
		{
			ScrollView_SkillDesc.ResetPosition();
		}
		SprkSkillDescriptionFormula.Setup(this, skillInfo);
	}

	private void SetupButtons(Helper.SkillDetailInfo skillInfo)
	{
		_ = skillInfo.BasicInfo.IsLocked;
	}

	private void SetupExtendSkills(Helper.SkillDetailInfo skillInfo)
	{
		string bookIconPath = "UpgradeBook/" + skillInfo.BookInfo.CreatureTagType + "_" + SkillSlotIndex;
		int num = System.Math.Min(skillInfo.ExtendInfos.Length, ExtendSkills.Length);
		for (int i = 0; i < num; i++)
		{
			ExtendSkills[i]?.Setup(HeroInfo, skillInfo.ExtendInfos[i], bookIconPath);
		}
	}

	public void PlayIconEffect()
	{
		if (IsOpen() && (bool)SkillLevelUpEffect)
		{
			if (SkillLevelUpEffect.IsPlaying)
			{
				SkillLevelUpEffect.Stop(immediatelyClear: true);
			}
			SkillLevelUpEffect.Play(null);
		}
	}

	public void PlayExtendedEffect(int extendedSlotIndex)
	{
		if (extendedSlotIndex <= 0)
		{
			return;
		}
		int num = extendedSlotIndex - 1;
		if (num < __Extension_Method_ICollection__.COUNT(ExtendSkills))
		{
			ExtendSkillUiGroup extendSkillUiGroup = ExtendSkills[num];
			if (extendSkillUiGroup != null && (bool)extendSkillUiGroup.Widget_Effect)
			{
				extendSkillUiGroup.Widget_Effect.Play(null);
			}
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}

	public void OnClickLearnSkillButton()
	{
		if (_listener != null)
		{
			_listener.LearnSkill(SkillSlotIndex);
		}
	}

	private void LearnExtendSkill(int extendSlot)
	{
		if (ExtendSkills.Length < extendSlot)
		{
			return;
		}
		ExtendSkillUiGroup extendSkillUiGroup = ExtendSkills[extendSlot - 1];
		if (extendSkillUiGroup == null)
		{
			return;
		}
		if (extendSkillUiGroup.ExtendInfo.ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.Extendable)
		{
			if (_listener != null)
			{
				SprkSkillBookTickets.Extend(_listener, SkillSlotIndex, extendSlot, enough: true, this);
			}
		}
		else if (extendSkillUiGroup.ExtendInfo.ExtendStatus == Helper.SkillExtendInfo.EExtendStatus.NotEnoughItem && _listener != null)
		{
			SprkSkillBookTickets.Extend(_listener, SkillSlotIndex, extendSlot, enough: false, this);
		}
	}

	public void OnClickLearnExtendSkill1()
	{
		LearnExtendSkill(1);
	}

	public void OnClickLearnExtendSkill2()
	{
		LearnExtendSkill(2);
	}

	public void OnClickLearnExtendSkill3()
	{
		LearnExtendSkill(3);
	}

	public void OnClickChangeLinkedSkill()
	{
		if (_creatureData == null)
		{
			return;
		}
		int[] linkedSkillIndexBySlot = _creatureData.GetLinkedSkillIndexBySlot(SkillSlotIndex);
		if (__Extension_Method_ICollection__.COUNT(linkedSkillIndexBySlot) != 0)
		{
			if (__Extension_Method_ICollection__.COUNT(linkedSkillIndexBySlot) < _linkedIndex + 1)
			{
				_linkedIndex = 0;
			}
			else
			{
				_linkedIndex++;
			}
			Refresh();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<ItemInfo>.OnChanged(ItemInfo itemInfo)
	{
		if (itemInfo != null && HeroInfo != null)
		{
			SkillBookUi.OnChanged(itemInfo);
		}
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo info)
	{
		if (info.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.SkillExtended)
		{
			Refresh();
			PlayExtendedEffect(info.NewValue);
		}
	}
}
