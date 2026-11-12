using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class NPCFriendshipRewardComponent : LinkFactoryComponent, InitOnCreate
{
	public static string PrefabPath = "Prefab2/Component/NPCRecruit/NPCFriendshipRewardComponent";

	public static PrefabAuto<NPCFriendshipRewardComponent> PrefabAuto = new PrefabAuto<NPCFriendshipRewardComponent>(PrefabPath);

	public UILabel GradeLabel;

	public UILabel ItemNameLabel;

	public UILabel RewardDescLabel;

	public Transform ItemIconRoot;

	private ItemIconComponent RewardItemIcon;

	public void Open(NPCFriendlyPointData pointData, NpcFriendlyType friendlyType)
	{
		string text = "";
		UIHelper.SetLabelText(text: (friendlyType != NpcFriendlyType.EventChristmas) ? LocalizationManager.GetString(NPCRecruit.NPC_FRIENDLY_GRADE, pointData.Step) : LocalizationManager.GetString(EVENTCHRISTMAS.TITLE_REWARD_EVENT_ITEM_LEVEL, pointData.Step), label: GradeLabel);
		UIHelper.SetLabelText(RewardDescLabel, LocalizationManager.GetStringDirect(pointData.RewardDesc));
		RewardItemIcon = UIHelper.CreateRewardMainItemIconComponent(pointData.RewardIndex, ItemIconRoot, ItemNameLabel);
	}

	public void Open(NPCFriendlyPointData pointData)
	{
		string text = LocalizationManager.GetString(NPCRecruit.NPC_FRIENDLY_GRADE, pointData.Step);
		UIHelper.SetLabelText(GradeLabel, text);
		UIHelper.SetLabelText(RewardDescLabel, LocalizationManager.GetStringDirect(pointData.RewardDesc));
		RewardItemIcon = UIHelper.CreateRewardMainItemIconComponent(pointData.RewardIndex, ItemIconRoot, ItemNameLabel);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref RewardItemIcon);
	}
}
