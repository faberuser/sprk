using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class NPCFriendshipRewardList : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/NPCRecruit/NPCFriendshipRewardList";

	public UILabelSelector TitleLabelSelector;

	public UILabelSelector DescLabelSelector;

	public UILabel GradeLabel;

	public UIScrollGrid ItemGrid;

	private List<NPCFriendshipRewardComponent> RewardList = new List<NPCFriendshipRewardComponent>();

	private NpcFriendlyType FriendlyType { get; set; }

	public void Open(int heroIndex, int step, NpcFriendlyType friendlyType)
	{
		base.Open();
		Init(heroIndex, step, friendlyType);
	}

	public void Init(int heroIndex, int step, NpcFriendlyType friendlyType)
	{
		FriendlyType = friendlyType;
		foreach (NPCFriendlyPointData pointData in GameRoot.Instance.NPCFriendlyPointDataContainer.GetPointDataList(heroIndex))
		{
			NPCFriendshipRewardComponent nPCFriendshipRewardComponent = UIHelper.AddGridItem(ItemGrid, NPCFriendshipRewardComponent.PrefabAuto);
			if (nPCFriendshipRewardComponent != null)
			{
				nPCFriendshipRewardComponent.Open(pointData, friendlyType);
				RewardList.Add(nPCFriendshipRewardComponent);
			}
		}
		UIHelper.RepositionGrid(ItemGrid);
		if (TitleLabelSelector != null)
		{
			TitleLabelSelector.Select(friendlyType.ToString());
		}
		if (DescLabelSelector != null)
		{
			DescLabelSelector.Select(friendlyType.ToString());
		}
		string text = "";
		UIHelper.SetLabelText(text: friendlyType switch
		{
			NpcFriendlyType.Recruit => LocalizationManager.GetString(NPCRecruit.NPC_FRIENDLY_GRADE_COMPLETE, step), 
			NpcFriendlyType.EventChristmas => LocalizationManager.GetString(EVENTCHRISTMAS.TITLE_REWARD_EVENT_ITEM_LEVEL, step), 
			_ => LocalizationManager.GetString(NPCRecruit.NPC_CONTRIBUTION_GRADE_COMPLETE, step), 
		}, label: GradeLabel);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(RewardList);
	}
}
