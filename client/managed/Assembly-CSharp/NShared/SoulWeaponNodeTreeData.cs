using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulWeaponNodeTreeData : IDataKeySelector<string>
{
	string IDataKeySelector<string>.Key => ItemCode;

	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public int SoulStoneItemIndex { get; set; }

	[Key(2)]
	public int WeaponCreatureIndex { get; set; }

	[Key(3)]
	public int SoulStoneCreatureIndex { get; set; }

	[Key(4)]
	public float[] ManageWeaponPosition { get; set; }

	[Key(5)]
	public float[] ManageWeaponRotation { get; set; }

	[Key(6)]
	public float ManageWeaponScale { get; set; }

	[Key(7)]
	public float[] UpgradeWeaponPosition { get; set; }

	[Key(8)]
	public float[] UpgradeWeaponRotation { get; set; }

	[Key(9)]
	public float UpgradeWeaponScale { get; set; }

	[Key(10)]
	public float[] UpgradeResultWeaponPosition { get; set; }

	[Key(11)]
	public float[] UpgradeResultWeaponRotation { get; set; }

	[Key(12)]
	public float UpgradeResultWeaponScale { get; set; }

	[Key(13)]
	public string[] LiberationBackGround { get; set; }

	[Key(14)]
	public string[] LiberationWait { get; set; }

	[Key(15)]
	public string[] LiberationAction { get; set; }

	[Key(16)]
	public string[] Liberation { get; set; }

	[Key(17)]
	public ESoulweaponLiberationResultSide LiberationResultSide { get; set; }

	[Key(18)]
	public string[] ModelView { get; set; }

	[Key(19)]
	public string[] ReinforceSuccess { get; set; }

	[Key(20)]
	public string[] ReinforceFail { get; set; }

	[Key(21)]
	public string[] Restore { get; set; }

	public string GetPackCode(SoulWeaponNodeTreeType type)
	{
		return GetNodeTreeData(type, SoulWeaponNodeTreeDataType.PackCode);
	}

	public string GetTreeName(SoulWeaponNodeTreeType type)
	{
		return GetNodeTreeData(type, SoulWeaponNodeTreeDataType.TreeName);
	}

	private string GetNodeTreeData(SoulWeaponNodeTreeType type, SoulWeaponNodeTreeDataType dataType)
	{
		switch (type)
		{
		case SoulWeaponNodeTreeType.Liberation:
		case SoulWeaponNodeTreeType.Upgrade:
			return ArgParser.GetValueString(Liberation, (int)dataType);
		case SoulWeaponNodeTreeType.LiberationBackground:
			return ArgParser.GetValueString(LiberationBackGround, (int)dataType);
		case SoulWeaponNodeTreeType.LiberationWait:
			return ArgParser.GetValueString(LiberationWait, (int)dataType);
		case SoulWeaponNodeTreeType.LiberationAction:
			return ArgParser.GetValueString(LiberationAction, (int)dataType);
		case SoulWeaponNodeTreeType.ModelView:
			return ArgParser.GetValueString(ModelView, (int)dataType);
		case SoulWeaponNodeTreeType.ReinforceSuccess:
			return ArgParser.GetValueString(ReinforceSuccess, (int)dataType);
		case SoulWeaponNodeTreeType.ReinforceFail:
			return ArgParser.GetValueString(ReinforceFail, (int)dataType);
		case SoulWeaponNodeTreeType.Restore:
			return ArgParser.GetValueString(Restore, (int)dataType);
		default:
			return string.Empty;
		}
	}

	public void GetModleViewValues(ESoulWeaponModelViewType modelType, out float[] position, out float[] rotation, out float scale)
	{
		position = new float[3];
		rotation = new float[3];
		scale = 1f;
		switch (modelType)
		{
		case ESoulWeaponModelViewType.Manage:
			position = ManageWeaponPosition;
			rotation = ManageWeaponRotation;
			scale = ManageWeaponScale;
			break;
		case ESoulWeaponModelViewType.Upgrade:
			position = UpgradeWeaponPosition;
			rotation = UpgradeWeaponRotation;
			scale = UpgradeWeaponScale;
			break;
		case ESoulWeaponModelViewType.UpgradeResult:
			position = UpgradeResultWeaponPosition;
			rotation = UpgradeResultWeaponRotation;
			scale = UpgradeResultWeaponScale;
			break;
		}
	}
}
