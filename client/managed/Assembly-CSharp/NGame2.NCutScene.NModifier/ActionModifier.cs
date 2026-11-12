using System.Collections.Generic;
using LitJson;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;

namespace NGame2.NCutScene.NModifier;

public class ActionModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable;

	public NGame2.NRender.NRenderFacade.AnimationData.StockData ActionStock = new NGame2.NRender.NRenderFacade.AnimationData.StockData();

	public bool BlendAction = true;

	public bool UseCustomFacial;

	public string[] FacialNameArray = new string[2]
	{
		string.Empty,
		string.Empty
	};

	public List<FacialAnimationModifier> FacialModList = new List<FacialAnimationModifier>();

	public List<FacialAnimationModifier> _excuteFacialModList;

	private bool IsModified;

	public bool IsEnd => __Extension_Method_ICollection__.COUNT(_excuteFacialModList) == 0;

	public void Begin(CreatureModifier creatureMod, ILocalTimer localTimer, int slotIndex)
	{
		if (!IsEnable || creatureMod == null)
		{
			return;
		}
		FacialAnimationData facialAnimationData = null;
		if (UseCustomFacial)
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(FacialNameArray) && (!String_Extension_Method.IsNullOrEmpty(FacialNameArray[0]) || !String_Extension_Method.IsNullOrEmpty(FacialNameArray[1])))
			{
				facialAnimationData = new FacialAnimationData();
				facialAnimationData.StaticShapeList.AddRange(FacialNameArray);
			}
			if (__Extension_Method_ICollection__.COUNT(FacialModList) > 0)
			{
				_excuteFacialModList = new List<FacialAnimationModifier>();
				_excuteFacialModList.AddRange(FacialModList);
			}
		}
		ActionStock.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
		creatureMod.RenderFacade.Action.Motion(ActionStock, BlendAction, localTimer, facialAnimationData);
	}

	public void Update(CreatureModifier creatureMod, ILocalTimer localTimer, float elapseTick, int slotIndex)
	{
		if (!IsEnable || __Extension_Method_ICollection__.COUNT(_excuteFacialModList) <= 0)
		{
			return;
		}
		for (int num = __Extension_Method_ICollection__.COUNT(_excuteFacialModList) - 1; num >= 0; num--)
		{
			if (_excuteFacialModList[num].StartTick <= elapseTick)
			{
				creatureMod.RenderFacade.AnimationData.ChangeFacialAnimation(_excuteFacialModList[num]);
				_excuteFacialModList.RemoveAt(num);
			}
		}
	}

	public void Skip(CreatureModifier creatureMod)
	{
		if (IsEnable && UseCustomFacial && __Extension_Method_ICollection__.COUNT(_excuteFacialModList) > 0)
		{
			FacialAnimationModifier last = Vespa_Generic_IList_Extension_Method.GetLast(_excuteFacialModList);
			creatureMod.RenderFacade.AnimationData.ChangeFacialAnimation(last);
			_excuteFacialModList.Clear();
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		JsonData jsonData = json["ActionStock.AnimationKey"];
		if (!jsonData.isNone())
		{
			ActionStock.AnimationKey = AnimationKeyData.CreateFromJson(jsonData);
		}
		else
		{
			DefineCreature.EMotion eNUM = json["ActionStock.MotionType"].GETENUM(ActionStock.AnimationKey.MotionType);
			string sTRING = json["ActionStock.MotionName"].GETSTRING(ActionStock.AnimationKey.Name);
			ActionStock.AnimationKey = new AnimationKeyData(eNUM, sTRING);
		}
		ActionStock.TimeType = json["ActionStock.TimeType"].GETENUM(ActionStock.TimeType);
		ActionStock.TimeValue = json["ActionStock.TimeValue"].GETFLOAT(ActionStock.TimeValue);
		ActionStock.BlendTime = json["ActionStock.BlendTime"].GETFLOAT(ActionStock.BlendTime);
		ActionStock.BlendCurve = json["ActionStock.BlendCurve"].GetAnimationCurve(ActionStock.BlendCurve);
		ActionStock.MotionEnd = json["ActionStock.MotionEnd"].GETENUM(ActionStock.MotionEnd);
		BlendAction = json["BlendAction"].GETBOOLEAN(BlendAction);
		UseCustomFacial = json["UseCustomFacial"].GETBOOLEAN(UseCustomFacial);
		FacialNameArray.LoadFromJsonEx(json["FacialNameArray"]);
		FacialModList.LoadFromJson(json["FacialModList"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddField("ActionStock.AnimationKey", ActionStock.AnimationKey.MakeJson());
		jsonData.AddFieldEnum("ActionStock.TimeType", ActionStock.TimeType);
		jsonData.AddFieldDouble("ActionStock.TimeValue", ActionStock.TimeValue);
		jsonData.AddFieldDouble("ActionStock.BlendTime", ActionStock.BlendTime);
		jsonData.AddFieldAnimationCurve("ActionStock.BlendCurve", ActionStock.BlendCurve);
		jsonData.AddFieldEnum("ActionStock.MotionEnd", ActionStock.MotionEnd);
		jsonData.AddFieldBool("BlendAction", BlendAction);
		jsonData.AddFieldBool("UseCustomFacial", UseCustomFacial);
		jsonData.AddField("FacialNameArray", Json_Extension_Method_IList.MakeJson(FacialNameArray));
		jsonData.AddField("FacialModList", Json_Extension_Method_IList.MakeJson(FacialModList));
		return jsonData;
	}
}
