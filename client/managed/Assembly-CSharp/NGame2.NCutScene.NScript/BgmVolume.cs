using LitJson;
using NGame2.NCutScene.NModifier;
using NVespa;

namespace NGame2.NCutScene.NScript;

public class BgmVolume : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public BgmVolumeModifier VolumeMod = new BgmVolumeModifier();

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	bool IScript.IsEnd => VolumeMod.IsEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		_elapsedTick = 0;
		VolumeMod.Begin();
	}

	void IScript.Update()
	{
		if (_localTimer != null)
		{
			_elapsedTick += _localTimer.deltaTick;
			VolumeMod.Update(_elapsedTick);
		}
	}

	void IScript.End()
	{
		if (_localTimer != null)
		{
			VolumeMod.Skip();
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
		VolumeMod.LOADFROMJSON(json["VolumeMod"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("VolumeMod", VolumeMod);
		return jsonData;
	}
}
