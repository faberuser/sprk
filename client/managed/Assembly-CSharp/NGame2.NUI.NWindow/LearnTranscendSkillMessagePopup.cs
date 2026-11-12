using System;
using NGame2.NUI.NComponent2.NStat;
using NGame2.NUI.NDescription;
using NGame2.NUI.NManager;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class LearnTranscendSkillMessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/LearnTranscendSkillMessagePopup";

	public UITexture Texture_SkillIcon;

	public UILabel Label_SkillName;

	public UILabel Label_SkillDesc;

	public UILabel Label_Warning;

	public UILabel Label_NeedSkillPoint;

	public UITable Table_Labels;

	public SingleStatInfo StatInfo;

	public SingleStatInfo StatInfo2;

	public UIBundleLoader BundleLoader_SkillIcon;

	private Action _onConfirm;

	private HeroInfo _heroInfo;

	public static void Open(HeroInfo heroInfo, int skillIndex, Action onConfirm)
	{
		WindowPrefab<LearnTranscendSkillMessagePopup> windowPrefab = new WindowPrefab<LearnTranscendSkillMessagePopup>("Resources/Prefab2/Window/MessagePopup/LearnTranscendSkillMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(heroInfo, skillIndex, onConfirm);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public static void Open(HeroInfo heroInfo, int transcended, int skillSlotIndex, Action onConfirm)
	{
		WindowPrefab<LearnTranscendSkillMessagePopup> windowPrefab = new WindowPrefab<LearnTranscendSkillMessagePopup>("Resources/Prefab2/Window/MessagePopup/LearnTranscendSkillMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(heroInfo, transcended, skillSlotIndex, onConfirm);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(HeroInfo heroInfo, int skillIndex, Action onConfirm)
	{
		base.Open();
		_onConfirm = onConfirm;
		_heroInfo = heroInfo;
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		if (data == null)
		{
			VespaDebug.LogError("Invalid skill index, skill data is null, skillIndex = " + skillIndex);
			Close();
			return;
		}
		string skillName = SkillDescContainer.GetSkillName(_heroInfo, data);
		AssetTexture skillIcon = UIHelper.GetSkillIcon2(skillIndex, heroInfo);
		UIHelper.ResetPlaneAssetTextureAsync(this, Texture_SkillIcon, skillIcon);
		UIHelper.SetLabelText(Label_SkillName, skillName);
		string byHeroInfo = SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(skillIndex, _heroInfo);
		UIHelper.SetLabelText(Label_SkillDesc, byHeroInfo);
		CreatureData.TranscendSkill transcendSkillData = _heroInfo.CreatureData.GetTranscendSkillData(skillIndex);
		if (transcendSkillData != null)
		{
			UIHelper.EnableGameObject(Label_Warning.gameObject, transcendSkillData.IsHeroSkillTranscend());
			string stringDirect = LocalizationManager.GetStringDirect("NEED_TRANSCEND_SKILL_POINT", transcendSkillData.ReqSKillPoint);
			UIHelper.SetLabelText(Label_NeedSkillPoint, stringDirect);
			if (Table_Labels != null)
			{
				Table_Labels.Reposition();
			}
		}
		RestoredPortal.ReadablePerkDescription(this);
	}

	private void OpenInternal(HeroInfo heroInfo, int transcended, int skillSlotIndex, Action onConfirm)
	{
		base.Open();
		_onConfirm = onConfirm;
		_heroInfo = heroInfo;
		CreatureTranscendOptionData data = GameRoot.Instance.CreatureTranscendOptionDataContainer.GetData(transcended, skillSlotIndex);
		if (data == null)
		{
			VespaDebug.LogError($"Invalid skill index, transcended = {transcended}, skillSlotIndex = {skillSlotIndex}");
			Close();
			return;
		}
		string buffIconPath = UIHelper.GetBuffIconPath(data.IconPath, heroInfo);
		UIHelper.SetBundleTexture(BundleLoader_SkillIcon, buffIconPath);
		string text = LocalizationManager.GetString(HeroTranscend.TRANSCEND_ATTRIBUTE, transcended);
		UIHelper.SetLabelText(Label_SkillName, text);
		OperationType optionType = data.OptionType1;
		int optionValue = data.OptionValue1;
		string optionDisplay = StatData.GetOptionDisplay(optionType, optionValue);
		UIHelper.SetLabelText(StatInfo.NameLabel, optionDisplay);
		if (data.OptionType2 == OperationType.None)
		{
			UIHelper.EnableGameObject(StatInfo2, enable: false);
		}
		else
		{
			OperationType optionType2 = data.OptionType2;
			optionValue = data.OptionValue2;
			optionDisplay = StatData.GetOptionDisplay(optionType2, optionValue);
			UIHelper.SetLabelText(StatInfo2.NameLabel, optionDisplay);
		}
		string stringDirect = LocalizationManager.GetStringDirect("NEED_TRANSCEND_SKILL_POINT", data.ReqTranscendPoint);
		UIHelper.SetLabelText(Label_NeedSkillPoint, stringDirect);
	}

	public void OnClickYes()
	{
		Close();
		Vespa_System_Action_Extension_Method.fire(_onConfirm);
	}

	public void OnClickCancel()
	{
		Close();
	}
}
