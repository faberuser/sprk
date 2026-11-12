using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class StoreKey : ExecuteBase, IKey, IExecute
{
	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public float BeginTime;

		public Creature cachedCreature;

		public string Key = string.Empty;

		public int Stack;

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Execute/" + typeof(StoreKey).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> KeyStoreUid;

	public string Key = string.Empty;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public StoreKey(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		KeyStoreUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		KeyStoreUid.GetValue(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		Dictionary<string, int> dictionary = null;
		if (KeyStoreUid.TryGetValue(variable, out var value))
		{
			Creature battleCreature = runtime.SceneContainer.GetBattleCreature((int)value);
			if (battleCreature != null)
			{
				localItem.cachedCreature = battleCreature;
				dictionary = battleCreature.KeyStackDic;
			}
		}
		if (dictionary != null)
		{
			if (!dictionary.ContainsKey(Key))
			{
				dictionary.Add(Key, 1);
			}
			else
			{
				dictionary[Key]++;
			}
		}
		localItem.BeginTime = runtime.LocalTimer.time;
		runtime.ExecuteContainer.Pending(this, localItem);
		CallerEnd.Call(runtime, localItem.Variable);
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (localItem.IsEnd)
		{
			return;
		}
		if (localItem.cachedCreature != null)
		{
			Dictionary<string, int> keyStackDic = localItem.cachedCreature.KeyStackDic;
			if (keyStackDic != null && keyStackDic.ContainsKey(Key))
			{
				keyStackDic[Key]--;
			}
		}
		localItem.ClearMessageHolder();
		localItem.IsEnd = true;
		CallerEnd.Call(localItem.Runtime, localItem.Variable);
	}

	public void UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			float num = localItem.localTimer.time - localItem.BeginTime;
			float value = DurationTime.GetValue(localItem.Variable, 1f);
			if (num > value || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
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

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		KeyStoreUid.LoadFromJson(json["KeyStoreUid"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		Key = json["Key"].GETSTRING(Key);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerEnd", CallerEnd);
		jsonData.AddField("KeyStoreUid", KeyStoreUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldString("Key", Key);
		return jsonData;
	}
}
