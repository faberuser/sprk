using LitJson;
using NGame2.NMessage;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class SendCommandMsg : ExecuteBase, IKey
{
	public static string ContentsTypeName = "Execute/" + typeof(SendCommandMsg).Name;

	public static readonly string[] CommandList = new string[6] { "Common/PlayBgm", "GetNewHero/PlayStarEffect", "GetNewHero/StopStarEffect", "EquipAwakenScene/ShowItemIcon", "EquipGachaScene/ShowItemIcon", "NewRaidCinema/ShowNewRaidUi" };

	public ChainCall ChainCall;

	public bool IsGlobalMessage;

	public string Command = string.Empty;

	public ArgumentList ArgsList = new ArgumentList();

	public SendCommandMsg(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
	}

	public override void Wakeup(IVariable variable)
	{
		ArgsList.Wakeup(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		if (String_Extension_Method.IsNullOrEmpty(Command))
		{
			return variable;
		}
		CommandMessage commandMessage = SmallPooling<CommandMessage>.CreateInstance();
		commandMessage.Open(Command);
		for (int i = 0; i < ArgsList.Count; i++)
		{
			commandMessage.AddArg(ArgsList[i].GetObjectValue(variable));
		}
		if (!IsGlobalMessage)
		{
			commandMessage.TargetScene = runtime.SCENECONTAINER();
		}
		SingletonAuto<GlobalManager>.instance.PostMessage(commandMessage);
		return variable;
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		IsGlobalMessage = json["IsGlobalMessage"].GETBOOLEAN(IsGlobalMessage);
		Command = json["Command"].GETSTRING(Command);
		ArgsList.LOADFROMJSON(json["ArgsList"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("ChainCall", ChainCall);
		jsonData.AddFieldBool("IsGlobalMessage", IsGlobalMessage);
		jsonData.AddFieldString("Command", Command);
		jsonData.AddFieldJsonMake("ArgsList", ArgsList);
		return jsonData;
	}
}
