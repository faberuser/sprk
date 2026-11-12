using LitJson;
using NGame2.NMessage;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NShared;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class OnEndFireSkill : ExecuteBase, IKey
{
	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public EndFireSkill Callback;

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Execute/" + typeof(OnEndFireSkill).Name;

	public Callee CalleeReady;

	public Caller CallerNotCancel;

	public Caller CallerCancel;

	public ArgumentSingleGeneric<long> SkillIndex;

	public OnEndFireSkill(NodeData i_node)
		: base(i_node)
	{
		CalleeReady = new Callee(this, "CalleeReady", ActionReady);
		CallerNotCancel = new Caller(this, "CallerReceiveNotCancel");
		CallerCancel = new Caller(this, "CallerReceiveCancel");
		SkillIndex = new ArgumentSingleGeneric<long>(StringKey.Empty);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private void ActionReady(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.AddOnMessageByCreatureUid<LocalItem, CallbackMessage<EndFireSkill>>(uid: (int)localItem.Variable.GetValue<long>(VariableKeyword.Myself), verifier: localItem, onMessage: LocalItemOnMessageFast);
	}

	private void LocalItemOnMessageFast(LocalItem localItem, CallbackMessage<EndFireSkill> message)
	{
		if (localItem == null || !message.IsValidMessage() || localItem.Callback != null)
		{
			return;
		}
		EndFireSkill callback = message.Callback;
		long value = localItem.Variable.GetValue<long>(VariableKeyword.Myself);
		if (value != 0L && (int)value == callback.CreatureUid && SkillIndex.GetValue(localItem.Variable) == callback.SkillId)
		{
			if (!callback.IsCancel)
			{
				CallerNotCancel.Call(localItem.Runtime, localItem.Variable);
			}
			else
			{
				CallerCancel.Call(localItem.Runtime, localItem.Variable);
			}
			localItem.IsEnd = true;
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeReady.LOADFROMJSON(json["CalleeStart"]);
		CallerNotCancel.LOADFROMJSON(json["CallerNotCancel"]);
		CallerCancel.LOADFROMJSON(json["CallerCancel"]);
		SkillIndex.LoadFromJson(json["SkillIndex"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeReady);
		jsonData.AddFieldJsonMake("CallerNotCancel", CallerNotCancel);
		jsonData.AddFieldJsonMake("CallerCancel", CallerCancel);
		jsonData.AddField("SkillIndex", SkillIndex.MakeJson());
		return jsonData;
	}
}
