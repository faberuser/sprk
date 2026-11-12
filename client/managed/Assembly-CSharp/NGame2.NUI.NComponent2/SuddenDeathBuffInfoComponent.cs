using NGame2.NUI.NDescription;
using NGameAsset;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class SuddenDeathBuffInfoComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Match/SuddenDeathBuffInfoComponent";

	public static PrefabAuto<SuddenDeathBuffInfoComponent> PrefabAuto = new PrefabAuto<SuddenDeathBuffInfoComponent>("Resources/Prefab2/Component/Match/SuddenDeathBuffInfoComponent");

	public UILabel BuffStep;

	public UILabel BuffName;

	public UILabel BuffDesc;

	public UILabel BuffTime;

	public UITexture SkillIcon;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(int step, SkillData skillData, int timeMs)
	{
		if (skillData != null)
		{
			UIHelper.SetLabelText(BuffStep, step + 1);
			UIHelper.SetLabelText(BuffName, SkillDescContainer.GetSkillName(skillData));
			AssetTexture skillIcon = UIHelper.GetSkillIcon2(skillData.Index, null);
			UIHelper.ResetPlaneAssetTextureAsync(this, SkillIcon, skillIcon);
			UIHelper.SetLabelText(BuffTime, LocalizationManager.GetString(Match.APPLY_SKILL_AFTER, timeMs / 1000));
			string description = skillData.Description;
			UIHelper.SetLabelText(BuffDesc, description);
		}
	}
}
