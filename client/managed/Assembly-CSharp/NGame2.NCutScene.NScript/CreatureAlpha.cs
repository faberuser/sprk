using LitJson;
using NGame2.NCutScene.NModifier;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NCutScene.NScript;

public class CreatureAlpha : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public ExtraAlphaModifier ExtraAlphaMod = new ExtraAlphaModifier();

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	public readonly ISlotHolder SlotHolder = new CreatureSlotHolder(SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr);

	public int SlotIndex
	{
		get
		{
			return SlotHolder.SlotIndex;
		}
		set
		{
			SlotHolder.SlotIndex = value;
		}
	}

	bool IScript.IsEnd => ExtraAlphaMod.IsEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		CreatureModifier creatureMod = SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.GetCreatureMod(SlotIndex);
		_localTimer = localTimer;
		_elapsedTick = 0;
		ExtraAlphaMod.Begin(creatureMod);
	}

	void IScript.Update()
	{
		if (_localTimer != null)
		{
			CreatureModifier creatureMod = SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.GetCreatureMod(SlotIndex);
			if (creatureMod != null)
			{
				_elapsedTick += _localTimer.deltaTick;
				ExtraAlphaMod.Update(creatureMod, _elapsedTick);
			}
		}
	}

	void IScript.End()
	{
		if (_localTimer != null)
		{
			CreatureModifier creatureMod = SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.GetCreatureMod(SlotIndex);
			if (creatureMod != null)
			{
				ExtraAlphaMod.Skip(creatureMod);
			}
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
		SlotIndex = json["SlotIndex"].GETINT(SlotIndex);
		ExtraAlphaMod.LOADFROMJSON(json["ExtraAlphaMod"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("SlotIndex", SlotIndex);
		jsonData.AddFieldJsonMake("ExtraAlphaMod", ExtraAlphaMod);
		return jsonData;
	}
}
