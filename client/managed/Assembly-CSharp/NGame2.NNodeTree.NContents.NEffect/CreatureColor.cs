using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreatureColor : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class ColorEffectValue : ColorEffect.IValue
	{
		private LocalTimer _localTimer;

		private Gradient _color;

		private float _loopTime;

		private float _blendTime;

		public bool IsEnd { get; private set; }

		public float BeginTime { get; private set; }

		public float EndTime { get; private set; }

		public void Begin(LocalTimer localTimer, Gradient colorGradient, float loopTime, float blendTime)
		{
			_localTimer = localTimer;
			_color = colorGradient;
			_loopTime = Mathf.Max(loopTime, Mathf.Epsilon * 2f);
			_blendTime = Mathf.Max(blendTime, Mathf.Epsilon * 2f);
			IsEnd = false;
			BeginTime = localTimer.time;
		}

		public void End()
		{
			IsEnd = true;
			EndTime = _localTimer.time;
		}

		bool ColorEffect.IValue.IsEnd()
		{
			if (!IsEnd)
			{
				return false;
			}
			return _localTimer.time - EndTime > _blendTime;
		}

		Color ColorEffect.IValue.GetColor()
		{
			Color currentColor = GetCurrentColor();
			if (IsEnd)
			{
				float t = Mathf.Clamp01((_localTimer.time - EndTime) / _blendTime);
				return Color.Lerp(currentColor, Colors.zero, t);
			}
			float t2 = Mathf.Clamp01((_localTimer.time - BeginTime) / _blendTime);
			return Color.Lerp(Colors.zero, currentColor, t2);
		}

		private Color GetCurrentColor()
		{
			float i_frame = Mathf.Repeat(_localTimer.time - BeginTime, _loopTime) / _loopTime;
			return Vespa_Unity_Gradient_Extension_Method.EVALUATE(_color, i_frame, Colors.zero);
		}
	}

	public class LocalItem : ItemBase
	{
		public ColorEffectValue ColorEffect = new ColorEffectValue();
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreatureColor).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<long> CreatureUid;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public Gradient ColorGradient = new Gradient();

	public float LoopTime = 1f;

	public float BlendTime;

	public CreatureColor(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		CreatureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		CreatureUid.GetValue(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.ColorEffect.Begin(runtime.LocalTimer, ColorGradient, LoopTime, BlendTime);
		long value = CreatureUid.GetValue(variable, 0L);
		RenderFacade renderFacade = runtime.SceneContainer.GetRenderFacade((int)value);
		if (renderFacade != null)
		{
			renderFacade.ColorEffect.AddValue(localItem.ColorEffect);
		}
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.ColorEffect.IsEnd)
		{
			localItem.ColorEffect.End();
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem localItem && !localItem.ColorEffect.IsEnd && (localItem.localTimer.time - localItem.ColorEffect.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false)))
		{
			ActionEnd(localItem);
		}
	}

	bool IExecute.IsExecuteEnd(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem localItem))
		{
			return true;
		}
		return localItem.ColorEffect.IsEnd;
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
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		ColorGradient = JsonTools.ToGradient(json["ColorGradient"], ColorGradient);
		LoopTime = json["LoopTime"].GETFLOAT(LoopTime);
		BlendTime = json["BlendTime"].GETFLOAT(BlendTime);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddField("ColorGradient", JsonTools.FromGradient(ColorGradient));
		jsonData.AddFieldDouble("LoopTime", LoopTime);
		jsonData.AddFieldDouble("BlendTime", BlendTime);
		return jsonData;
	}
}
