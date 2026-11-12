using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NContents.NEffect;

public class ReleaseAssetEffect : ContentsBase, IKey
{
	public static string ContentsTypeName = "Effect/" + typeof(ReleaseAssetEffect).Name;

	public ChainCall ChainCall;

	public string Code = string.Empty;

	public ReleaseAssetEffect(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		ISceneContainer sceneContainer = runtime.SCENECONTAINER();
		if (sceneContainer == null)
		{
			return variable;
		}
		sceneContainer.ReleaseAssetEffect(Code);
		return variable;
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		Code = json["Code"].GETSTRING(Code);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddFieldString("Code", Code);
		return jsonData;
	}
}
