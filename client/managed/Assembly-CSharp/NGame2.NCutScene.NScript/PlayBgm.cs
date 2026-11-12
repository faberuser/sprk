using LitJson;
using NGameAsset;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NCutScene.NScript;

public class PlayBgm : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public string Code = string.Empty;

	public bool IsEventBgm = true;

	public float Volume = 1f;

	private ILocalTimer _localTimer;

	bool IScript.IsEnd => true;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		if (IsEventBgm)
		{
			SceneAutoBehaviour<CutSceneManager>.instance.PlayEventBgm(Code, Volume);
		}
		else
		{
			SingletonAuto<BgmManager2>.instance.Request(Code, isEvent: false, Volume);
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
		Code = json["Code"].GETSTRING(Code);
		IsEventBgm = json["IsEventBgm"].GETBOOLEAN(IsEventBgm);
		Volume = json["Volume"].GETFLOAT(Volume);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldBool("IsEventBgm", IsEventBgm);
		jsonData.AddFieldDouble("Volume", Volume);
		return jsonData;
	}
}
