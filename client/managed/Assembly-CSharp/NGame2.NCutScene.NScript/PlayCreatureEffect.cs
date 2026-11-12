using LitJson;
using NGame2.NCutScene.NModifier;
using NGameAsset;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NCutScene.NScript;

public class PlayCreatureEffect : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public string Code = string.Empty;

	public string PivotName = string.Empty;

	public TransformData ExtraTransform = new TransformData();

	public bool LoopPlay;

	public float LoopEndTick;

	public bool ReleaseImmediately;

	public bool KeepPlay;

	public int StartGroup;

	public int EndGroup;

	private ILocalTimer _localTimer;

	private IAsyncLoad<IEffect> _effectAsync;

	private float _duringEndTick;

	private bool _isEnd;

	public readonly ISlotHolder SlotHolder = new CreatureSlotHolder(SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr);

	public int CreatureSlot
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

	bool IScript.IsEnd => _isEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		if (!CreateAssetEffect())
		{
			_isEnd = true;
		}
		else
		{
			_isEnd = false;
		}
	}

	private bool CreateAssetEffect()
	{
		if (_effectAsync != null)
		{
			ReleaseAssetEffect();
		}
		CreatureModifier creatureMod = SceneAutoBehaviour<CutSceneManager>.instance.CreatureSlotMgr.GetCreatureMod(CreatureSlot);
		if (creatureMod == null || creatureMod.RenderFacade == null)
		{
			return false;
		}
		int layer = creatureMod.RenderFacade.gameObject.layer;
		_effectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(Code, layer, ExtraTransform);
		_effectAsync.Value.SetTimer(_localTimer);
		if (!String_Extension_Method.IsNullOrEmpty(PivotName))
		{
			Transform parent = creatureMod.RenderFacade.Pivot.FindTransform(PivotName);
			_effectAsync.Value.SetParent(parent);
		}
		else
		{
			_effectAsync.Value.SetParent(creatureMod.RenderFacade.AssetCreature);
		}
		_effectAsync.Value.GetRenderGroupModifier().SetGroundClip(enable: false);
		return true;
	}

	private void ReleaseAssetEffect()
	{
		if (_effectAsync != null)
		{
			EffectContainer.ReleaseEffect(_effectAsync, detachParent: true, ReleaseImmediately);
			_effectAsync = null;
		}
	}

	private bool CheckKeepPlayEffect()
	{
		if (KeepPlay)
		{
			if (StartGroup < 0 && EndGroup <= 0)
			{
				return false;
			}
			if (!SceneAutoBehaviour<CutSceneManager>.isValidInstance)
			{
				return false;
			}
			if (SceneAutoBehaviour<CutSceneManager>.instance.CurrentScriptGroup == null)
			{
				return false;
			}
			int currentScript = SceneAutoBehaviour<CutSceneManager>.instance.CurrentScript;
			if (currentScript < StartGroup)
			{
				return false;
			}
			if (currentScript > EndGroup)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	void IScript.Update()
	{
		if (CheckKeepPlayEffect())
		{
			if (_effectAsync != null)
			{
				if (_effectAsync.Value.IsEnd())
				{
					CreateAssetEffect();
				}
				_effectAsync.Value.Update();
			}
			else
			{
				CreateAssetEffect();
			}
		}
		else if (_localTimer == null || _isEnd)
		{
			_isEnd = true;
		}
		else
		{
			if (_effectAsync == null)
			{
				return;
			}
			_effectAsync.Value.Update();
			if (KeepPlay)
			{
				_isEnd = true;
				ReleaseAssetEffect();
			}
			else if (LoopPlay)
			{
				bool flag = false;
				if (LoopEndTick > 0f)
				{
					_duringEndTick += _localTimer.deltaTick;
				}
				if (_duringEndTick > LoopEndTick)
				{
					flag = true;
				}
				if (flag)
				{
					_isEnd = true;
					ReleaseAssetEffect();
				}
				else if (_effectAsync.Value.IsEnd())
				{
					CreateAssetEffect();
				}
			}
			else if (_effectAsync.Value.IsEnd())
			{
				_isEnd = true;
				ReleaseAssetEffect();
			}
		}
	}

	private void FinalizeScript()
	{
		_isEnd = true;
		_duringEndTick = 0f;
		if (_localTimer != null)
		{
			ReleaseAssetEffect();
		}
	}

	void IScript.End()
	{
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
		if (finalizeType == FinalizeType.ForceStop)
		{
			FinalizeScript();
		}
		else if (!CheckKeepPlayEffect())
		{
			FinalizeScript();
		}
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		Code = json["Code"].GETSTRING(Code);
		CreatureSlot = json["CreatureSlot"].GETINT(CreatureSlot);
		PivotName = json["PivotName"].GETSTRING(PivotName);
		ExtraTransform.LOADFROMJSON(json["ExtraTransform"]);
		LoopPlay = json["LoopPlay"].GETBOOLEAN(LoopPlay);
		LoopEndTick = json["LoopEndTick"].GETFLOAT(LoopEndTick);
		ReleaseImmediately = json["ReleaseImmediately"].GETBOOLEAN(ReleaseImmediately);
		KeepPlay = json["KeepPlay"].GETBOOLEAN(KeepPlay);
		StartGroup = json["StartGroup"].GETINT(StartGroup);
		EndGroup = json["EndGroup"].GETINT(EndGroup);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldLong("CreatureSlot", CreatureSlot);
		jsonData.AddFieldString("PivotName", PivotName);
		jsonData.AddFieldJsonMake("ExtraTransform", ExtraTransform);
		jsonData.AddFieldBool("LoopPlay", LoopPlay);
		jsonData.AddFieldDouble("LoopEndTick", LoopEndTick);
		jsonData.AddFieldBool("ReleaseImmediately", ReleaseImmediately);
		jsonData.AddFieldBool("KeepPlay", KeepPlay);
		jsonData.AddFieldLong("StartGroup", StartGroup);
		jsonData.AddFieldLong("EndGroup", EndGroup);
		return jsonData;
	}
}
