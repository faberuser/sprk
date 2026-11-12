using LitJson;
using NGame2.NCutScene.NModifier;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NCutScene.NScript;

public class CreatureAction : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public PositionModifier PositionMod = new PositionModifier();

	public DirectionModifier DirectionMod = new DirectionModifier();

	public ScaleModifier ScaleMod = new ScaleModifier();

	public ActionModifier ActionMod = new ActionModifier();

	public RotationModifier RotationMod = new RotationModifier();

	public readonly ISlotHolder SlotHolder = new CreatureSlotHolder(SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr);

	private ILocalTimer _localTimer;

	private int _elapsedTick;

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

	bool IScript.IsEnd
	{
		get
		{
			if (PositionMod.IsEnd && DirectionMod.IsEnd && ScaleMod.IsEnd && ActionMod.IsEnd)
			{
				return RotationMod.IsEnd;
			}
			return false;
		}
	}

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		CreatureModifier creatureMod = SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.GetCreatureMod(SlotIndex);
		_localTimer = localTimer;
		_elapsedTick = 0;
		PositionMod.Begin(creatureMod);
		DirectionMod.Begin(creatureMod);
		ScaleMod.Begin(creatureMod);
		RotationMod.Begin(creatureMod);
		ActionMod.Begin(creatureMod, _localTimer, SlotIndex);
	}

	void IScript.Update()
	{
		if (_localTimer != null)
		{
			CreatureModifier creatureMod = SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.GetCreatureMod(SlotIndex);
			if (creatureMod != null)
			{
				_elapsedTick += _localTimer.deltaTick;
				PositionMod.Update(creatureMod, _elapsedTick);
				DirectionMod.Update(creatureMod, _elapsedTick);
				ScaleMod.Update(creatureMod, _elapsedTick);
				ActionMod.Update(creatureMod, _localTimer, _elapsedTick, SlotIndex);
				RotationMod.Update(creatureMod, _elapsedTick);
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
				PositionMod.Skip(creatureMod);
				DirectionMod.Skip(creatureMod);
				ScaleMod.Skip(creatureMod);
				RotationMod.Skip(creatureMod);
				ActionMod.Skip(creatureMod);
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
		PositionMod.LOADFROMJSON(json["PositionMod"]);
		DirectionMod.LOADFROMJSON(json["DirectionMod"]);
		ScaleMod.LOADFROMJSON(json["ScaleMod"]);
		ActionMod.LOADFROMJSON(json["ActionMod"]);
		RotationMod.LOADFROMJSON(json["RotationMod"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("SlotIndex", SlotIndex);
		jsonData.AddFieldJsonMake("PositionMod", PositionMod);
		jsonData.AddFieldJsonMake("DirectionMod", DirectionMod);
		jsonData.AddFieldJsonMake("ScaleMod", ScaleMod);
		jsonData.AddFieldJsonMake("ActionMod", ActionMod);
		jsonData.AddFieldJsonMake("RotationMod", RotationMod);
		return jsonData;
	}
}
