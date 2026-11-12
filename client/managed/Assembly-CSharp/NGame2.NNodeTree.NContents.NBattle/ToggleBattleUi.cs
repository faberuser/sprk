using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NUI.NManager.NBattle;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NBattle;

public class ToggleBattleUi : BattleBase, IKey
{
	public static string ContentsTypeName = "Battle/" + typeof(ToggleBattleUi).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public float TimeLength = 0.5f;

	public AnimationCurve AlphaCurve = new AnimationCurve();

	public bool ApplyBattleField;

	public bool ApplyUi;

	public bool ApplyUiOn3d;

	public TeamType ApplyTeamType = TeamType.All;

	public bool CloseSystemButton;

	public ToggleBattleUi(NodeData i_node)
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
			AlphaToggleEntry alphaToggleEntry = new AlphaToggleEntry();
			alphaToggleEntry.ApplyBattleField = ApplyBattleField;
			alphaToggleEntry.ApplyUi = ApplyUi;
			alphaToggleEntry.ApplyUiOn3d = ApplyUiOn3d;
			alphaToggleEntry.AlphaCurve = AlphaCurve;
			alphaToggleEntry.TimeLength = TimeLength;
			alphaToggleEntry.ApplyTeamType = ApplyTeamType;
			cAMERA.SetAlphaToggleEntry(alphaToggleEntry);
			if (CloseSystemButton)
			{
				SceneAutoBehaviour<BattleManagement>.instance.CloseMenu(EBattleMenu.SystemButton);
			}
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
		AlphaCurve = json["AlphaCurve"].GetAnimationCurve(AlphaCurve);
		ApplyBattleField = json["ApplyBattleField"].GETBOOLEAN(ApplyBattleField);
		ApplyUi = json["ApplyUi"].GETBOOLEAN(ApplyUi);
		ApplyUiOn3d = json["ApplyUiOn3d"].GETBOOLEAN(ApplyUiOn3d);
		ApplyTeamType = json["ApplyTeamType"].GETENUM(ApplyTeamType);
		CloseSystemButton = json["CloseSystemButton"].GETBOOLEAN(CloseSystemButton);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddFieldDouble("TimeLength", TimeLength);
		jsonData.AddFieldAnimationCurve("AlphaCurve", AlphaCurve);
		jsonData.AddFieldBool("ApplyBattleField", ApplyBattleField);
		jsonData.AddFieldBool("ApplyUi", ApplyUi);
		jsonData.AddFieldBool("ApplyUiOn3d", ApplyUiOn3d);
		jsonData.AddFieldEnum("ApplyTeamType", ApplyTeamType);
		jsonData.AddFieldBool("CloseSystemButton", CloseSystemButton);
		return jsonData;
	}
}
