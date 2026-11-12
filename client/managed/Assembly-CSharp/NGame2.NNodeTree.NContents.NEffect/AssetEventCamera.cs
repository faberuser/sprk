using System;
using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using NGameAsset;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class AssetEventCamera : EffectBase, IKey, IKeyFinalize, IExecute
{
	public class EventCameraEntry : IEventCameraEntry
	{
		public Vector3 CameraTarget;

		public Quaternion CameraRotation;

		public float CameraDistance;

		public float CameraFov;

		public float Weight;

		public float BattleFieldAlpha;

		public float UiOn3dAlpha;

		public bool FadeInEnd;

		int IEventCameraEntry.EventPriority => 0;

		float IEventCameraEntry.BattleFieldAlpha => BattleFieldAlpha;

		float IEventCameraEntry.UiAlpha => 1f;

		float IEventCameraEntry.UiOn3dAlpha => UiOn3dAlpha;

		float IEventCameraEntry.ShakeWeight => 0f;

		bool IEventCameraEntry.FadeInEnd => FadeInEnd;

		void IEventCameraEntry.UpdateCamera(ref Vector3 targetPos, ref Quaternion cameraRot, ref float cameraDistance, ref float fieldOfView)
		{
			targetPos = Vector3.Lerp(targetPos, CameraTarget, Weight);
			cameraRot = Quaternion.Lerp(cameraRot, CameraRotation, Weight);
			cameraDistance = Mathf.Lerp(cameraDistance, CameraDistance, Weight);
			fieldOfView = Mathf.Lerp(fieldOfView, CameraFov, Weight);
		}
	}

	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;

		public bool IsFlip;

		public CameraData CameraData;

		public LocalTimer EventTimer;

		public EventCameraEntry Entry;
	}

	public static string ContentsTypeName = "Effect/" + typeof(AssetEventCamera).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<long> AttachUid;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public ArgumentSingleGeneric<bool> KeepThis;

	public string Code;

	public ArgumentSingleGeneric<bool> IsFlip;

	public ArgumentSingleGeneric<float> PositionScale;

	public ArgumentSingleGeneric<float> OrthoScale;

	public AnimationCurve WeightCurve = new AnimationCurve();

	public AnimationCurve BattleFieldAlphaCurve = new AnimationCurve();

	public AnimationCurve UiOn3dAlphaCurve = new AnimationCurve();

	public bool IsBossIntroTimer;

	public bool FitToFovH;

	public AssetEventCamera(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		AttachUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
		KeepThis = new ArgumentSingleGeneric<bool>(value: false);
		IsFlip = new ArgumentSingleGeneric<bool>(value: false);
		PositionScale = new ArgumentSingleGeneric<float>(1f);
		OrthoScale = new ArgumentSingleGeneric<float>(2f);
	}

	public override void Wakeup(IVariable variable)
	{
		AttachUid.GetValue(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
		KeepThis.GetValue(variable);
		IsFlip.GetValue(variable);
		PositionScale.GetValue(variable);
		OrthoScale.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.IsFlip = IsFlip.GetValue(variable);
		localItem.CameraData = SingletonAuto<CameraContainer>.instance.Get(Code);
		if (localItem.CameraData != null)
		{
			localItem.Entry = new EventCameraEntry();
			UpdateEventCamera(localItem);
			runtime.SCENECONTAINER().GETCAMERA()?.SetEventEntry(localItem.Entry);
		}
		int uid = (int)AttachUid.GetValue(localItem.Variable);
		RenderFacade renderFacade = runtime.SceneContainer.GetRenderFacade(uid);
		if (renderFacade != null && IsBossIntroTimer)
		{
			localItem.EventTimer = renderFacade.LocalTimer;
			localItem.EventTimer.EventSet(ETimerEventFlag.BossIntroPlaying, isSet: true);
			MainTimer.EventSet(ETimerEventFlag.BossIntroPause, isSet: true);
		}
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			if (localItem.Entry != null && !KeepThis.GetValue(localItem.Variable, defaultValue: false))
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
		if (localItem.Entry == null || localItem.CameraData == null)
		{
			return;
		}
		long value = AttachUid.GetValue(localItem.Variable);
		RenderFacade renderFacade = localItem.GetRenderFacade((int)value);
		if (renderFacade == null)
		{
			return;
		}
		Transform pivot = renderFacade.AssetCreature.GetPivot(EPivotType.AssetRoot);
		if (!(pivot == null))
		{
			float value2 = PositionScale.GetValue(localItem.Variable, 1f);
			float value3 = OrthoScale.GetValue(localItem.Variable, 1f);
			float num = Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable)));
			Vector3 vector = localItem.CameraData.PositionCurve.Evaluate(num) * value2;
			Vector3 vector2 = localItem.CameraData.TargetCurve.Evaluate(num) * value2;
			float num2 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(localItem.CameraData.OrthoCurve, num, 1f) * value3;
			float num3 = localItem.CameraData.RollAngle.Evaluate(num) * value3;
			if (FitToFovH)
			{
				num2 = Mathf.Atan(Mathf.Tan(num2 * 0.5f * (MathF.PI / 180f)) * (16f * (float)Screen.height / (9f * (float)Screen.width))) * 2f * 57.29578f;
			}
			if (localItem.IsFlip)
			{
				vector.x = 0f - vector.x;
				vector2.x = 0f - vector2.x;
			}
			if (pivot != null)
			{
				vector = pivot.localToWorldMatrix.MultiplyPoint(vector);
				vector2 = pivot.localToWorldMatrix.MultiplyPoint(vector2);
			}
			Vector3 eulerAngles = Quaternion.LookRotation(vector2 - vector).eulerAngles;
			localItem.Entry.CameraRotation.eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y, eulerAngles.z + num3);
			localItem.Entry.CameraTarget = vector2;
			localItem.Entry.CameraDistance = Vector3.Distance(vector2, vector);
			localItem.Entry.CameraFov = num2;
			localItem.Entry.Weight = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(WeightCurve, num, 1f);
			localItem.Entry.BattleFieldAlpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(BattleFieldAlphaCurve, num, 1f);
			localItem.Entry.UiOn3dAlpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(UiOn3dAlphaCurve, num, 1f);
			if (NVespa.Math.Approximately(localItem.Entry.Weight, 1f))
			{
				localItem.Entry.FadeInEnd = true;
			}
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

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
		if (itemBase is LocalItem { EventTimer: not null } localItem && IsBossIntroTimer)
		{
			localItem.EventTimer.EventSet(ETimerEventFlag.BossIntroPlaying, isSet: false);
			MainTimer.EventSet(ETimerEventFlag.BossIntroPause, isSet: false);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		AttachUid.LoadFromJson(json["AttachUid"]);
		Code = json["Code"].GETSTRING(Code);
		IsFlip.LoadFromJson(json["IsFlip"]);
		PositionScale.LoadFromJson(json["PositionScale"]);
		OrthoScale.LoadFromJson(json["OrthoScale"]);
		WeightCurve = json["WeightCurve"].GetAnimationCurve(WeightCurve);
		BattleFieldAlphaCurve = json["BattleFieldAlphaCurve"].GetAnimationCurve(BattleFieldAlphaCurve);
		UiOn3dAlphaCurve = json["UiOn3dAlphaCurve"].GetAnimationCurve(UiOn3dAlphaCurve);
		IsBossIntroTimer = json.GetJsonAny("IsBossIntro", "IsBossIntroTimer").GETBOOLEAN(IsBossIntroTimer);
		FitToFovH = json["FitToFovH"].GETBOOLEAN(FitToFovH);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		string text = "KeepThis";
		if (json.Contains(text))
		{
			KeepThis.LoadFromJson(json[text]);
		}
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("AttachUid", AttachUid.MakeJson());
		jsonData.AddFieldString("Code", Code);
		jsonData.AddField("IsFlip", IsFlip.MakeJson());
		jsonData.AddField("PositionScale", PositionScale.MakeJson());
		jsonData.AddField("OrthoScale", OrthoScale.MakeJson());
		jsonData.AddFieldAnimationCurve("WeightCurve", WeightCurve);
		jsonData.AddFieldAnimationCurve("BattleFieldAlphaCurve", BattleFieldAlphaCurve);
		jsonData.AddFieldAnimationCurve("UiOn3dAlphaCurve", UiOn3dAlphaCurve);
		jsonData.AddFieldBool("IsBossIntroTimer", IsBossIntroTimer);
		jsonData.AddFieldBool("FitToFovH", FitToFovH);
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddField("KeepThis", KeepThis.MakeJson());
		return jsonData;
	}
}
