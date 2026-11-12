using LitJson;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NCutScene.NScript;

public class EndPlayCreatureEffect : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool EndPlay;

	public int StartGroup;

	public int StartScript;

	bool IScript.IsEnd => true;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		EndPlayEffect();
	}

	private void EndPlayEffect()
	{
		if (!EndPlay || !SceneAutoBehaviour<CutSceneManager>.isValidInstance || __Extension_Method_ICollection__.IsNullOrEmpty(SceneAutoBehaviour<CutSceneManager>.instance.GroupList) || SceneAutoBehaviour<CutSceneManager>.instance.GroupList.Count <= StartGroup)
		{
			return;
		}
		ScriptGroup scriptGroup = SceneAutoBehaviour<CutSceneManager>.instance.GroupList[StartGroup];
		if (scriptGroup != null && !__Extension_Method_ICollection__.IsNullOrEmpty(scriptGroup.ScriptList) && scriptGroup.ScriptList.Count > StartScript)
		{
			IScript value = scriptGroup.ScriptList[StartScript].Value;
			if (value != null && !value.IsEnd)
			{
				value.FinalizeScript(FinalizeType.ForceStop);
			}
		}
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
		EndPlay = json["EndPlay"].GETBOOLEAN(EndPlay);
		StartGroup = json["StartGroup"].GETINT(StartGroup);
		StartScript = json["StartScript"].GETINT(StartScript);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("EndPlay", EndPlay);
		jsonData.AddFieldLong("StartGroup", StartGroup);
		jsonData.AddFieldLong("StartScript", StartScript);
		return jsonData;
	}
}
