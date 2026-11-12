using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NNodeTree.NTypeValue;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NBattle;

public class FocusBattleCreature : BattleBase, IKey
{
	public class FocusCameraEntry : IFocusCameraEntry
	{
		public float TimeLength;

		public AnimationCurve WeightCurve;

		public Creature Creature;

		public Vector3 Offset;

		public float Distance;

		private int _updateFrameCount;

		private Vector3 _position;

		public float Weight { get; private set; }

		public bool Released { get; private set; }

		void IFocusCameraEntry.UpdateCamera(ref Vector3 targetPos, ref Quaternion cameraRot, ref float cameraDistance, ref float fieldOfView)
		{
			UpdateCameraData();
			float t = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(WeightCurve, Weight, Weight);
			targetPos = Vector3.Lerp(targetPos, _position, t);
			cameraDistance = Mathf.Lerp(cameraDistance, Distance, t);
		}

		void IFocusCameraEntry.ReleaseFocus()
		{
			Released = true;
		}

		private void UpdateCameraData()
		{
			if (_updateFrameCount != Time.frameCount)
			{
				_updateFrameCount = Time.frameCount;
				float num = GlobalTimer.deltaTime / Vespa_Float_Extension_Method.NoneZero(TimeLength);
				if (!Released)
				{
					Weight = Mathf.Clamp01(Weight + num);
				}
				else
				{
					Weight = Mathf.Clamp01(Weight - num);
				}
				if (Creature != null)
				{
					_position = Creature.transform.position + Offset;
				}
			}
		}
	}

	public static string ContentsTypeName = "Battle/" + typeof(FocusBattleCreature).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> CreatureUid;

	public ArgumentSingleGeneric<Vector3> Offset;

	public float TimeLength = 0.5f;

	public float Distance = 5f;

	public AnimationCurve WeightCurve = new AnimationCurve();

	public FocusBattleCreature(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		CreatureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
		Offset = new ArgumentSingleGeneric<Vector3>(Vector3.zero);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		CreateCameraFocusEntry(runtime, variable);
		CallerEnd.Call(runtime, variable);
	}

	private void CreateCameraFocusEntry(RuntimeInstance runtime, IVariable variable)
	{
		ICamera cAMERA = runtime.SCENECONTAINER().GETCAMERA();
		if (cAMERA != null)
		{
			Creature bATTLECREATURE = runtime.SCENECONTAINER().GETBATTLECREATURE((int)CreatureUid.GetValue(variable));
			if (bATTLECREATURE == null)
			{
				cAMERA.SetFocusEntry(null);
				return;
			}
			FocusCameraEntry focusCameraEntry = new FocusCameraEntry();
			focusCameraEntry.TimeLength = TimeLength;
			focusCameraEntry.WeightCurve = WeightCurve;
			focusCameraEntry.Creature = bATTLECREATURE;
			focusCameraEntry.Offset = Offset.GetValue(variable);
			focusCameraEntry.Distance = Distance;
			cAMERA.SetFocusEntry(focusCameraEntry);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		Offset.LoadFromJson(json["Offset"]);
		TimeLength = json["TimeLength"].GETFLOAT(TimeLength);
		Distance = json["Distance"].GETFLOAT(Distance);
		WeightCurve = json["WeightCurve"].GetAnimationCurve(WeightCurve);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddField("Offset", Offset.MakeJson());
		jsonData.AddFieldDouble("TimeLength", TimeLength);
		jsonData.AddFieldDouble("Distance", Distance);
		jsonData.AddFieldAnimationCurve("WeightCurve", WeightCurve);
		return jsonData;
	}
}
