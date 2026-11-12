using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NExecute;

public class OnBeginMoveCallback : ExecuteBase, IKey, IKeyFinalize, IExecute
{
	public enum EDirectionType
	{
		Init,
		Update,
		DontChange
	}

	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public BeginMove Callback;

		public float BeginTime;

		public float DurationTime;

		public Vector3 BeginPos;

		public Vector3 Forward;

		public Vector3 Right;

		public int CallbackCount;

		public bool PendingExecute;

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public class MoveCurveData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public AnimationCurve ForwardCurve = new AnimationCurve();

		public AnimationCurve RightCurve = new AnimationCurve();

		public AnimationCurve UpCurve = new AnimationCurve();

		bool ILoadJson.Load(JsonData json)
		{
			ForwardCurve = JsonTools.ToCurve(json["ForwardCurve"], ForwardCurve);
			RightCurve = JsonTools.ToCurve(json["RightCurve"], RightCurve);
			UpCurve = JsonTools.ToCurve(json["UpCurve"], UpCurve);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddField("ForwardCurve", JsonTools.FromCurve(ForwardCurve));
			jsonData.AddField("RightCurve", JsonTools.FromCurve(RightCurve));
			jsonData.AddField("UpCurve", JsonTools.FromCurve(UpCurve));
			return jsonData;
		}
	}

	public static string OldContentsTypeName = "Action/" + typeof(OnBeginMoveCallback).Name;

	public static string ContentsTypeName = "Execute/" + typeof(OnBeginMoveCallback).Name;

	public Callee CalleeReady;

	public Caller CallerEnd;

	public List<MoveCurveData> MoveCurveList = new List<MoveCurveData>();

	public EDirectionType DirectionType;

	public bool BlendDirection = true;

	public OnBeginMoveCallback(NodeData i_node)
		: base(i_node)
	{
		CalleeReady = new Callee(this, "CalleeReady", ActionReady);
		CallerEnd = new Caller(this, "CallerEnd");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private void ActionReady(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		Creature battleCreature = localItem.Variable.GetBattleCreature(VariableKeyword.Myself);
		if (battleCreature != null)
		{
			localItem.AddOnMessageByCreatureUid<LocalItem, CallbackMessage<BeginMove>>(localItem, LocalItemOnMessageFast, battleCreature.Uid);
		}
	}

	private void LocalItemOnMessageFast(LocalItem localItem, CallbackMessage<BeginMove> message)
	{
		if (localItem == null || !message.IsValidMessage())
		{
			return;
		}
		BeginMove callback = message.Callback;
		Creature battleCreature = localItem.Variable.GetBattleCreature(VariableKeyword.Myself);
		if (battleCreature == null || battleCreature.Uid != callback.CreatureUid || callback.OperationUid == 0L || SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(callback.OperationUid) == null)
		{
			return;
		}
		Vector3 vector = Vespa_Unity_Vector3_Extension_Method.ReplaceY(battleCreature.Position, 0f);
		Vector3 forward = Vespa_Unity_Vector3_Extension_Method.ReplaceY(callback.TargetPosition, 0f) - vector;
		if (!(forward.magnitude < Mathf.Epsilon))
		{
			if (DirectionType == EDirectionType.Init || DirectionType == EDirectionType.Update)
			{
				battleCreature.Action.SetDirection(Vespa_Unity_Vector3_Extension_Method.VecXZ(forward.normalized), BlendDirection);
			}
			localItem.Callback = callback;
			localItem.BeginTime = localItem.localTimer.time;
			localItem.DurationTime = forward.magnitude / Vespa_Float_Extension_Method.NoneZero((float)callback.SpeedMms * 0.001f);
			localItem.BeginPos = Vespa_Unity_Vector3_Extension_Method.ReplaceY(battleCreature.Position, 0f);
			localItem.Forward = forward;
			localItem.Right = Vector3.Cross(Vector3.up, localItem.Forward);
			localItem.CallbackCount++;
			localItem.IsEnd = false;
			if (!localItem.PendingExecute)
			{
				localItem.Runtime.ExecuteContainer.PendingWeak(this, localItem);
				localItem.PendingExecute = true;
			}
		}
	}

	private void ActionEnd(LocalItem localItem, bool isCancel)
	{
		if (localItem.IsEnd || localItem.Callback == null)
		{
			return;
		}
		if (isCancel)
		{
			Creature battleCreature = localItem.GetBattleCreature(localItem.Callback.CreatureUid);
			if (battleCreature != null)
			{
				Vector3 i_src = localItem.Callback.TargetPosition;
				battleCreature.SetBattlePos(Vespa_Unity_Vector3_Extension_Method.ReplaceY(i_src, 0f), validateBattleArea: true);
			}
		}
		localItem.IsEnd = true;
		CallerEnd.Call(localItem.Runtime, localItem.Variable);
	}

	public void UpdateExecute(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem { IsEnd: false } localItem))
		{
			return;
		}
		Creature battleCreature = localItem.GetBattleCreature(localItem.Callback.CreatureUid);
		if (!(battleCreature == null))
		{
			AnimationCurve i_this = null;
			AnimationCurve i_this2 = null;
			AnimationCurve i_this3 = null;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(MoveCurveList))
			{
				MoveCurveData moveCurveData = MoveCurveList[(localItem.CallbackCount + 1) % MoveCurveList.Count];
				i_this = moveCurveData.ForwardCurve;
				i_this2 = moveCurveData.RightCurve;
				i_this3 = moveCurveData.UpCurve;
			}
			Vector3 position = battleCreature.Position;
			float num = localItem.localTimer.time - localItem.BeginTime;
			float num2 = Mathf.Clamp01(num / Vespa_Float_Extension_Method.NoneZero(localItem.DurationTime));
			float num3 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(i_this, num2, num2);
			float num4 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(i_this2, num2, 0f);
			float num5 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(i_this3, num2, 0f);
			Vector3 vector = localItem.BeginPos + localItem.Forward * num3 + localItem.Right * num4 + Vector3.up * num5 * localItem.Forward.magnitude;
			battleCreature.SetBattlePos(vector, validateBattleArea: true);
			VectorXZ direction = Vespa_Unity_Vector3_Extension_Method.VecXZ(vector - position);
			if (direction.magnitude > Mathf.Epsilon && DirectionType == EDirectionType.Update)
			{
				battleCreature.Action.SetDirection(direction, BlendDirection);
			}
			if (num >= localItem.DurationTime)
			{
				ActionEnd(localItem, isCancel: false);
			}
		}
	}

	public bool IsExecuteEnd(ItemBase itemBase)
	{
		return false;
	}

	public void RequestStopExecute(ItemBase itemBase, ERequestStop requestStop)
	{
		if (itemBase is LocalItem localItem)
		{
			ActionEnd(localItem, isCancel: true);
		}
	}

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
		if (itemBase is LocalItem localItem)
		{
			ActionEnd(localItem, isCancel: true);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData i_json)
	{
		CalleeReady.LOADFROMJSON(i_json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(i_json["CallerEnd"]);
		JsonData i_json2 = i_json["MoveCurveList"];
		if (i_json2.isNone())
		{
			MoveCurveData moveCurveData = new MoveCurveData();
			moveCurveData.ForwardCurve = JsonTools.ToCurve(i_json["ForwardCurve"], moveCurveData.ForwardCurve);
			moveCurveData.RightCurve = JsonTools.ToCurve(i_json["RightCurve"], moveCurveData.RightCurve);
			moveCurveData.UpCurve = JsonTools.ToCurve(i_json["UpCurve"], moveCurveData.UpCurve);
			MoveCurveList.Add(moveCurveData);
		}
		else
		{
			JsonTools.ToList(i_json2, MoveCurveList);
		}
		DirectionType = i_json["DirectionType"].GETENUM(DirectionType);
		BlendDirection = i_json["BlendDirection"].GETBOOLEAN(BlendDirection);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeReady.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddFieldList("MoveCurveList", MoveCurveList);
		jsonData.AddFieldString("DirectionType", Vespa_String_Extension_Method.GetEnumString(DirectionType));
		jsonData.AddFieldBool("BlendDirection", BlendDirection);
		return jsonData;
	}
}
