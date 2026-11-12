using LitJson;
using NGame2.NBattleContext;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NNodeTree.NTypeValue;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class ModifyEventCamera2 : EffectBase, IKey, IExecute
{
	public enum ECameraType
	{
		None,
		Skill,
		LastKill,
		BossIntro,
		CutScene
	}

	public class EventCameraEntry : IEventCameraEntry
	{
		public Tuple<Vector3, float> WeightTarget = new Tuple<Vector3, float>(Vector3.zero, 0f);

		public Tuple<Quaternion, float> WeightRotation = new Tuple<Quaternion, float>(Quaternion.identity, 0f);

		public Tuple<float, float> WeightDistance = new Tuple<float, float>(0f, 0f);

		public Tuple<float, float> WeightFov = new Tuple<float, float>(0f, 0f);

		public int EventPriority { get; private set; }

		public float BattleFieldAlpha { get; set; }

		public float UiAlpha { get; set; }

		public float UiOn3dAlpha { get; set; }

		public float ShakeWeight { get; set; }

		public bool FadeInEnd { get; set; }

		void IEventCameraEntry.UpdateCamera(ref Vector3 targetPos, ref Quaternion cameraRot, ref float cameraDistance, ref float fieldOfView)
		{
			targetPos = Vector3.Lerp(targetPos, WeightTarget.Item1, WeightTarget.Item2);
			cameraRot = Quaternion.Lerp(cameraRot, WeightRotation.Item1, WeightRotation.Item2);
			cameraDistance = Mathf.Lerp(cameraDistance, WeightDistance.Item1, WeightDistance.Item2);
			if (WeightFov.Item1 > Mathf.Epsilon)
			{
				fieldOfView = Mathf.Lerp(fieldOfView, WeightFov.Item1, WeightFov.Item2);
			}
		}

		public EventCameraEntry(int priority)
		{
			EventPriority = priority;
		}
	}

	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;

		public EventCameraEntry Entry;
	}

	public static string ContentsTypeName = "Effect/" + typeof(ModifyEventCamera2).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public ECameraType CameraType;

	public bool StopWithNodeTree;

	public int EventPriority;

	public float FadeInEndFrame = 0.5f;

	public AnimationCurve BattleFieldAlpha = new AnimationCurve();

	public AnimationCurve UiAlpha = new AnimationCurve();

	public AnimationCurve UiOn3dAlpha = new AnimationCurve();

	public AnimationCurve ShakeWeight = new AnimationCurve();

	public ArgumentSingleGeneric<Vector3> CameraTarget;

	public AnimationCurve TargetWeight = new AnimationCurve();

	public ArgumentSingleGeneric<Vector3> CameraDirection;

	public AnimationCurve DirectionWeight = new AnimationCurve();

	public ArgumentSingleGeneric<float> CameraDistance;

	public AnimationCurve DistanceWeight = new AnimationCurve();

	public ArgumentSingleGeneric<float> CameraFov;

	public AnimationCurve FovWeight = new AnimationCurve();

	public DefineVariable<float> DefineFrame = new DefineVariable<float>();

	public ModifyEventCamera2(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
		CameraTarget = new ArgumentSingleGeneric<Vector3>(Vector3.zero);
		CameraDirection = new ArgumentSingleGeneric<Vector3>(Vector3.forward);
		CameraDistance = new ArgumentSingleGeneric<float>(7f);
		CameraFov = new ArgumentSingleGeneric<float>(0f);
	}

	public override void Wakeup(IVariable variable)
	{
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
		CameraTarget.GetValue(variable);
		CameraDirection.GetValue(variable);
		CameraDistance.GetValue(variable);
		CameraFov.GetValue(variable);
	}

	private bool IsSkillCameraEnabled()
	{
		if (GameSharedSetting.data.Value.GetTimerEvent().DisableSkillCamera)
		{
			return false;
		}
		if (SingletonAuto<BattleContext>.isValidInstance && SingletonAuto<BattleContext>.instance.DataProvider != null)
		{
			BattleDefineData battleDefine = SingletonAuto<BattleContext>.instance.DataProvider.GetBattleDefine();
			if (battleDefine != null && battleDefine.SkillCameraOff)
			{
				return false;
			}
		}
		return GameOptionData.Render_EnableSkillCamera;
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		if (CameraType != ECameraType.Skill || IsSkillCameraEnabled())
		{
			localItem.Entry = new EventCameraEntry(EventPriority);
		}
		UpdateEventCamera(localItem);
		ICamera cAMERA = runtime.SCENECONTAINER().GETCAMERA();
		if (cAMERA != null && localItem.Entry != null)
		{
			cAMERA.SetEventEntry(localItem.Entry);
		}
		DefineFrame.SetFunc(localItem, () => GetFrame(localItem));
		if (StopWithNodeTree)
		{
			runtime.ExecuteContainer.PendingWeak(this, localItem);
		}
		else
		{
			runtime.ExecuteContainer.Pending(this, localItem);
		}
		return localItem.Variable;
	}

	private float GetFrame(LocalItem localItem)
	{
		return Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable)));
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			if (localItem.Entry != null)
			{
				localItem.Runtime.SCENECONTAINER().GETCAMERA()?.ReleaseEventEntry(localItem.Entry);
				localItem.Entry = null;
			}
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			UpdateEventCamera(localItem);
			if (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
			}
		}
	}

	private void UpdateEventCamera(LocalItem localItem)
	{
		if (localItem.Entry != null)
		{
			float frame = GetFrame(localItem);
			localItem.Entry.BattleFieldAlpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(BattleFieldAlpha, frame, 1f);
			localItem.Entry.UiAlpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(UiAlpha, frame, 1f);
			localItem.Entry.UiOn3dAlpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(UiOn3dAlpha, frame, 1f);
			localItem.Entry.ShakeWeight = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(ShakeWeight, frame, 1f);
			localItem.Entry.FadeInEnd = frame > FadeInEndFrame;
			localItem.Entry.WeightTarget.Item1 = CameraTarget.GetValue(localItem.Variable);
			localItem.Entry.WeightTarget.Item2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(TargetWeight, frame, 1f);
			Vector3 value = CameraDirection.GetValue(localItem.Variable);
			localItem.Entry.WeightRotation.Item1 = Quaternion.LookRotation(value);
			localItem.Entry.WeightRotation.Item2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(DirectionWeight, frame, 1f);
			localItem.Entry.WeightDistance.Item1 = CameraDistance.GetValue(localItem.Variable);
			localItem.Entry.WeightDistance.Item2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(DistanceWeight, frame, 1f);
			localItem.Entry.WeightFov.Item1 = CameraFov.GetValue(localItem.Variable);
			localItem.Entry.WeightFov.Item2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(FovWeight, frame, 1f);
		}
	}

	bool IExecute.IsExecuteEnd(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem localItem))
		{
			return true;
		}
		return localItem.IsEnd;
	}

	void IExecute.RequestStopExecute(ItemBase itemBase, ERequestStop requestStop)
	{
		if (itemBase is LocalItem localItem)
		{
			ActionEnd(localItem);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		CameraType = json["CameraType"].GETENUM(CameraType);
		StopWithNodeTree = json["StopWithNodeTree"].GETBOOLEAN(StopWithNodeTree);
		EventPriority = json["EventPriority"].GETINT(EventPriority);
		FadeInEndFrame = json["FadeInEndFrame"].GETFLOAT(FadeInEndFrame);
		BattleFieldAlpha = json["BattleFieldAlpha"].GetAnimationCurve(BattleFieldAlpha);
		UiAlpha = json["UiAlpha"].GetAnimationCurve(UiAlpha);
		UiOn3dAlpha = json["UiOn3dAlpha"].GetAnimationCurve(UiOn3dAlpha);
		ShakeWeight = json["ShakeWeight"].GetAnimationCurve(ShakeWeight);
		CameraTarget.LoadFromJson(json["CameraTarget"]);
		TargetWeight = json["TargetWeight"].GetAnimationCurve(TargetWeight);
		CameraDirection.LoadFromJson(json["CameraDirection"]);
		DirectionWeight = json["DirectionWeight"].GetAnimationCurve(DirectionWeight);
		CameraDistance.LoadFromJson(json["CameraDistance"]);
		DistanceWeight = json["DistanceWeight"].GetAnimationCurve(DistanceWeight);
		CameraFov.LoadFromJson(json["CameraFov"]);
		FovWeight = json["FovWeight"].GetAnimationCurve(FovWeight);
		DefineFrame.LOADFROMJSON(json["DefineFrame"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldEnum("CameraType", CameraType);
		jsonData.AddFieldBool("StopWithNodeTree", StopWithNodeTree);
		jsonData.AddFieldLong("EventPriority", EventPriority);
		jsonData.AddFieldDouble("FadeInEndFrame", FadeInEndFrame);
		jsonData.AddFieldAnimationCurve("BattleFieldAlpha", BattleFieldAlpha);
		jsonData.AddFieldAnimationCurve("UiAlpha", UiAlpha);
		jsonData.AddFieldAnimationCurve("UiOn3dAlpha", UiOn3dAlpha);
		jsonData.AddFieldAnimationCurve("ShakeWeight", ShakeWeight);
		jsonData.AddField("CameraTarget", CameraTarget.MakeJson());
		jsonData.AddFieldAnimationCurve("TargetWeight", TargetWeight);
		jsonData.AddField("CameraDirection", CameraDirection.MakeJson());
		jsonData.AddFieldAnimationCurve("DirectionWeight", DirectionWeight);
		jsonData.AddField("CameraDistance", CameraDistance.MakeJson());
		jsonData.AddFieldAnimationCurve("DistanceWeight", DistanceWeight);
		jsonData.AddField("CameraFov", CameraFov.MakeJson());
		jsonData.AddFieldAnimationCurve("FovWeight", FovWeight);
		jsonData.AddFieldJsonMake("DefineFrame", DefineFrame);
		return jsonData;
	}
}
