using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class TreasureHouseBossComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void OnClickTab(TreasureHouseBossComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/TreasureHouse/TreasureHouseBossComponent";

	public const string PrefabPath2 = "Prefab2/Component/TreasureHouse/TreasureHouseSelectBossComponent";

	public static PrefabAuto<TreasureHouseBossComponent> PrefabAuto = new PrefabAuto<TreasureHouseBossComponent>("Prefab2/Component/TreasureHouse/TreasureHouseBossComponent");

	public static PrefabAuto<TreasureHouseBossComponent> PrefabAuto2 = new PrefabAuto<TreasureHouseBossComponent>("Prefab2/Component/TreasureHouse/TreasureHouseSelectBossComponent");

	private IListener _listener;

	public TreasureHouseDungeonData _treasureHouseData;

	public UIToggle TreasureHouseTab;

	public UIBundleLoader BossIconTex;

	public UILabel BossTitleLabel;

	public void Open(IListener listener, TreasureHouseDungeonData treasureHouseData, bool isSelectedDungeon = false)
	{
		if (listener == null)
		{
			VespaDebug.LogError("listener == null");
			return;
		}
		_listener = listener;
		_treasureHouseData = treasureHouseData;
		string assetPath = (isSelectedDungeon ? treasureHouseData.SelectIconPortraitPath : treasureHouseData.IconPortraitPath);
		UIHelper.SetBundleTexture(BossIconTex, assetPath);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(treasureHouseData.CreatureIndex);
		UIHelper.SetLabelText(BossTitleLabel, LocalizationManager.GetStringDirect(data.Name));
	}

	public void OnClick()
	{
		if (_listener != null)
		{
			_listener.OnClickTab(this);
		}
	}

	public void SelectToggle(bool isSelect)
	{
		TreasureHouseTab.Set(isSelect);
	}
}
