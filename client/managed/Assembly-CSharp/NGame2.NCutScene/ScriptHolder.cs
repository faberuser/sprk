using LitJson;
using NVespa.NSingleton;

namespace NGame2.NCutScene;

public class ScriptHolder : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public int Tick;

	public IScript Value;

	bool ILoadJson.Load(JsonData json)
	{
		Tick = json["Tick"].GETINT(Tick);
		string sTRING = json["ScriptType"].GETSTRING(string.Empty);
		Value = SceneAutoBehaviour<ScriptFactory>.instance.CreateScriptSafe(sTRING);
		Value.Load(json["ScriptBody"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("Tick", Tick);
		jsonData.AddFieldString("ScriptType", Value.GetType().Name);
		jsonData.AddFieldJsonMake("ScriptBody", Value);
		return jsonData;
	}
}
