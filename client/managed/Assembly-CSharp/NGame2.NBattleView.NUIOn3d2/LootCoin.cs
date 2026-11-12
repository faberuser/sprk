using NGame2.NUI;
using NVespa.NPrefab2;

namespace NGame2.NBattleView.NUIOn3d2;

public class LootCoin : OnFieldDisplayBase
{
	public static PrefabAuto<LootCoin> PrefabAuto = new PrefabAuto<LootCoin>("Resources/Prefab2/UIOn3d2/LootCoin", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public UILabel Label;

	public bool Open(int creatureUid, int amount)
	{
		UIHelper.SetLabelText(Label, amount.ToString());
		return Open(creatureUid);
	}
}
