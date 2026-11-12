using LitJson;
using NGame2.NMessage;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NShared;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class OnFlyingOperation : ExecuteBase, IKey
{
	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public FlyingOperation Callback;

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Execute/" + typeof(OnFlyingOperation).Name;

	public Callee CalleeReady;

	public Caller CallerReceive;

	public OnFlyingOperation(NodeData i_node)
		: base(i_node)
	{
		CalleeReady = new Callee(this, "CalleeReady", ActionReady);
		CallerReceive = new Caller(this, "CallerReceive");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private void ActionReady(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.AddOnMessageByOperationUid<LocalItem, OperationMessage<FlyingOperation>>(uid: localItem.Variable.GetValue<long>(VariableKeyword.OperationUid), verifier: localItem, onMessage: LocalItemOnMessageFast);
	}

	private void LocalItemOnMessageFast(LocalItem localItem, OperationMessage<FlyingOperation> message)
	{
		if (localItem != null && message.IsValidMessage() && localItem.Callback == null && localItem.Variable.GetValue<long>(VariableKeyword.OperationUid) == message.OperationUid)
		{
			localItem.Callback = message.Callback;
			LocalVariable variable = localItem.Variable;
			variable.AddValue(VariableKeyword.FlyingDurationTime, (float)localItem.Callback.FlyingDurationTimeMs * 0.001f);
			variable.AddValue(VariableKeyword.FlyingTarget, localItem.Callback.TargetCreatureUid);
			CallerReceive.Call(localItem.Runtime, localItem.Variable);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData i_json)
	{
		CalleeReady.LOADFROMJSON(i_json["CalleeStart"]);
		CallerReceive.LOADFROMJSON(i_json["CallerReceive"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeReady);
		jsonData.AddFieldJsonMake("CallerReceive", CallerReceive);
		return jsonData;
	}
}
