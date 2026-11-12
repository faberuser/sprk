using LitJson;
using NGame2.NCutScene.NModifier;
using NVespa;

namespace NGame2.NCutScene.NScript;

public class TextWindowAlpha : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public TextWindowAlphaModifier AlphaMod = new TextWindowAlphaModifier();

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	bool IScript.IsEnd => AlphaMod.IsEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		_elapsedTick = 0;
		AlphaMod.Begin();
	}

	void IScript.Update()
	{
		if (_localTimer != null)
		{
			_elapsedTick += _localTimer.deltaTick;
			AlphaMod.Update(_elapsedTick);
		}
	}

	void IScript.End()
	{
		if (_localTimer != null)
		{
			AlphaMod.Skip();
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
		AlphaMod.LOADFROMJSON(json["AlphaMod"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("AlphaMod", AlphaMod);
		return jsonData;
	}
}
