using LitJson;
using NGame2.NGameSetting;
using UnityEngine;

namespace NGame2;

public class GameSetting : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public const string AssetPath = "Resources/Common/GameSetting.json";

	public const string ResourcePath = "Resources/Common/GameSetting";

	public static GameSetting data;

	public TestData Test = new TestData();

	public EntryMenuData EntryMenu = new EntryMenuData();

	public UiData Ui = new UiData();

	public WorldMapData WorldMap = new WorldMapData();

	public VolumeData Volume = new VolumeData();

	public TutorialData Tutorial = new TutorialData();

	public BattleFieldData BattleField = new BattleFieldData();

	public BattleViewData BattleView = new BattleViewData();

	public CutSceneData CutScene = new CutSceneData();

	public GameAssetData GameAsset = new GameAssetData();

	public DamageDisplayData DamageDisplay = new DamageDisplayData();

	public DamageColorData DamageColor = new DamageColorData();

	public DamageShakeData DamageShake = new DamageShakeData();

	static GameSetting()
	{
		Init();
	}

	public static void Init()
	{
		if (data == null)
		{
			Load();
		}
	}

	public static void Load()
	{
		data = new GameSetting();
		TextAsset textAsset = VespaResource.Load<TextAsset>("Resources/Common/GameSetting");
		if (!(textAsset == null))
		{
			JsonData json = JsonMapper.ToObject(textAsset.text);
			data.LOADFROMJSON(json);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		Test.LOADFROMJSON(json["Test"]);
		EntryMenu.LOADFROMJSON(json["EntryMenu"]);
		Ui.LOADFROMJSON(json.GetJsonAny("Ui", "UI"));
		WorldMap.LOADFROMJSON(json["WorldMap"]);
		Volume.LOADFROMJSON(json["Volume"]);
		Tutorial.LOADFROMJSON(json["Tutorial"]);
		BattleField.LOADFROMJSON(json["BattleField"]);
		BattleView.LOADFROMJSON(json["BattleView"]);
		CutScene.LOADFROMJSON(json["CutScene"]);
		GameAsset.LOADFROMJSON(json["GameAsset"]);
		DamageDisplay.LOADFROMJSON(json.GetJsonAny("DamageDisplay", "damage_display"));
		DamageColor.LOADFROMJSON(json.GetJsonAny("DamageColor", "defeat_color"));
		DamageShake.LOADFROMJSON(json.GetJsonAny("DamageShake", "defeat_shake"));
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("Test", Test);
		jsonData.AddFieldJsonMake("EntryMenu", EntryMenu);
		jsonData.AddFieldJsonMake("Ui", Ui);
		jsonData.AddFieldJsonMake("WorldMap", WorldMap);
		jsonData.AddFieldJsonMake("Volume", Volume);
		jsonData.AddFieldJsonMake("Tutorial", Tutorial);
		jsonData.AddFieldJsonMake("BattleField", BattleField);
		jsonData.AddFieldJsonMake("BattleView", BattleView);
		jsonData.AddFieldJsonMake("CutScene", CutScene);
		jsonData.AddFieldJsonMake("GameAsset", GameAsset);
		jsonData.AddFieldJsonMake("DamageDisplay", DamageDisplay);
		jsonData.AddFieldJsonMake("DamageColor", DamageColor);
		jsonData.AddFieldJsonMake("DamageShake", DamageShake);
		return jsonData;
	}
}
