using NGame2.NUI;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NBattleView.NUIOn3d2;

public class DamageResist : OnFieldDisplayBase
{
	public static PrefabAuto<DamageResist> PrefabAuto = new PrefabAuto<DamageResist>("Resources/Prefab2/UIOn3d2/DamageResist", isGlobal: false, EPrefabType.Ngui, pooling: true, touchMaterial: true);

	public UILabel Label;

	public bool Open(int creatureUid, string stateType)
	{
		string stringDirect = LocalizationManager.GetStringDirect("BATTLE.STATE_TYPE_NAME." + stateType);
		UIHelper.SetLabelText(Label, stringDirect);
		return Open(creatureUid);
	}
}
