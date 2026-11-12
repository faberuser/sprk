using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;

namespace NGame2.NNodeTree.NContents.NExecute;

public class UniqueExecute : ExecuteBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public long Uid;

		public StringKey KeywordKey;
	}

	public static string ContentsTypeName = "Execute/" + typeof(UniqueExecute).Name;

	public Callee CalleeStart;

	public Caller CallerNext;

	public ArgumentSingleGeneric<long> CreatureUid;

	public string Keyword = string.Empty;

	private StringKey _keywordKey;

	public UniqueExecute(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		CallerNext = new Caller(this, "CallerNext");
		CreatureUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
	}

	public override void Wakeup(IVariable variable)
	{
		CreatureUid.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		if (CanExecute(runtime, variable))
		{
			CallerNext.Call(runtime, variable);
		}
	}

	private bool CanExecute(RuntimeInstance runtime, IVariable variable)
	{
		if (!CreatureUid.TryGetValue(variable, out var value))
		{
			return false;
		}
		Creature battleCreature = runtime.SceneContainer.GetBattleCreature((int)value);
		if (battleCreature == null)
		{
			return false;
		}
		StringKey variableKeyword = GetVariableKeyword();
		IVariableValue<bool> variableValue = battleCreature.Variable.GetValueBase(variableKeyword).Casting<bool>();
		if (variableValue != null && variableValue.GetValue())
		{
			return false;
		}
		battleCreature.Variable.AddValue(variableKeyword, value: true, 999f);
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.Uid = value;
		localItem.KeywordKey = variableKeyword;
		runtime.ExecuteContainer.PendingWeak(this, localItem);
		return true;
	}

	private StringKey GetVariableKeyword()
	{
		if (_keywordKey == null)
		{
			if (!String_Extension_Method.IsNullOrEmpty(Keyword))
			{
				_keywordKey = StringKey.Get(Keyword);
			}
			else
			{
				_keywordKey = StringKey.Get(GetContentsPath());
			}
		}
		return _keywordKey;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			Creature battleCreature = localItem.GetBattleCreature((int)localItem.Uid);
			if (!(battleCreature == null))
			{
				battleCreature.Variable.RemoveValue(localItem.KeywordKey);
			}
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
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

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerNext.LOADFROMJSON(json["CallerNext"]);
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		Keyword = json["Keyword"].GETSTRING(Keyword);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerNext", CallerNext);
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddFieldString("Keyword", Keyword);
		return jsonData;
	}
}
