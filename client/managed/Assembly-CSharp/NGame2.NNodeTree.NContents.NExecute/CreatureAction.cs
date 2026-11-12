using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NObject.NMotionTrail;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NUtil;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NScene;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NExecute;

public class CreatureAction : ExecuteBase, IKey, IExecute
{
	public enum EDirectionType
	{
		DontChange,
		InitDirection,
		LookTarget,
		CallbackMove,
		FollowBattleDirection
	}

	public class MoveCurveData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public float BeginFrame;

		public float EndFrame = 1f;

		public AnimationCurve ForwardCurve = new AnimationCurve();

		public AnimationCurve RightCurve = new AnimationCurve();

		public AnimationCurve UpCurve = new AnimationCurve();

		bool ILoadJson.Load(JsonData json)
		{
			BeginFrame = json["BeginFrame"].GETFLOAT(BeginFrame);
			EndFrame = json["EndFrame"].GETFLOAT(EndFrame);
			ForwardCurve = json["ForwardCurve"].GetAnimationCurve(ForwardCurve);
			RightCurve = json["RightCurve"].GetAnimationCurve(RightCurve);
			UpCurve = json["UpCurve"].GetAnimationCurve(UpCurve);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldDouble("BeginFrame", BeginFrame);
			jsonData.AddFieldDouble("EndFrame", EndFrame);
			jsonData.AddFieldAnimationCurve("ForwardCurve", ForwardCurve);
			jsonData.AddFieldAnimationCurve("RightCurve", RightCurve);
			jsonData.AddFieldAnimationCurve("UpCurve", UpCurve);
			return jsonData;
		}
	}

	public class CallbackMoveData
	{
		public Vector3 BeginPos;

		public Vector3 Forward;

		public Vector3 Right;
	}

	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public float BeginTime;

		public NGame2.NRender.NRenderFacade.AnimationData.StockData MotionStock;

		public List<MotionTrailObject> MotionTrailList;

		public CallbackMoveData CallbackMove;

		public UidCache MyselfCache = new UidCache();

		public UidCache TargetCache = new UidCache();

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string OldContentsTypeName = "Action/NoMoveDirection";

	public static string OldContentsTypeName2 = "Execute/NoMoveDirection";

	public static string ContentsTypeName = "Execute/" + typeof(CreatureAction).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> MyselfUid;

	public ArgumentSingleGeneric<long> TargetUid;

	public AnimationKeyData AnimationKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, string.Empty);

	public DefineCreature.EMotionEnd MotionEnd;

	public bool BlendMotion = true;

	public bool DynamicHub;

	public ArgumentSingleGeneric<float> HubOffsetY;

	public bool UseCustomFacial;

	public string[] FacialNameArray = new string[2]
	{
		string.Empty,
		string.Empty
	};

	public EDirectionType DirectionType = EDirectionType.InitDirection;

	public ArgumentSingleGeneric<Vector3> Direction;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public List<CreateData> MotionTrailList = new List<CreateData>();

	public List<MoveCurveData> MoveCurveList = new List<MoveCurveData>();

	public CreatureAction(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		MyselfUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
		TargetUid = new ArgumentSingleGeneric<long>(VariableKeyword.Target);
		Direction = new ArgumentSingleGeneric<Vector3>(VariableKeyword.Direction);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
		HubOffsetY = new ArgumentSingleGeneric<float>(0.3f);
	}

	public override void Wakeup(IVariable variable)
	{
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
		HubOffsetY.GetValue(variable);
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		RenderFacade renderFacade = UidCache.FindRenderFacade(variable, MyselfUid);
		if (renderFacade == null)
		{
			return;
		}
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.MyselfCache.SetRenderFacade(renderFacade);
		Creature battleCreature = localItem.MyselfCache.GetBattleCreature(variable, MyselfUid);
		if (battleCreature != null)
		{
			battleCreature.NodeTreeController.ReleaseExternalHandler();
			battleCreature.OnDynamicHUD = DynamicHub;
			if (DynamicHub)
			{
				battleCreature.HUDOffetY = HubOffsetY.GetValue(localItem.Variable, 0.3f);
			}
		}
		if (DirectionType != EDirectionType.DontChange)
		{
			Vector3 value = Direction.GetValue(variable);
			if (value.magnitude > 0.01f)
			{
				renderFacade.Action.SetDirection(Vespa_Unity_Vector3_Extension_Method.VecXZ(value.normalized), BlendMotion);
			}
		}
		float value2 = DurationTime.GetValue(variable, 1f);
		NGame2.NRender.NRenderFacade.AnimationData.StockData stock = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
		stock.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
		stock.AnimationKey = AnimationKey;
		stock.MotionEnd = MotionEnd;
		stock.TimeLength = value2;
		renderFacade.Action.ApplyModifier(ref stock);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.MotionStock = stock;
		CreatureMotion(renderFacade, localItem.MotionStock, runtime.LocalTimer);
		if (__Extension_Method_ICollection__.COUNT(MotionTrailList) > 0)
		{
			Transform transform = renderFacade.Pivot.FindTransform(EPivotType.AssetRoot);
			if (transform != null)
			{
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(MotionTrailList); i++)
				{
					MotionTrailObject motionTrailObject = Container.instance.CreateMotionTrail(MotionTrailList[i], transform, runtime);
					if (!(motionTrailObject == null))
					{
						if (localItem.MotionTrailList == null)
						{
							localItem.MotionTrailList = new List<MotionTrailObject>();
						}
						localItem.MotionTrailList.Add(motionTrailObject);
					}
				}
			}
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(MoveCurveList) && battleCreature != null)
		{
			localItem.AddOnMessageByCreatureUid<LocalItem, CallbackMessage<SetPosition>>(localItem, LocalItemOnMessageFast, battleCreature.Uid);
			localItem.AddOnMessageByCreatureUid<LocalItem, CallbackMessage<BeginMove>>(localItem, LocalItemOnMessageFast, battleCreature.Uid);
		}
		runtime.ExecuteContainer.Pending(this, localItem);
	}

	private void CreatureMotion(RenderFacade renderFacade, NGame2.NRender.NRenderFacade.AnimationData.StockData motionStock, ILocalTimer localTimer)
	{
		FacialAnimationData facialAnimationData = null;
		if (UseCustomFacial && !__Extension_Method_ICollection__.IsNullOrEmpty(FacialNameArray) && (!String_Extension_Method.IsNullOrEmpty(FacialNameArray[0]) || !String_Extension_Method.IsNullOrEmpty(FacialNameArray[1])))
		{
			facialAnimationData = new FacialAnimationData();
			facialAnimationData.StaticShapeList.AddRange(FacialNameArray);
		}
		renderFacade.Action.Motion(motionStock, BlendMotion, localTimer, facialAnimationData);
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (localItem.IsEnd)
		{
			return;
		}
		Creature battleCreature = localItem.MyselfCache.GetBattleCreature(localItem.Variable, MyselfUid);
		if (battleCreature != null)
		{
			battleCreature.OnDynamicHUD = false;
			battleCreature.HUDOffetY = 0f;
		}
		RenderFacade renderFacade = localItem.MyselfCache.GetRenderFacade(localItem.Variable, MyselfUid);
		if (renderFacade != null && Vespa_Common_Class_Extension_Method.EQUALS(renderFacade.Action.CurrentMotionStock, localItem.MotionStock) && MotionEnd == DefineCreature.EMotionEnd.End)
		{
			renderFacade.Action.Idle();
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(localItem.MotionTrailList))
		{
			for (int i = 0; i < localItem.MotionTrailList.Count; i++)
			{
				Container.instance.ReleaseObject(localItem.MotionTrailList[i]);
			}
			localItem.MotionTrailList.Clear();
		}
		localItem.ClearMessageHolder();
		localItem.IsEnd = true;
		CallerEnd.Call(localItem.Runtime, localItem.Variable);
	}

	public void UpdateExecute(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem { IsEnd: false } localItem))
		{
			return;
		}
		float num = localItem.localTimer.time - localItem.BeginTime;
		float value = DurationTime.GetValue(localItem.Variable, 1f);
		if (DynamicHub)
		{
			localItem.MyselfCache.GetBattleCreature(localItem.Variable, MyselfUid).HUDOffetY = HubOffsetY.GetValue(localItem.Variable, 0.3f);
		}
		if (DirectionType == EDirectionType.LookTarget)
		{
			SetDirectionToTarget(localItem);
		}
		else if (DirectionType == EDirectionType.FollowBattleDirection)
		{
			FollowBattleDirection(localItem);
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(localItem.MotionTrailList))
		{
			float frame = Mathf.Clamp01(num / Vespa_Float_Extension_Method.NoneZero(value));
			for (int i = 0; i < localItem.MotionTrailList.Count; i++)
			{
				localItem.MotionTrailList[i].SetFrame(frame);
				localItem.MotionTrailList[i].ChainUpdate();
			}
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(MoveCurveList))
		{
			UpdateCallbackMove(localItem, num / Vespa_Float_Extension_Method.NoneZero(value));
		}
		if (num > value || StopThis.GetValue(localItem.Variable, defaultValue: false))
		{
			ActionEnd(localItem);
		}
	}

	private void SetDirectionToTarget(LocalItem localItem)
	{
		if (!localItem.MyselfCache.TryGetPosition(localItem.Variable, MyselfUid, out var oPos) || !localItem.TargetCache.TryGetPosition(localItem.Variable, TargetUid, out var oPos2))
		{
			return;
		}
		Vector3 vector = oPos2 - oPos;
		if (!(vector.magnitude < 0.001f))
		{
			RenderFacade renderFacade = localItem.MyselfCache.GetRenderFacade(localItem.Variable, MyselfUid);
			if (renderFacade != null)
			{
				renderFacade.Action.SetDirection(Vespa_Unity_Vector3_Extension_Method.VecXZ(vector.normalized), BlendMotion);
			}
		}
	}

	private void FollowBattleDirection(LocalItem localItem)
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return;
		}
		long value = MyselfUid.GetValue(localItem.Variable);
		if (value == 0L)
		{
			return;
		}
		BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache((int)value);
		if (creatureCache != null)
		{
			Vector3 vector = creatureCache.Direction;
			RenderFacade renderFacade = localItem.MyselfCache.GetRenderFacade(localItem.Variable, MyselfUid);
			if (renderFacade != null)
			{
				renderFacade.Action.SetDirection(Vespa_Unity_Vector3_Extension_Method.VecXZ(vector.normalized), BlendMotion);
			}
		}
	}

	private void UpdateCallbackMove(LocalItem localItem, float frame)
	{
		Creature battleCreature = localItem.MyselfCache.GetBattleCreature(localItem.Variable, MyselfUid);
		if (battleCreature == null)
		{
			return;
		}
		MoveCurveData moveCurveData = null;
		for (int i = 0; i < MoveCurveList.Count; i++)
		{
			if (!(frame < MoveCurveList[i].BeginFrame))
			{
				moveCurveData = MoveCurveList[i];
			}
		}
		if (moveCurveData == null)
		{
			return;
		}
		if (localItem.CallbackMove == null)
		{
			BattleFacade.CreatureCacheData creatureCache = battleCreature.CreatureCache;
			Vector3 position = battleCreature.Position;
			Vector3 vector = creatureCache.Position - position;
			if (vector.magnitude < 0.3f)
			{
				return;
			}
			localItem.CallbackMove = new CallbackMoveData();
			localItem.CallbackMove.BeginPos = position;
			localItem.CallbackMove.Forward = vector;
			localItem.CallbackMove.Right = Vector3.Cross(Vector3.up, vector);
		}
		float num = Mathf.InverseLerp(moveCurveData.BeginFrame, moveCurveData.EndFrame, frame);
		float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(moveCurveData.ForwardCurve, num, num);
		float num3 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(moveCurveData.RightCurve, num, 0f);
		float num4 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(moveCurveData.UpCurve, num, 0f);
		Vector3 vector2 = localItem.CallbackMove.BeginPos + localItem.CallbackMove.Forward * num2 + localItem.CallbackMove.Right * num3 + Vector3.up * localItem.CallbackMove.Forward.magnitude * num4;
		Vector3 position2 = battleCreature.Position;
		battleCreature.SetBattlePos(vector2, validateBattleArea: true);
		if (DirectionType == EDirectionType.CallbackMove)
		{
			VectorXZ normalized = Vespa_Unity_Vector3_Extension_Method.VecXZ(vector2 - position2).normalized;
			if (normalized.sqrMagnitude > Mathf.Epsilon)
			{
				battleCreature.Action.SetDirection(normalized, BlendMotion);
			}
		}
	}

	public bool IsExecuteEnd(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem localItem))
		{
			return true;
		}
		return localItem.IsEnd;
	}

	public void RequestStopExecute(ItemBase itemBase, ERequestStop requestStop)
	{
		if (itemBase is LocalItem localItem)
		{
			ActionEnd(localItem);
		}
	}

	private void LocalItemOnMessageFast(LocalItem localItem, CallbackMessage<BeginMove> message)
	{
		if (localItem == null || !message.IsValidMessage())
		{
			return;
		}
		BeginMove callback = message.Callback;
		if (callback.OperationUid == 0L)
		{
			return;
		}
		Creature battleCreature = localItem.MyselfCache.GetBattleCreature(localItem.Variable, MyselfUid);
		if (!(battleCreature == null) && battleCreature.Uid == callback.CreatureUid)
		{
			Vector3 position = battleCreature.Position;
			Vector3 vector = callback.TargetPosition - position;
			if (!(vector.sqrMagnitude < Mathf.Epsilon))
			{
				localItem.CallbackMove = new CallbackMoveData();
				localItem.CallbackMove.BeginPos = position;
				localItem.CallbackMove.Forward = vector;
				localItem.CallbackMove.Right = Vector3.Cross(Vector3.up, vector);
			}
		}
	}

	private void LocalItemOnMessageFast(LocalItem localItem, CallbackMessage<SetPosition> message)
	{
		if (localItem == null || !message.IsValidMessage())
		{
			return;
		}
		SetPosition callback = message.Callback;
		if (callback.OperationUid == 0L || callback.MethodType != SetPosition.SetPositionMethodType.Warp)
		{
			return;
		}
		Creature battleCreature = localItem.MyselfCache.GetBattleCreature(localItem.Variable, MyselfUid);
		if (!(battleCreature == null) && battleCreature.Uid == callback.CreatureUid)
		{
			Vector3 position = battleCreature.Position;
			Vector3 vector = callback.Position - position;
			if (!(vector.sqrMagnitude < Mathf.Epsilon))
			{
				localItem.CallbackMove = new CallbackMoveData();
				localItem.CallbackMove.BeginPos = position;
				localItem.CallbackMove.Forward = vector;
				localItem.CallbackMove.Right = Vector3.Cross(Vector3.up, vector);
			}
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
		System.Action action = delegate
		{
			for (int i = 0; i < MotionTrailList.Count; i++)
			{
				MotionTrailList[i].ReservePrefab();
			}
		};
		preloader.AddAction(action, string.Empty);
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		if (!MyselfUid.LoadFromJson(json["MyselfUid"]))
		{
			MyselfUid.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Myself);
		}
		if (!TargetUid.LoadFromJson(json["TargetUid"]))
		{
			TargetUid.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Target);
		}
		AnimationKey = AnimationKeyData.CreateFromJson(json["AnimationKey"]);
		BlendMotion = json["BlendMotion"].GETBOOLEAN(BlendMotion);
		DirectionType = json["DirectionType"].GETENUM(DirectionType);
		MotionEnd = json["MotionEnd"].GETENUM(MotionEnd);
		Direction.LoadFromJson(json["Direction"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		MotionTrailList.LoadFromJson(json["MotionTrailList"]);
		MoveCurveList.LoadFromJson(json["MoveCurveList"]);
		UseCustomFacial = json["UseCustomFacial"].GETBOOLEAN(UseCustomFacial);
		FacialNameArray.LoadFromJsonEx(json["FacialNameArray"]);
		DynamicHub = json["DynamicHub"].GETBOOLEAN(DynamicHub);
		HubOffsetY.LoadFromJson(json["HubOffsetY"]);
		LoadOldJson(json);
	}

	private void LoadOldJson(JsonData json)
	{
		JsonData i_json = json["MotionName"];
		if (i_json.IsString())
		{
			AnimationKey = AnimationKeyData.CreateNamedAction(i_json.GETSTRING(string.Empty));
		}
		JsonData i_json2 = json["FrameLength"];
		if (i_json2.IsDouble())
		{
			float fLOAT = i_json2.GETFLOAT(1f);
			if (Mathf.Approximately(fLOAT, 1f))
			{
				DurationTime.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<float>(VariableKeyword.DurationTime);
			}
			else
			{
				DurationTime.Holder.Value = SingletonAuto<ValueFactory>.instance.Create("Float/Calculate/Multiply");
				ITypeValueBase append = SingletonAuto<ValueFactory>.instance.CreateConstant(fLOAT);
				ITypeValueBase append2 = SingletonAuto<ValueFactory>.instance.CreateVariable<float>(VariableKeyword.DurationTime);
				ArgumentContainer argumentContainer = DurationTime.Holder.Value.GetArgumentContainer();
				argumentContainer.AppendTypeValue(append);
				argumentContainer.AppendTypeValue(append2);
			}
		}
		if (json.Contains("DontChangeDirection"))
		{
			bool bOOLEAN = json["DontChangeDirection"].GETBOOLEAN(i_default: false);
			bool bOOLEAN2 = json["FollowTargetDirection"].GETBOOLEAN(i_default: false);
			if (bOOLEAN)
			{
				DirectionType = EDirectionType.DontChange;
			}
			else
			{
				DirectionType = ((!bOOLEAN2) ? EDirectionType.InitDirection : EDirectionType.LookTarget);
			}
		}
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("MyselfUid", MyselfUid.MakeJson());
		jsonData.AddField("TargetUid", TargetUid.MakeJson());
		jsonData.AddField("AnimationKey", AnimationKey.MakeJson());
		jsonData.AddFieldBool("BlendMotion", BlendMotion);
		jsonData.AddFieldEnum("DirectionType", DirectionType);
		jsonData.AddFieldEnum("MotionEnd", MotionEnd);
		jsonData.AddField("Direction", Direction.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldList("MotionTrailList", MotionTrailList);
		jsonData.AddFieldList("MoveCurveList", MoveCurveList);
		jsonData.AddFieldBool("UseCustomFacial", UseCustomFacial);
		jsonData.AddField("FacialNameArray", Json_Extension_Method_IList.MakeJson(FacialNameArray));
		jsonData.AddFieldBool("DynamicHub", DynamicHub);
		jsonData.AddField("HubOffsetY", HubOffsetY.MakeJson());
		return jsonData;
	}
}
