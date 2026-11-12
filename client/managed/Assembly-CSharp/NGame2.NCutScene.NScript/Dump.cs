using LitJson;
using NVespa;

namespace NGame2.NCutScene.NScript;

public class Dump : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	private string _jsonBody = string.Empty;

	bool IScript.IsEnd => true;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
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
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		_jsonBody = json.ToJson(i_prettyPrint: true);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		return JsonMapper.ToObject(_jsonBody);
	}
}
