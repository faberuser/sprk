using LitJson;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NCutScene.NScript;

public class SceneChange : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public string EntryMenuCode = string.Empty;

	public bool ClearCreature = true;

	public bool CloseForeground = true;

	public bool ClearText = true;

	public bool Reverse;

	private bool _isEnd;

	bool IScript.IsEnd => _isEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_isEnd = false;
		SceneAutoBehaviour<CutSceneManager>.instance.ChangeEntryMenuBackground(EntryMenuCode, ClearCreature, CloseForeground, ClearText, isSkip, Reverse, delegate
		{
			_isEnd = true;
		});
	}

	void IScript.Update()
	{
	}

	void IScript.End()
	{
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
		_isEnd = true;
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		EntryMenuCode = json["EntryMenuCode"].GETSTRING(EntryMenuCode);
		ClearCreature = json["ClearCreature"].GETBOOLEAN(ClearCreature);
		CloseForeground = json["CloseForeground"].GETBOOLEAN(CloseForeground);
		ClearText = json["ClearText"].GETBOOLEAN(ClearText);
		Reverse = json["Reverse"].GETBOOLEAN(Reverse);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("EntryMenuCode", EntryMenuCode);
		jsonData.AddFieldBool("ClearCreature", ClearCreature);
		jsonData.AddFieldBool("CloseForeground", CloseForeground);
		jsonData.AddFieldBool("ClearText", ClearText);
		jsonData.AddFieldBool("Reverse", Reverse);
		return jsonData;
	}
}
