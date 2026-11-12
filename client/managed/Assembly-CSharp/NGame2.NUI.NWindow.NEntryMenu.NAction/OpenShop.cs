using System.Collections.Generic;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NAction;

public class OpenShop : IAction
{
	void IAction.Run(CampaignNodeData nodeData, NShared.EntryMenu menu, List<object> values)
	{
		string valueString = ArgParser.GetValueString(menu.EntryMenuButton, 2);
		ShopData data = GameRoot.Instance.ShopDataContainer.GetData(valueString);
		if (data == null)
		{
			VespaDebug.LogError("shop data not found. shopCode=" + valueString);
			return;
		}
		NPCData npcData = null;
		if (nodeData != null)
		{
			npcData = GameRoot.Instance.NPCDataContainer.GetData(nodeData.EntryNPCCode);
		}
		SceneAutoBehaviour<ShopManagement>.instance.OpenShop(data, npcData);
	}

	void IAction.Run(NShared.EntryMenu menu, List<object> values)
	{
		string valueString = ArgParser.GetValueString(menu.EntryMenuButton, 2);
		ShopData data = GameRoot.Instance.ShopDataContainer.GetData(valueString);
		if (data == null)
		{
			VespaDebug.LogError("shop data not found. shopCode=" + valueString);
			return;
		}
		NPCData npcData = null;
		if (values != null && __Extension_Method_ICollection__.COUNT(values) > 0)
		{
			npcData = GameRoot.Instance.NPCDataContainer.GetData(values[0].ToString());
		}
		SceneAutoBehaviour<ShopManagement>.instance.OpenShop(data, npcData);
	}
}
