using LitJson;
using NVespa;

namespace NGame2.NCutScene.NScript;

public class PauseMainScene : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsPauseMainScene;

	private ILocalTimer _localTimer;

	bool IScript.IsEnd => true;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		if (IsPauseMainScene)
		{
			GlobalTimer.PauseTimer(GlobalTimer.EPauseFlag.CutScene);
		}
		else
		{
			GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.CutScene);
		}
	}

	void IScript.Update()
	{
		_ = _localTimer;
	}

	void IScript.End()
	{
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsPauseMainScene = json["IsPauseMainScene"].GETBOOLEAN(IsPauseMainScene);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsPauseMainScene", IsPauseMainScene);
		return jsonData;
	}
}
