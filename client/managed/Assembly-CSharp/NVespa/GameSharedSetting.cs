using LitJson;
using NGame2.NBattleContext;
using NVespa.NGameSharedSetting;
using UnityEngine;

namespace NVespa;

public class GameSharedSetting : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public const string AssetPath = "[0.GameShared]/Vespa/Resources/GameSharedSetting.json";

	public const string ResourcePath = "Resources/GameSharedSetting";

	public static GameSharedSetting data;

	public NguiData Ngui = new NguiData();

	public ShaderData Shader = new ShaderData();

	public ValueData Value = new ValueData();

	public SoundData Sound = new SoundData();

	static GameSharedSetting()
	{
		Init();
	}

	public static void Init()
	{
		if (data == null)
		{
			Load();
			SingletonAuto<BattleContext>.instance.RegisterListener(data.Value);
		}
	}

	public static void Load()
	{
		data = new GameSharedSetting();
		TextAsset textAsset = VespaResource.Load<TextAsset>("Resources/GameSharedSetting");
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
		Ngui.LOADFROMJSON(json["Ngui"]);
		Shader.LOADFROMJSON(json["Shader"]);
		Value.LOADFROMJSON(json["Value"]);
		Sound.LOADFROMJSON(json["Sound"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("Ngui", Ngui);
		jsonData.AddFieldJsonMake("Shader", Shader);
		jsonData.AddFieldJsonMake("Value", Value);
		jsonData.AddFieldJsonMake("Sound", Sound);
		return jsonData;
	}
}
