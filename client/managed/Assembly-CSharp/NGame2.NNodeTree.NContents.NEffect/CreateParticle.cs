using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NTypeValue.NModifier;
using NGame2.NScene;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreateParticle : EffectBase, IKey, IExecute
{
	public class LocalItem : ItemBase, TransformModifier.IItem
	{
		public bool IsEnd;

		public float BeginTime;

		public long Uid;

		private bool[] _isBurst;

		public Vector3 PrevPosition { get; set; }

		public bool IsAlreadyBurst(int index)
		{
			if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_isBurst, index))
			{
				return false;
			}
			return _isBurst[index];
		}

		public void OnBurstParticle(int index, int count)
		{
			if (_isBurst == null)
			{
				_isBurst = new bool[count];
			}
			_isBurst[index] = true;
		}
	}

	public class ParticleBurst : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public float Frame;

		public int Count;

		bool ILoadJson.Load(JsonData json)
		{
			Frame = json["Frame"].GETFLOAT(Frame);
			Count = json["Count"].GETINT(Count);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldDouble("Frame", Frame);
			jsonData.AddFieldLong("Count", Count);
			return jsonData;
		}
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreateParticle).Name;

	public ChainCall ChainCall;

	public PrefabGoAuto PrefabAuto = new PrefabGoAuto(string.Empty, isGlobal: false, EPrefabType.Default, isPooling: true, touchMaterial: true);

	public bool IsDistanceEmit;

	public ArgumentSingleGeneric<Transform> ParentTransform;

	public TransformModifier TransformModifier;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public DefineVariable<long> DefineUid = new DefineVariable<long>();

	public DefineVariable<float> DefineFrame = new DefineVariable<float>();

	public List<ParticleBurst> BurstList = new List<ParticleBurst>();

	public CreateParticle(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		ParentTransform = new ArgumentSingleGeneric<Transform>(StringKey.Empty);
		TransformModifier = new TransformModifier();
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		ParentTransform.GetValue(variable);
		TransformModifier.Wakeup(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		if (!CheckShowSkillEffect(runtime, variable))
		{
			return variable;
		}
		Transform value = ParentTransform.GetValue(variable);
		ParticleObject particleObject = NGame2.NNodeTree.NObject.Container.instance.CreateParticle(PrefabAuto, IsDistanceEmit, value, runtime);
		if (particleObject == null)
		{
			return variable;
		}
		if (particleObject == null)
		{
			return variable;
		}
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.Uid = particleObject.Uid;
		DefineUid.SetValue(localItem, localItem.Uid);
		DefineFrame.SetFunc(localItem, () => Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable))));
		TransformModifier.InitTransform(particleObject.transform, localItem.Variable, localItem);
		particleObject.Play = true;
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			NGame2.NNodeTree.NObject.Container.instance.ReleaseObjectUid(localItem.Uid);
			localItem.Uid = 0L;
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			UpdateParticle(localItem);
			if (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
			}
		}
	}

	private void UpdateParticle(LocalItem localItem)
	{
		ParticleObject particleObject = NGame2.NNodeTree.NObject.Container.instance.GetObjectBase(localItem.Uid) as ParticleObject;
		if (particleObject == null)
		{
			return;
		}
		TransformModifier.UpdateTransform(particleObject.transform, localItem.Variable, localItem);
		float num = Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable)));
		int num2 = __Extension_Method_ICollection__.COUNT(BurstList);
		for (int i = 0; i < num2; i++)
		{
			ParticleBurst particleBurst = BurstList[i];
			if (!(num < particleBurst.Frame) && !localItem.IsAlreadyBurst(i))
			{
				particleObject.Burst(particleBurst.Count);
				localItem.OnBurstParticle(i, num2);
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

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
		Action action = delegate
		{
			PrefabAuto.ReserveFactory(5);
		};
		preloader.AddAction(action, string.Empty);
	}

	private bool CheckShowSkillEffect(RuntimeInstance runtime, IVariable variable)
	{
		if (!SystemOptionData.Render_ShowSkillEffect)
		{
			return false;
		}
		int uid = (int)variable.GetValue<long>(VariableKeyword.Myself);
		Creature battleCreature = runtime.SceneContainer.GetBattleCreature(uid);
		if (battleCreature != null)
		{
			if (battleCreature.IsLeftTeam)
			{
				return SystemOptionData.Render_ShowAllySkillEffect;
			}
			return SystemOptionData.Render_ShowEnemySkillEffect;
		}
		return true;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		PrefabAuto.LOADFROMJSON(json.GetJsonAny("PrefabAuto", "PrefabPath"));
		IsDistanceEmit = json["IsDistanceEmit"].GETBOOLEAN(IsDistanceEmit);
		ParentTransform.LoadFromJson(json["ParentTransform"]);
		TransformModifier.LOADFROMJSON(json["TransformModifier"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		DefineUid.LOADFROMJSON(json["DefineUid"]);
		DefineFrame.LOADFROMJSON(json["DefineFrame"]);
		JsonTools.ToList(json["BurstList"], BurstList);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddFieldJsonMake("PrefabAuto", PrefabAuto);
		jsonData.AddFieldBool("IsDistanceEmit", IsDistanceEmit);
		jsonData.AddField("ParentTransform", ParentTransform.MakeJson());
		jsonData.AddFieldJsonMake("TransformModifier", TransformModifier);
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldJsonMake("DefineUid", DefineUid);
		jsonData.AddFieldJsonMake("DefineFrame", DefineFrame);
		jsonData.AddField("BurstList", JsonTools.FromList(BurstList));
		return jsonData;
	}
}
