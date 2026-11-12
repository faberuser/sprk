using LitJson;
using NGame2.NMessage;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class SendCreatureEvent : ExecuteBase, IKey
{
	public static string ContentsTypeName = "Execute/" + typeof(SendCreatureEvent).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<long> CreatureUid;

	public CreatureEventMessage.EEventType EventType = CreatureEventMessage.EEventType.BossNameTag;

	public string EventKeyword = string.Empty;

	public SendCreatureEvent(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		CreatureUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
	}

	public override void Wakeup(IVariable variable)
	{
		CreatureUid.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		int creatureUid = (int)CreatureUid.GetValue(variable);
		CreatureEventMessage creatureEventMessage = SmallPooling<CreatureEventMessage>.CreateInstance();
		creatureEventMessage.Open(creatureUid, EventType, null, EventKeyword, runtime.LocalTimer);
		SingletonAuto<GlobalManager>.instance.PostMessage(creatureEventMessage);
		return variable;
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		EventType = json["EventType"].GETENUM(EventType);
		EventKeyword = json["EventKeyword"].GETSTRING(EventKeyword);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("ChainCall", ChainCall);
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddFieldEnum("EventType", EventType);
		jsonData.AddFieldString("EventKeyword", EventKeyword);
		return jsonData;
	}
}
