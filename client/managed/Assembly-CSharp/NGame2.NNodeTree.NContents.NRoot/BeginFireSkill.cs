using System.Collections.Generic;
using LitJson;
using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NRuntime.NVariable;
using NGame2.NNodeTree.NUtil;
using NGame2.NRender;
using NGame2.NScene;
using NGameAsset;
using NGameAsset.NSound;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NRoot;

public class BeginFireSkill : RootBase, IKey, IKeyFinalize, IExecute, RuntimeInstance.IRuntimeStart, Holder.ICollector
{
	public class RootVariable : SmallPooling<RootVariable>, IVariable, ISmallPoolingItem
	{
		private bool _reservedMemberVariable;

		private RuntimeInstance _runtime;

		private NShared.BeginFireSkill _packet;

		private SkillData _skillData;

		private ValueGenericMember<long> _myself;

		private ValueGenericMember<long> _target;

		private ValueGenericMember<float> _durationTime;

		private ValueGenericMember<float> _actingTime;

		private ValueGenericMember<float> _soloTime;

		private StringKeyDictionary<IVariableValueBase> _dictionary = new StringKeyDictionary<IVariableValueBase>();

		void ISmallPoolingItem.OnCreateNew()
		{
			ReserveMemberVariable();
		}

		void ISmallPoolingItem.OnDestroy()
		{
		}

		void ISmallPoolingItem.OnCreate()
		{
		}

		void ISmallPoolingItem.OnRelease()
		{
			_skillData = null;
			_packet = null;
			_runtime = null;
		}

		private void ReserveMemberVariable()
		{
			if (!_reservedMemberVariable)
			{
				_reservedMemberVariable = true;
				_myself = ValueGenericMember<long>.Create(0L);
				_target = ValueGenericMember<long>.Create(0L);
				_durationTime = ValueGenericMember<float>.Create(0f);
				_actingTime = ValueGenericMember<float>.Create(0f);
				_soloTime = ValueGenericMember<float>.Create(0f);
				_dictionary.Add(VariableKeyword.Myself, _myself);
				_dictionary.Add(VariableKeyword.Target, _target);
				_dictionary.Add(VariableKeyword.Direction, new ValueGenericFunc<Vector3>(GetDirectionValue));
				_dictionary.Add(VariableKeyword.DurationTime, _durationTime);
				_dictionary.Add(VariableKeyword.ActingTime, _actingTime);
				_dictionary.Add(Keyword_SoloTime, _soloTime);
				_dictionary.Add(VariableKeyword.Frame, new ValueGenericFunc<float>(GetFrameValue));
			}
		}

		internal bool Preopen(NShared.BeginFireSkill packet)
		{
			_skillData = GameRoot.Instance.SkillDataContainer.GetData(packet.SkillId);
			if (_skillData == null)
			{
				return false;
			}
			_packet = packet;
			return true;
		}

		void IVariable.Open(RuntimeInstance runtime)
		{
			_runtime = runtime;
			_myself.Value = _packet.CreatureUid;
			_target.Value = _packet.FirstTargetCreatureUid;
			_durationTime.Value = (float)_packet.DurationTimeMs * 0.001f;
			_actingTime.Value = (float)_skillData.ActingTimeMs * 0.001f;
			_soloTime.Value = (float)_skillData.GetSoloTimeMs(_packet.IsIgnoreSoloTime) * 0.001f;
		}

		IVariable IVariable.CaptureSelf()
		{
			return SmallPooling<RootVariable>.CaptureInstance(this);
		}

		void IVariable.ReleaseSelf()
		{
			SmallPooling<RootVariable>.ReleaseInstance(this);
		}

		RuntimeInstance IVariable.GetRuntimeInstance()
		{
			return _runtime;
		}

		IVariableValueBase IVariable.GetValueBase(StringKey key)
		{
			if (key.IsNullOrEmpty())
			{
				return null;
			}
			if (_dictionary.TryGetValue(key, out var value))
			{
				return value;
			}
			return SingletonAuto<EnvironmentVariable>.instance.GetValueBase(key);
		}

		JsonData IVariable.DumpJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("Variable Type", "BeginFireSkill");
			foreach (KeyValuePair<StringKey, IVariableValueBase> item in _dictionary.CreateDictionary())
			{
				JsonData value = item.Value.DumpJson();
				jsonData.AddField(item.Key.Value, value);
			}
			return jsonData;
		}

		private Vector3 GetDirectionValue()
		{
			Creature battleCreature = _runtime.SceneContainer.GetBattleCreature(_packet.CreatureUid);
			Vector3 vector = GetTargetPosition() - battleCreature.Position;
			if (vector.magnitude > Mathf.Epsilon)
			{
				return vector.normalized;
			}
			return battleCreature.Action.GetDirection().ToVec3();
		}

		private float GetFrameValue()
		{
			if (!_runtime.Variable.TryGetValue<float>(VariableKeyword.BeginTime, out var o_value))
			{
				return 0f;
			}
			return Mathf.Clamp01((_runtime.LocalTimer.time - o_value) / Vespa_Float_Extension_Method.NoneZero(_durationTime.Value));
		}

		private Vector3 GetTargetPosition()
		{
			RenderFacade renderFacade = _runtime.SceneContainer.GetRenderFacade(_packet.FirstTargetCreatureUid);
			if (renderFacade != null)
			{
				return renderFacade.transform.position;
			}
			return _packet.TargetPosition;
		}
	}

	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public float BeginTime;

		public float DurationTime;

		public ISound Sound;

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Root/" + typeof(BeginFireSkill).Name;

	public static readonly StringKey Keyword_SoloTime = StringKey.Get("SoloTime");

	public Caller Caller;

	public int SkillIndex;

	public bool RetargetOnFire;

	public bool IsActionSkill = true;

	public bool PlaySkillSound = true;

	public EStopContentsType StopSoundType;

	private DrawRuntimeVariableHelper _drawVariableHelper = new DrawRuntimeVariableHelper();

	public BeginFireSkill(NodeData i_node)
		: base(i_node)
	{
		Caller = new Caller(this, "Caller");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	void Holder.ICollector.Collect(Holder holder)
	{
		holder.AddBySkillIndex<CallbackMessage<NShared.BeginFireSkill>>(CreatureManager.Verifier, OnMessageFast, SkillIndex);
	}

	bool RuntimeInstance.IRuntimeStart.Start(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.DurationTime = variable.GetValue<float>(VariableKeyword.DurationTime);
		int num = (int)localItem.Variable.GetValue<long>(VariableKeyword.Myself);
		CreateSkillSound(runtime, localItem, num);
		runtime.ExecuteContainer.PendingWeak(this, localItem);
		Caller.Call(runtime, localItem.Variable);
		localItem.AddOnMessageByCreatureUid<LocalItem, CallbackMessage<NShared.FireSkill>>(localItem, LocalItemOnMessageFast, num);
		localItem.AddOnMessageByCreatureUid<LocalItem, CallbackMessage<EndFireSkill>>(localItem, LocalItemOnMessageFast, num);
		return true;
	}

	private void ActionEnd(LocalItem localItem, ERequestStop requestStop)
	{
		if (localItem.IsEnd)
		{
			return;
		}
		bool flag = StopSoundType == EStopContentsType.StopWithThis || ((StopSoundType == EStopContentsType.StopIfRequestStop) ? (requestStop != ERequestStop.None) : ((StopSoundType == EStopContentsType.StopIfBattleCancel) ? (requestStop == ERequestStop.BattleCancel) : (StopSoundType != EStopContentsType.None)));
		if (localItem.Sound != null)
		{
			if (flag || localItem.Sound.ContainsLoop)
			{
				SoundContainer.ReleaseSound(localItem.Sound);
			}
			else
			{
				SingletonUpdater<AssetUpdateManager>.instance.Add(localItem.Sound, 60f);
			}
			localItem.Sound = null;
		}
		localItem.IsEnd = true;
	}

	private void CreateSkillSound(RuntimeInstance runtime, LocalItem localItem, int creatureUid)
	{
		if (!PlaySkillSound)
		{
			return;
		}
		Creature bATTLECREATURE = runtime.SCENECONTAINER().GETBATTLECREATURE(creatureUid);
		if (bATTLECREATURE == null || bATTLECREATURE.CreateInfo == null)
		{
			return;
		}
		CachedHeroInfo heroInfo = bATTLECREATURE.CreateInfo.HeroInfo;
		string prefix = "BeginFireSkill";
		string heroCode = SkillIndex.ToString();
		string costumeVoicePostfix = SoundHelper.GetCostumeVoicePostfix(heroInfo);
		string soundCode = SoundHelper.GetSoundCode(prefix, heroCode, costumeVoicePostfix);
		SoundStock stock = new SoundStock
		{
			Code = soundCode,
			LocalTimer = localItem.localTimer,
			MixerFlag = EAudioMixerFlag.Skill
		};
		ISound sound = SingletonAuto<SoundContainer>.instance.CreateSoundExact(stock);
		if (sound != null)
		{
			if (StopSoundType == EStopContentsType.None && !sound.ContainsLoop)
			{
				SingletonUpdater<AssetUpdateManager>.instance.Add(sound, 60f);
			}
			else
			{
				localItem.Sound = sound;
			}
		}
	}

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			if (localItem.Sound != null)
			{
				localItem.Sound.Update();
			}
			if (localItem.localTimer.time - localItem.BeginTime >= localItem.DurationTime)
			{
				ActionEnd(localItem, ERequestStop.None);
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
			ActionEnd(localItem, requestStop);
		}
	}

	private void OnMessageFast(CallbackMessage<NShared.BeginFireSkill> message)
	{
		if (!message.IsValidMessage())
		{
			return;
		}
		NShared.BeginFireSkill callback = message.Callback;
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(callback.CreatureUid);
		if (creature == null || SkillIndex != callback.SkillId)
		{
			return;
		}
		RootVariable rootVariable = SmallPooling<RootVariable>.CreateInstance();
		if (!rootVariable.Preopen(callback))
		{
			SmallPooling<RootVariable>.ReleaseInstance(rootVariable);
			return;
		}
		RuntimeInstance runtimeInstance = SmallPooling<RuntimeInstance>.CreateInstance();
		runtimeInstance.Open(this.GetNode().GetTree(), rootVariable, creature.ActionLocalTimer, null, Game3DRoot.DefaultLayer, this);
		if (IsActionSkill)
		{
			creature.NodeTreeController.SetMainRuntime(runtimeInstance);
		}
		else
		{
			SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(runtimeInstance, immediatelyStart: true);
		}
	}

	private void LocalItemOnMessageFast(LocalItem localItem, CallbackMessage<NShared.FireSkill> message)
	{
		if (localItem != null && message.IsValidMessage())
		{
			NShared.FireSkill callback = message.Callback;
			long value = localItem.Variable.GetValue<long>(VariableKeyword.Myself);
			if (value != 0L && (int)value == callback.CreatureUid && SkillIndex == callback.SkillId && RetargetOnFire)
			{
				localItem.Variable.AddValue(VariableKeyword.Target, (long)callback.FirstTargetCreatureUid);
			}
		}
	}

	private void LocalItemOnMessageFast(LocalItem localItem, CallbackMessage<EndFireSkill> message)
	{
		if (localItem == null || !message.IsValidMessage())
		{
			return;
		}
		EndFireSkill callback = message.Callback;
		long value = localItem.Variable.GetValue<long>(VariableKeyword.Myself);
		if (value != 0L && (int)value == callback.CreatureUid && SkillIndex == callback.SkillId && callback.IsCancel)
		{
			ActionEnd(localItem, ERequestStop.BattleCancel);
			if (SingletonAuto<BattleFacade>.instance.IsBattleEnabled())
			{
				localItem.Runtime.RequestStopFromInternal(ERequestStop.BattleCancel);
			}
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
		HeroInfo heroInfo = preloader.ExtraData as HeroInfo;
		string prefix = "BeginFireSkill";
		string postfix = string.Empty;
		if (heroInfo != null)
		{
			postfix = SoundHelper.GetCostumeVoicePostfix(heroInfo);
		}
		string fullSoundCode = SoundHelper.GetSoundCode(prefix, SkillIndex.ToString(), postfix);
		preloader.ExecuteAction(preloader.SoundPreloadType, delegate
		{
			preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound(fullSoundCode));
		});
	}

	public override void LoadFromJson(JsonData json)
	{
		Caller.LOADFROMJSON(json["Caller"]);
		SkillIndex = json["SkillIndex"].GETINT(SkillIndex);
		RetargetOnFire = json["RetargetOnFire"].GETBOOLEAN(RetargetOnFire);
		IsActionSkill = json["IsActionSkill"].GETBOOLEAN(IsActionSkill);
		PlaySkillSound = json["PlaySkillSound"].GETBOOLEAN(PlaySkillSound);
		StopSoundType = json["StopSoundType"].GETENUM(StopSoundType);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("Caller", Caller.MAKEJSON());
		jsonData.AddFieldLong("SkillIndex", SkillIndex);
		jsonData.AddFieldBool("RetargetOnFire", RetargetOnFire);
		jsonData.AddFieldBool("IsActionSkill", IsActionSkill);
		jsonData.AddFieldBool("PlaySkillSound", PlaySkillSound);
		jsonData.AddFieldEnum("StopSoundType", StopSoundType);
		return jsonData;
	}
}
