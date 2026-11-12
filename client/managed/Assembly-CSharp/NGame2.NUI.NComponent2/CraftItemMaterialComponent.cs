using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class CraftItemMaterialComponent : AwakeChallengeItemComponent
{
	public new const string PrefabPath = "Prefab2/Component/Inventory/CraftItemMaterialComponent";

	public new static PrefabAuto<CraftItemMaterialComponent> PrefabAuto = new PrefabAuto<CraftItemMaterialComponent>("Prefab2/Component/Inventory/CraftItemMaterialComponent");

	public void Open(ItemData itemData, long ReqCount, int CurCount)
	{
		Open(itemData, ReqCount, CurCount, null);
		UseGraySacle(ReqCount > CurCount);
	}

	protected virtual void SetItemCountText(int ReqCount, int CurCount)
	{
		string text = ((ReqCount > CurCount) ? "[f0f0f0]" : "[ffcc00]") + $"{CurCount}/{ReqCount}" + "[-]";
		text = ((ReqCount == 0 && CurCount == 0) ? "[f0f0f0]- / -" : text);
		UIHelper.SetLabelText(Label_Count, text);
	}
}
