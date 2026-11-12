using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class DecoratorNameEvent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/WorldMap/Node/Decorator/DecoratorName_02";

	public static PrefabAuto<DecoratorNameEvent> PrefabAuto = new PrefabAuto<DecoratorNameEvent>("Prefab2/WorldMap/Node/Decorator/DecoratorName_02");

	public UILabel LabelName;

	public void Open(int chapterIndex, int dungeonIndex)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(data.Code))
			{
				UIHelper.SetLabelText(LabelName, $"{chapterIndex}-{dungeonIndex}");
			}
			else
			{
				UIHelper.SetLabelText(LabelName, LocalizationManager.GetStringDirect(data.Code[0]));
			}
		}
	}
}
