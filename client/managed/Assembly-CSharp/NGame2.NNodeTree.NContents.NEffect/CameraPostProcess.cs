using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NNodeTree.NTypeValue;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CameraPostProcess : EffectBase, IKey, IExecute
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;

		public PostProcessVolume PostProcessVolume;
	}

	public static string ContentsTypeName = "Effect/" + typeof(CameraPostProcess).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public bool StopWithNodeTree;

	private int _profileChoiceIndex;

	private string[] _allProfiles;

	public string ProfileName = string.Empty;

	public ArgumentSingleGeneric<float> BlendWeight;

	public int Priority;

	public CameraPostProcess(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		BlendWeight = new ArgumentSingleGeneric<float>(1f);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		ICamera cAMERA = runtime.SCENECONTAINER().GETCAMERA();
		if (cAMERA != null)
		{
			GameObject gameObject = new GameObject("CameraPostProcess");
			PostProcessVolume componentAuto = Vespa_Unity_GameObject_Extension_Method.GetComponentAuto<PostProcessVolume>(gameObject);
			componentAuto.profile = VespaResource.Load<PostProcessProfile>(ProfileName);
			componentAuto.isGlobal = true;
			componentAuto.priority = Priority;
			gameObject.transform.parent = cAMERA.GetUnityCamera().transform;
			localItem.PostProcessVolume = componentAuto;
		}
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			if (localItem.PostProcessVolume != null)
			{
				Object.Destroy(localItem.PostProcessVolume.gameObject);
			}
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			UpdateInternal(localItem);
			if (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
			}
		}
	}

	private float GetFrame(LocalItem localItem)
	{
		return Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable)));
	}

	private void UpdateInternal(LocalItem localItem)
	{
		if (!(localItem.PostProcessVolume == null))
		{
			localItem.PostProcessVolume.weight = BlendWeight.GetValue(localItem.Variable);
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
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		ProfileName = json["ProfileName"].GETSTRING(ProfileName);
		BlendWeight.LoadFromJson(json["BlendWeight"]);
		Priority = json["Priority"].GETINT(Priority);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldString("ProfileName", ProfileName);
		jsonData.AddField("BlendWeight", BlendWeight.MakeJson());
		jsonData.AddFieldLong("Priority", Priority);
		return jsonData;
	}
}
