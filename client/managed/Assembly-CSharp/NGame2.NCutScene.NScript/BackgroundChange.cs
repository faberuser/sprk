using LitJson;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NCutScene.NScript;

public class BackgroundChange : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public BackgroundController.StockData Stock = new BackgroundController.StockData();

	public bool ClearCreature = true;

	public bool CloseForeground = true;

	public bool ClearText = true;

	private bool _isEnd;

	bool IScript.IsEnd => _isEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_isEnd = false;
		SceneAutoBehaviour<CutSceneManager>.instance.BackgroundControl.Close();
		SceneAutoBehaviour<CutSceneManager>.instance.BackgroundControl.Open(SceneAutoBehaviour<CutSceneManager>.instance, Stock, isSkip, delegate
		{
			_isEnd = true;
		});
	}

	void IScript.Reset()
	{
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

	bool ILoadJson.Load(JsonData json)
	{
		Stock.LOADFROMJSON(json["Stock"]);
		ClearCreature = json["ClearCreature"].GETBOOLEAN(ClearCreature);
		CloseForeground = json["CloseForeground"].GETBOOLEAN(CloseForeground);
		ClearText = json["ClearText"].GETBOOLEAN(ClearText);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("Stock", Stock);
		jsonData.AddFieldBool("ClearCreature", ClearCreature);
		jsonData.AddFieldBool("CloseForeground", CloseForeground);
		jsonData.AddFieldBool("ClearText", ClearText);
		return jsonData;
	}
}
