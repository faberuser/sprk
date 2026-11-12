using NVespa.NPrefab2;

namespace NGame2.NBattleView.NUIOn3D;

public class Damage : OnFieldDisplayBase
{
	public static PrefabAuto<Damage> PrefabAuto = new PrefabAuto<Damage>("Resources/Prefab2/UIOn3D/Damage");

	public static PrefabAuto<Damage> CoinPrefabAuto = new PrefabAuto<Damage>("Resources/Prefab2/UIOn3D/LootCoin");

	public static PrefabAuto<Damage> WorldMapCoinPrefabAuto = new PrefabAuto<Damage>("Resources/Prefab2/UIOn3D/WorldMapCoin");

	public static PrefabAuto<Damage> WorldMapRubyPrefabAuto = new PrefabAuto<Damage>("Resources/Prefab2/UIOn3D/WorldMapRuby");

	public UILabel Label;
}
