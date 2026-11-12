using LitJson;
using NVespa;

namespace NGame2.NCutScene.NScript;

public class EmptyScript : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	bool IScript.IsEnd => true;

	void IScript.Visit(IVisitor visitor)
	{
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
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		return new JsonData();
	}
}
