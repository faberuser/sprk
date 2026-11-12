using System;
using System.Text;
using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class EventCraftMaterialComponent : AwakeChallengeItemComponent
{
	public new const string PrefabPath = "Prefab2/Component/EventCraft/EventCraftMaterialComponent";

	public new static PrefabAuto<EventCraftMaterialComponent> PrefabAuto = new PrefabAuto<EventCraftMaterialComponent>("Prefab2/Component/EventCraft/EventCraftMaterialComponent");

	public string EnableColor = "[ffcc00]";

	public string DisableColor = "[f0f0f0]";

	public int ReqCount { get; set; }

	public int CurCount { get; set; }

	public void Open(ItemData itemData, int reqCount, int curCount)
	{
		ReqCount = reqCount;
		CurCount = curCount;
		Action<ItemIconComponent> onClick = null;
		Open(itemData, reqCount, curCount, onClick);
		UseGraySacle(reqCount > curCount);
	}

	protected virtual void SetItemCountText(int ReqCount, int CurCount)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (ReqCount == 0 && CurCount == 0)
		{
			stringBuilder.Append(DisableColor).Append("- / -[-]");
		}
		else
		{
			stringBuilder.Append((ReqCount > CurCount) ? DisableColor : EnableColor);
			stringBuilder.Append(CurCount).Append("[-]").Append(EnableColor)
				.Append("/")
				.Append(ReqCount)
				.Append("[-]");
		}
		UIHelper.SetLabelText(Label_Count, stringBuilder.ToString());
	}
}
