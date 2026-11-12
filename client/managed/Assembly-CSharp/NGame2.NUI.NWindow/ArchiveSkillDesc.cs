using System;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NDescription;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class ArchiveSkillDesc : BaseWindow
{
	[Serializable]
	public class ExtendSkillUiGroup
	{
		public UIBundleLoader SkillIconBundleTex;

		public UILabel Label_Desc;

		public UIBundleLoader BookIconBundleTex;

		public UILabel Label_ItemCount;

		public Helper.SkillExtendInfo ExtendInfo { get; private set; }

		public void Setup(HeroInfo heroInfo, Helper.SkillExtendInfo extendInfo, string bookIconPath)
		{
			ExtendInfo = extendInfo;
			int index = extendInfo.TableData.Index;
			string skillIconPath = UIHelper.GetSkillIconPath(index, heroInfo);
			UIHelper.SetBundleTexture(SkillIconBundleTex, skillIconPath);
			string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(index, heroInfo);
			UIHelper.SetLabelText(Label_Desc, byHeroInfo);
			SetupSkillBook(ExtendInfo, bookIconPath);
		}

		private void SetupSkillBook(Helper.SkillExtendInfo extendInfo, string bookIconPath)
		{
			UIHelper.SetBundleTexture(BookIconBundleTex, bookIconPath);
			string text = "[FFC800]" + extendInfo.ExtendItemCount + "[-]";
			UIHelper.SetLabelText(Label_ItemCount, text);
		}
	}

	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/IllustratedBook/ArchiveSkillDesc";

	public UILabel LabelName;

	public UILabel LabelCoolDown;

	public UILabel LabelDesc;

	public UIButton ButtonShowTalent;

	public UISimpleGrid IconGrid;

	public HeroSkillDesc.SkillManaUiGroup SkillManaUi = new HeroSkillDesc.SkillManaUiGroup();

	public ExtendSkillUiGroup[] ExtendSkills;

	public UILabel LabelShowTalent;

	public UIWidget WidgetTalentSkill;

	public UIButton Button_LinkedSkill;

	private HeroSkillIconComponent _SkillComponent;

	private bool _isShowTalentSkill;

	private int _linkedIndex;

	private CreatureData _creatureData;

	private Listener _Listener;

	public CachedHeroInfo HeroInfo { get; private set; }

	public int SkillSlotIndex { get; private set; }

	public void Open(Listener listener, CachedHeroInfo heroInfo, int skillSlotIndex, int linkedIndex)
	{
		base.Open();
		_Listener = listener;
		HeroInfo = heroInfo;
		SkillSlotIndex = skillSlotIndex;
		_linkedIndex = linkedIndex;
		SetSkillInfo();
		_creatureData = GameRoot.Instance.CreatureDataContainer.GetData(HeroInfo.HeroIndex);
		if (_creatureData != null)
		{
			bool enable = _creatureData.HasLinkedSkill(SkillSlotIndex);
			UIHelper.EnableGameObject(Button_LinkedSkill, enable);
			_isShowTalentSkill = false;
			UIHelper.EnableGameObject(LabelDesc, !_isShowTalentSkill);
			UIHelper.EnableGameObject(WidgetTalentSkill, _isShowTalentSkill);
		}
	}

	public void SetSkillInfo()
	{
		Helper.SkillDetailInfo skillDetailInfo = Helper.GetSkillDetailInfo(HeroInfo, SkillSlotIndex, _linkedIndex);
		if (skillDetailInfo.BasicInfo.TableData == null)
		{
			VespaDebug.LogError("Skill TableData is null");
			SprkSkillDescriptionFormula.SetupArchive(this);
			return;
		}
		string skillName = SkillDescContainer.GetSkillName(HeroInfo, skillDetailInfo.BasicInfo.TableData);
		UIHelper.SetLabelText(LabelName, skillName);
		for (int i = 0; i < 4; i++)
		{
			HeroInfo.SetSkillLevel(i + 1, HeroInfo.Level);
		}
		NGame2.NUI.Common.SetupSkillTypeInfo(LabelCoolDown, SkillManaUi, HeroInfo, skillDetailInfo);
		string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(skillDetailInfo.BasicInfo.TableIndex, HeroInfo);
		UIHelper.SetLabelText(LabelDesc, byHeroInfo);
		FactoryBase.SafeReleaseComponent(ref _SkillComponent);
		_SkillComponent = UIHelper.AddGridItem(IconGrid, HeroSkillIconComponent.PrefabAuto);
		_SkillComponent.Open(HeroInfo, skillDetailInfo, SkillSlotIndex, null);
		SetupExtendSkills(HeroInfo, skillDetailInfo, SkillSlotIndex);
		SetupSkillShowLabel();
		SprkSkillDescriptionFormula.SetupArchive(this);
	}

	public override void Reset()
	{
		base.Reset();
		_isShowTalentSkill = false;
		SkillManaUi.ClearManaGauge();
		FactoryBase.SafeReleaseComponent(ref _SkillComponent);
	}

	private void SetupSkillShowLabel()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_isShowTalentSkill ? "ARCHIVE_HERO_SKILL_HIDE" : "ARCHIVE_HERO_SKILL_SHOW");
		UIHelper.SetLabelText(LabelShowTalent, stringDirect);
	}

	private void SetupExtendSkills(CachedHeroInfo heroInfo, Helper.SkillDetailInfo skillInfo, int skillSlotIndex)
	{
		string bookIconPath = "UpgradeBook/" + skillInfo.BookInfo.CreatureTagType + "_" + skillSlotIndex;
		long num = NVespa.Math.Min(skillInfo.ExtendInfos.Length, ExtendSkills.Length);
		for (int i = 0; i < num; i++)
		{
			ExtendSkills[i]?.Setup(heroInfo, skillInfo.ExtendInfos[i], bookIconPath);
		}
	}

	public void OnClickShowTalentSkill()
	{
		_isShowTalentSkill = !_isShowTalentSkill;
		SetupSkillShowLabel();
		UIHelper.EnableGameObject(LabelDesc, !_isShowTalentSkill);
		UIHelper.EnableGameObject(WidgetTalentSkill, _isShowTalentSkill);
	}

	public void OnClickSwitchLinkedSkill()
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
			SetSkillInfo();
		}
	}
}
