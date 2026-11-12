using LitJson;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NCutScene.NScript;

public class CreatureLeave : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public int SlotIndex;

	bool IScript.IsEnd => true;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		SceneAutoBehaviour<CutSceneManager>.instance.LeaveCreature(SlotIndex);
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
		SlotIndex = json["SlotIndex"].GETINT(SlotIndex);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("SlotIndex", SlotIndex);
		return jsonData;
	}
}
