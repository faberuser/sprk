using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CampaignNodeData : IDataKeySelector<ChapterDungeonKey2>
{
	public enum EMenuOpenCondition
	{
		None,
		ClearDungeon,
		NotReady
	}

	public enum ENodeOpenStyle
	{
		Multi,
		Single
	}

	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(ChapterIndex, DungeonIndex);

	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int DungeonIndex { get; set; }

	[Key(3)]
	public NodeFunctionType Function { get; set; }

	[Key(4)]
	public string[] SwitchCondition { get; set; }

	[Key(5)]
	public HashString NodePrefab { get; set; }

	[Key(6)]
	public HashString FunctionButton { get; set; }

	[Key(7)]
	public string[] Code { get; set; }

	[Key(8)]
	public string[] Value { get; set; }

	[Key(9)]
	public int UiPositionX { get; set; }

	[Key(10)]
	public int UiPositionY { get; set; }

	[Key(11)]
	public int[] NextNodeIndex { get; set; }

	[Key(12)]
	public bool Oneway { get; set; }

	[Key(13)]
	public bool Opened { get; set; }

	[Key(14)]
	public bool Hidden { get; set; }

	[Key(15)]
	public int[] ResetNodeIndex { get; set; }

	[Key(16)]
	public int PortalPositionX { get; set; }

	[Key(17)]
	public int PortalPositionY { get; set; }

	[Key(18)]
	public string PortalNameCode { get; set; }

	[Key(19)]
	public string PortalImageCode { get; set; }

	[Key(20)]
	public int RecommendHeroLevel { get; set; }

	[Key(21)]
	public int VisitRewardIndex { get; set; }

	[Key(22)]
	public int ReqDungeonIndex { get; set; }

	[Key(23)]
	public int ReqTutorialIndex { get; set; }

	[Key(24)]
	public string[] ReqCondition { get; set; }

	[Key(25)]
	public int[] AddDirectionNode { get; set; }

	[Key(26)]
	public ENodeOpenStyle NodeOpenStyle { get; set; }

	[Key(27)]
	public string[] NodeOpenValues { get; set; }

	[Key(28)]
	public bool OpenBackgroundDownloading { get; set; }

	[Key(29)]
	public string EntryNPCCode { get; set; }

	[Key(30)]
	public EMenuOpenCondition MenuOpenCondition { get; set; }

	[Key(31)]
	public string[] MenuOpenConditionArgs { get; set; }

	[Key(32)]
	public StaminaType EntryMenuStamina { get; set; }

	[Key(33)]
	public int PortalListScore { get; set; }

	[Key(34)]
	public ShopCostType EntryMenuAsset { get; set; }

	[Key(35)]
	public ContentsStatusType ContentsType { get; set; }

	[Key(36)]
	public bool ControlNPC { get; set; }

	[Key(37)]
	public GuildConstructionResourceTopmostMenuType GuildResourceMenuType { get; set; }

	[IgnoreMember]
	public string Name => GameRoot.LocalizeProxy.GetCache_or_Build(this, "Name", BuildLocalize_Name);

	public string GetValue(int index)
	{
		return ArgParser.GetValueString(Value, index);
	}

	public int GetValueInt(int index)
	{
		return ArgParser.GetValueInt(Value, index);
	}

	public bool GetValueBool(int index)
	{
		return ArgParser.GetValueBool(Value, index);
	}

	public T GetValueEnum<T>(int index) where T : struct
	{
		return ArgParser.GetValueEnum<string, T>(Value, index);
	}

	public string GetSwitchCondtionValue(int index)
	{
		return ArgParser.GetValueString(SwitchCondition, index);
	}

	public int GetSwitchCondtionValueInt(int index)
	{
		return ArgParser.GetValueInt(SwitchCondition, index);
	}

	public bool GetSwitchCondtionValueBool(int index)
	{
		return ArgParser.GetValueBool(SwitchCondition, index);
	}

	public T GetSwitchCondtionValueEnum<T>(int index, T defaultValue) where T : struct
	{
		return ArgParser.GetValueEnum<string, T>((IList<string>)SwitchCondition, index, defaultValue);
	}

	public bool CheckClearChapter()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(ChapterIndex, DungeonIndex);
		if (data != null && data.CheckClearChapter != 0)
		{
			return true;
		}
		return false;
	}

	private string BuildLocalize_Name()
	{
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(ChapterIndex);
		string text = ((data != null) ? data.GetChapterName() : string.Empty);
		if (Code == null || Code.Length == 0)
		{
			return $"{text} {ChapterIndex} - {DungeonIndex}";
		}
		string valueString = ArgParser.GetValueString(Code, 0);
		bool valueBool = ArgParser.GetValueBool(Code, 1);
		return string.Concat(str1: GameRoot.LocalizeProxy.GetString(valueString), str0: valueBool ? text : "");
	}
}
