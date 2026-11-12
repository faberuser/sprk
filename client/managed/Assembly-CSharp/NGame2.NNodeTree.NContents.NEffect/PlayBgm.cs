using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NScene;
using NGameAsset;
using NVespa;

namespace NGame2.NNodeTree.NContents.NEffect;

public class PlayBgm : ExecuteBase, IKey, IKeyFinalize
{
	public static string ContentsTypeName = "Effect/" + typeof(PlayBgm).Name;

	public ChainCall ChainCall;

	public string BgmCode;

	public bool IsEventBgm = true;

	public PlayBgm(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		if (!VolatileOptionData.Sound_EnableBgm)
		{
			return variable;
		}
		if (!SystemOptionData.Sound_EnableBgm)
		{
			return variable;
		}
		if (!String_Extension_Method.IsNullOrEmpty(BgmCode))
		{
			SingletonAuto<BgmManager2>.instance.Request(BgmCode, IsEventBgm);
			runtime.ItemContainer.CreateItem<ItemBase>(this, variable);
		}
		return variable;
	}

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
		if (!String_Extension_Method.IsNullOrEmpty(BgmCode) && IsEventBgm)
		{
			SingletonAuto<BgmManager2>.instance.StopEvent(BgmCode);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
		preloader.ExecuteAction(preloader.SoundPreloadType, delegate
		{
			preloader.AddAsyncLoad(SingletonAuto<BgmManager2>.instance.ReserveLock(BgmCode));
		});
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		BgmCode = json["BgmCode"].GETSTRING(BgmCode);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("ChainCall", ChainCall);
		jsonData.AddFieldString("BgmCode", BgmCode);
		return jsonData;
	}
}
