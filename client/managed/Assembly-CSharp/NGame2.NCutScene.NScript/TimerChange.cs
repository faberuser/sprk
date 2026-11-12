using LitJson;
using NVespa;

namespace NGame2.NCutScene.NScript;

public class TimerChange : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool PauseTimer = true;

	private bool _isEnd;

	bool IScript.IsEnd => _isEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		if (!PauseTimer)
		{
			GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.CutScene);
		}
		else
		{
			GlobalTimer.PauseTimer(GlobalTimer.EPauseFlag.CutScene);
		}
		_isEnd = true;
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
		PauseTimer = json["PauseTimer"].GETBOOLEAN(PauseTimer);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("PauseTimer", PauseTimer);
		return jsonData;
	}
}
