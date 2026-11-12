using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NSceneContainer;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NBattle;

public class ToggleBattleUi2 : BattleBase, IKey
{
	public static string ContentsTypeName = "Battle/" + typeof(ToggleBattleUi2).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public float TimeLength = 0.5f;

	public AnimationCurve BlendCurve = new AnimationCurve();

	public EBoolCheck ShowBattleField;

	public EBoolCheck ShowUi;

	public EBoolCheck ShowUiOn3d;

	public ToggleBattleUi2(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		CreateBattleUiEntry(runtime, variable);
		CallerEnd.Call(runtime, variable);
	}

	private void CreateBattleUiEntry(RuntimeInstance runtime, IVariable variable)
	{
		ICamera cAMERA = runtime.SCENECONTAINER().GETCAMERA();
		if (cAMERA != null)
		{
			AlphaToggleEntry2 alphaToggleEntry = new AlphaToggleEntry2();
			alphaToggleEntry.ShowBattleField = ShowBattleField;
			alphaToggleEntry.ShowUi = ShowUi;
			alphaToggleEntry.ShowUiOn3d = ShowUiOn3d;
			alphaToggleEntry.BlendCurve = BlendCurve;
			alphaToggleEntry.TimeLength = TimeLength;
			cAMERA.SetAlphaToggleEntry2(alphaToggleEntry);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		TimeLength = json["TimeLength"].GETFLOAT(TimeLength);
		BlendCurve = json["AlphaCurve"].GetAnimationCurve(BlendCurve);
		ShowBattleField = json["ShowBattleField"].GETENUM(ShowBattleField);
		ShowUi = json["ShowUi"].GETENUM(ShowUi);
		ShowUiOn3d = json["ShowUiOn3d"].GETENUM(ShowUiOn3d);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddFieldDouble("TimeLength", TimeLength);
		jsonData.AddFieldAnimationCurve("AlphaCurve", BlendCurve);
		jsonData.AddFieldEnum("ShowBattleField", ShowBattleField);
		jsonData.AddFieldEnum("ShowUi", ShowUi);
		jsonData.AddFieldEnum("ShowUiOn3d", ShowUiOn3d);
		return jsonData;
	}
}
