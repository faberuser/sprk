using LitJson;
using NGame2.NCutScene.NModifier;
using NVespa;

namespace NGame2.NCutScene.NScript;

public class BackgroundColor : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public BackgroundColorModifier ColorMod = new BackgroundColorModifier();

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	bool IScript.IsEnd => ColorMod.IsEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		_elapsedTick = 0;
		ColorMod.Begin();
	}

	void IScript.Update()
	{
		if (_localTimer != null)
		{
			_elapsedTick += _localTimer.deltaTick;
			ColorMod.Update(_elapsedTick);
		}
	}

	void IScript.End()
	{
		if (_localTimer != null)
		{
			ColorMod.Skip();
		}
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		ColorMod.LOADFROMJSON(json["ColorMod"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("ColorMod", ColorMod);
		return jsonData;
	}
}
