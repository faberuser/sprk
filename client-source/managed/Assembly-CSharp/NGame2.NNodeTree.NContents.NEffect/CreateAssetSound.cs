using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NScene;
using NGameAsset;
using NGameAsset.NSound;
using NShared;
using NVespa;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreateAssetSound : EffectBase, IKey, IExecute, IKeyFinalize
{
	public enum ELifetime
	{
		None,
		StopOnEnd
	}

	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;

		public ISound Sound;
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreateAssetSound).Name;

	public ChainCall ChainCall;

	public ELifetime Lifetime;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public string Code = string.Empty;

	public float PreventZombieTime = 10f;

	public CreateAssetSound(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
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
		int uid = (int)localItem.Variable.GetValue<long>(VariableKeyword.Myself);
		Creature bATTLECREATURE = runtime.SCENECONTAINER().GETBATTLECREATURE(uid);
		string code = Code;
		string postfix = string.Empty;
		if (bATTLECREATURE != null && bATTLECREATURE.CreateInfo != null)
		{
			postfix = SoundHelper.GetCostumeVoicePostfix(bATTLECREATURE.CreateInfo.HeroInfo);
		}
		string soundCode = SoundHelper.GetSoundCode(code, postfix);
		SoundStock stock = new SoundStock
		{
			Code = soundCode,
			LocalTimer = runtime.LocalTimer
		};
		if (Lifetime == ELifetime.None)
		{
			ISound sound = SingletonAuto<SoundContainer>.instance.CreateSound(stock);
			SingletonUpdater<AssetUpdateManager>.instance.Add(sound, PreventZombieTime);
			return variable;
		}
		if (Lifetime == ELifetime.StopOnEnd)
		{
			localItem.BeginTime = runtime.LocalTimer.time;
			localItem.Sound = SingletonAuto<SoundContainer>.instance.CreateSound(stock);
			runtime.ExecuteContainer.Pending(this, localItem);
			return localItem.Variable;
		}
		return variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			if (localItem.Sound != null)
			{
				SoundContainer.ReleaseSound(localItem.Sound);
				localItem.Sound = null;
			}
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			localItem.Sound.Update();
			if (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
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
		HeroInfo heroInfo = preloader.ExtraData as HeroInfo;
		string postfix = string.Empty;
		if (heroInfo != null)
		{
			postfix = SoundHelper.GetCostumeVoicePostfix(heroInfo);
		}
		string fullSoundCode = SoundHelper.GetSoundCode(Code, postfix);
		preloader.ExecuteAction(preloader.SoundPreloadType, delegate
		{
			preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound(fullSoundCode));
		});
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		Lifetime = json["Lifetime"].GETENUM(Lifetime);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		Code = json["Code"].GETSTRING(Code);
		PreventZombieTime = json["PreventZombieTime"].GETFLOAT(PreventZombieTime);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddFieldEnum("Lifetime", Lifetime);
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldDouble("PreventZombieTime", PreventZombieTime);
		return jsonData;
	}
}
