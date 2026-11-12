using LitJson;
using NGameAsset;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NCutScene.NScript;

public class PlayAssetSoundInCutScene : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public string Code = string.Empty;

	public int AddVolume;

	private ILocalTimer _localTimer;

	bool IScript.IsEnd => true;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		SoundStock soundStock = new SoundStock
		{
			Code = Code,
			AddVolume = AddVolume,
			LocalTimer = _localTimer
		};
		SceneAutoBehaviour<CutSceneManager>.instance.PlaySound(soundStock);
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
		if (_localTimer != null && finalizeType == FinalizeType.ForceStop)
		{
			SceneAutoBehaviour<CutSceneManager>.instance.StopSound();
		}
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		Code = json["Code"].GETSTRING(Code);
		AddVolume = json["AddVolume"].GETINT(AddVolume);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldLong("AddVolume", AddVolume);
		return jsonData;
	}
}
