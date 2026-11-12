using System;
using NGame2.NAccount;
using NGame2.NUI.NDescription;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ArchiveSkillComponent : LinkFactoryComponent, InitOnCreate
{
	[Serializable]
	public class ExtendSkillUiGroup
	{
		public UIBundleLoader SkillIconBundleTex;

		public UILabel Label_Desc;

		public UIBundleLoader BookIconBundleTex;

		public UILabel Label_ItemCount;

		public Helper.SkillExtendInfo ExtendInfo { get; private set; }

		public void Setup(HeroInfo heroInfo, Helper.SkillExtendInfo extendInfo, int extendSkillIndex, string bookIconPath)
		{
			ExtendInfo = extendInfo;
			string skillIconPath = UIHelper.GetSkillIconPath(extendSkillIndex, heroInfo);
			UIHelper.SetBundleTexture(SkillIconBundleTex, skillIconPath);
			string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(extendSkillIndex, heroInfo);
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

	public const string PrefabPath = "Prefab2/Component/IllustratedBook/ArchiveSkillComponent";

	public static PrefabAuto<ArchiveSkillComponent> PrefabAuto = new PrefabAuto<ArchiveSkillComponent>("Prefab2/Component/IllustratedBook/ArchiveSkillComponent");

	public UILabel LabelName;

	public UILabel LabelLevel;

	public UILabel LabelCoolDown;

	public UILabel LabelDesc;

	public UIButton ButtonShowTalent;

	public UISimpleGrid IconGrid;

	public HeroSkillDesc.SkillManaUiGroup SkillManaUi = new HeroSkillDesc.SkillManaUiGroup();

	public UIWidget WidgetTalentSkill;

	public UIWidget WidgetFrame;

	public ExtendSkillUiGroup[] ExtendSkills;

	public UILabel LabelShowTalent;

	public int OriginFrameHeight;

	private HeroSkillIconComponent _SkillComponent;

	private bool _isShowTalentSkill;

	private Listener _Listener;

	public void Open(Listener listener, CachedHeroInfo heroInfo, Helper.SkillDetailInfo skillInfo, int skillSlotIndex)
	{
		_Listener = listener;
		if (skillInfo.BasicInfo.TableData == null)
		{
			VespaDebug.LogError("Skill TableData is null");
			return;
		}
		Reset();
		string skillName = SkillDescContainer.GetSkillName(heroInfo, skillInfo.BasicInfo.TableData);
		UIHelper.SetLabelText(LabelName, skillName);
		UIHelper.SetLabelText(LabelLevel, "");
		for (int i = 0; i < 4; i++)
		{
			heroInfo.SetSkillLevel(i + 1, heroInfo.Level);
		}
		NGame2.NUI.Common.SetupSkillTypeInfo(LabelCoolDown, SkillManaUi, heroInfo, skillInfo);
		string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(skillInfo.BasicInfo.TableIndex, heroInfo);
		UIHelper.SetLabelText(LabelDesc, byHeroInfo);
		_SkillComponent = UIHelper.AddGridItem(IconGrid, HeroSkillIconComponent.PrefabAuto);
		_SkillComponent.Open(heroInfo, skillSlotIndex, null);
		SetupExtendSkills(heroInfo, skillInfo, skillSlotIndex);
		SetupSkillShowLabel();
		UIHelper.EnableGameObject(WidgetTalentSkill, enable: false);
	}

	public void Reset()
	{
		_isShowTalentSkill = false;
		SkillManaUi.ClearManaGauge();
		FactoryBase.SafeReleaseComponent(ref _SkillComponent);
		WidgetFrame.height = OriginFrameHeight;
	}

	void InitOnCreate.OnCreate()
	{
		Reset();
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}

	private void OnDisable()
	{
		_isShowTalentSkill = false;
		WidgetFrame.height = OriginFrameHeight;
	}

	private void SetupSkillShowLabel()
	{
		string stringDirect = LocalizationManager.GetStringDirect(_isShowTalentSkill ? "ARCHIVE_HERO_SKILL_HIDE" : "ARCHIVE_HERO_SKILL_SHOW");
		UIHelper.SetLabelText(LabelShowTalent, stringDirect);
	}

	private void RefreshTalentSkillUI()
	{
		UIHelper.EnableGameObject(WidgetTalentSkill, _isShowTalentSkill);
		WidgetFrame.height += (_isShowTalentSkill ? WidgetTalentSkill.height : (WidgetTalentSkill.height * -1));
	}

	private void SetupExtendSkills(CachedHeroInfo heroInfo, Helper.SkillDetailInfo skillInfo, int skillSlotIndex)
	{
		int[] skillExtend = heroInfo.CreatureData.GetSkillExtend(skillSlotIndex);
		string bookIconPath = "UpgradeBook/" + skillInfo.BookInfo.CreatureTagType + "_" + skillSlotIndex;
		long num = NVespa.Math.Min(NVespa.Math.Min(skillInfo.ExtendInfos.Length, ExtendSkills.Length), skillExtend.Length);
		for (int i = 0; i < num; i++)
		{
			ExtendSkills[i]?.Setup(heroInfo, skillInfo.ExtendInfos[i], skillExtend[i], bookIconPath);
		}
	}

	public void OnClickShowTalentSkill()
	{
		_isShowTalentSkill = !_isShowTalentSkill;
		SetupSkillShowLabel();
		RefreshTalentSkillUI();
		UIHelper.EnableGameObject(LabelDesc, !_isShowTalentSkill);
	}
}
