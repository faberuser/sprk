using LitJson;
using NGame2.NMessage;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class DisplayBuffIcon : ExecuteBase, IKey, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public long CreatureUid;

		public string IconCode;

		public string CustomCode;
	}

	public static string ContentsTypeName = "Execute/" + typeof(DisplayBuffIcon).Name;

	public Callee CalleeStart;

	public Caller CallerNext;

	public ArgumentSingleGeneric<long> CreatureUid;

	public string IconCode = string.Empty;

	public string CustomCode = string.Empty;

	public DisplayBuffIcon(NodeData i_node)
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
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.CreatureUid = CreatureUid.GetValue(variable);
		localItem.IconCode = IconCode;
		localItem.CustomCode = CustomCode;
		BuffIconMessage buffIconMessage = SmallPooling<BuffIconMessage>.CreateInstance();
		buffIconMessage.Open(BuffIconMessage.EEventType.Show, localItem.CreatureUid, localItem.IconCode, localItem.CustomCode);
		SingletonAuto<GlobalManager>.instance.PostMessage(buffIconMessage);
		CallerNext.Call(runtime, variable);
	}

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
		if (itemBase is LocalItem localItem)
		{
			BuffIconMessage buffIconMessage = SmallPooling<BuffIconMessage>.CreateInstance();
			buffIconMessage.Open(BuffIconMessage.EEventType.Hide, localItem.CreatureUid, localItem.IconCode, localItem.CustomCode);
			SingletonAuto<GlobalManager>.instance.PostMessage(buffIconMessage);
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
		IconCode = json["IconCode"].GETSTRING(IconCode);
		CustomCode = json["CustomCode"].GETSTRING(CustomCode);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerNext", CallerNext);
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddFieldString("IconCode", IconCode);
		jsonData.AddFieldString("CustomCode", CustomCode);
		return jsonData;
	}
}
