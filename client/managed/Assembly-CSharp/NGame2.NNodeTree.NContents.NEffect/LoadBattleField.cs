using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue.NModifier;
using NGame2.NScene;
using NGameAsset;
using NGameAsset.NBattleFieldMap2;
using NVespa;

namespace NGame2.NNodeTree.NContents.NEffect;

public class LoadBattleField : EffectBase, IKey
{
	public static string ContentsTypeName = "Effect/" + typeof(LoadBattleField).Name;

	public ChainCall ChainCall;

	public TransformGet TransformGet;

	public string Code = string.Empty;

	public string Prefix = string.Empty;

	public float BackgroundAngle;

	public LoadBattleField(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		TransformGet = new TransformGet();
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
		TransformGet.GetTransformValue(variable, out var oPosition, out var oRotation, out var oScale);
		sceneContainer.LoadBattleField(Code, Prefix, oPosition, oRotation, oScale, BackgroundAngle);
		return variable;
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get(Code);
		if (builder != null)
		{
			BattleFieldMapStock stock = new BattleFieldMapStock(Code, Prefix);
			OpenData openData = builder.CreateOpenData(stock);
			if (openData != null)
			{
				preloader.AddAsyncLoad(openData);
			}
		}
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		TransformGet.LOADFROMJSON(json["TransformGet"]);
		Code = json["Code"].GETSTRING(Code);
		Prefix = json["Prefix"].GETSTRING(Prefix);
		BackgroundAngle = json["BackgroundAngle"].GETFLOAT(BackgroundAngle);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("ChainCall", ChainCall);
		jsonData.AddFieldJsonMake("TransformGet", TransformGet);
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldString("Prefix", Prefix);
		jsonData.AddFieldDouble("BackgroundAngle", BackgroundAngle);
		return jsonData;
	}
}
