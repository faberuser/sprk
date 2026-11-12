using NGame2.NUI.NWindow;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class RaidQuestComponent : QuestComponent, InitOnCreate
{
	public new const string PrefabPath = "Prefab2/Component/Quest/RaidQuestComponent";

	public new static PrefabAuto<RaidQuestComponent> PrefabAuto = new PrefabAuto<RaidQuestComponent>("Prefab2/Component/Quest/RaidQuestComponent");

	public UIBundleLoader PortraitBundleTex;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClosePortrait();
	}

	private void InitPortrait(RaidData raidData)
	{
		RaidHelper.InitPortrait(this, PortraitBundleTex, raidData);
	}

	private void ClosePortrait()
	{
		UIHelper.SetBundleTexture(PortraitBundleTex, string.Empty);
	}

	protected override void SetupAchievement()
	{
		base.SetupAchievement();
		if (base.AchievementData.ReqType != AchievementReqType.ClearHardRaid)
		{
			return;
		}
		int num = Vespa_String_Extension_Method.TOINT(base.AchievementData.ReqValue, 0);
		if (num == 0)
		{
			UIHelper.SetBundleTexture(PortraitBundleTex, "Portrait/Circle/Raid/4Dragon_2");
			return;
		}
		RaidData data = GameRoot.Instance.RaidDataContainer.GetData(num, 1);
		if (data != null)
		{
			InitPortrait(data);
		}
	}
}
