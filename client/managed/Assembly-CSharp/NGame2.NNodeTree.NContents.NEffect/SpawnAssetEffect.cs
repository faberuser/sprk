using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NTypeValue.NModifier;
using NGame2.NScene;
using NGameAsset;
using NGameAsset.NEffect;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class SpawnAssetEffect : EffectBase, IKey
{
	public static string ContentsTypeName = "Effect/" + typeof(SpawnAssetEffect).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<Transform> ParentTransform;

	public TransformGet TransformGet;

	public string Code = string.Empty;

	public SpawnAssetEffect(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		ParentTransform = new ArgumentSingleGeneric<Transform>(StringKey.Empty);
		TransformGet = new TransformGet();
	}

	public override void Wakeup(IVariable variable)
	{
		ParentTransform.GetValue(variable);
		TransformGet.Wakeup(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		ISceneContainer sceneContainer = runtime.SCENECONTAINER();
		if (sceneContainer == null)
		{
			return variable;
		}
		ITransformData transformData = TransformGet.GetTransformData(variable);
		IAsyncLoad<IEffect> asyncLoad = SingletonAuto<EffectContainer>.instance.CreateAsync(Code, runtime.Layer, transformData);
		asyncLoad.Value.SetTimer(runtime.LocalTimer);
		asyncLoad.Value.SetParent(ParentTransform.GetValue(variable));
		sceneContainer.SpawnAssetEffect(asyncLoad);
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
			preloader.ExecuteAction(preloader.EffectPreloadType, delegate
			{
				preloader.AddAsyncLoad(builder.CreateOpenData());
			});
		}
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		ParentTransform.LoadFromJson(json["ParentTransform"]);
		TransformGet.LOADFROMJSON(json["TransformGet"]);
		Code = json["Code"].GETSTRING(Code);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("ParentTransform", ParentTransform.MakeJson());
		jsonData.AddFieldJsonMake("TransformGet", TransformGet);
		jsonData.AddFieldString("Code", Code);
		return jsonData;
	}
}
