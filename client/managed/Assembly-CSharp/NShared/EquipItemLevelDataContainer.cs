using MessagePack;
using NShared.NJit;

namespace NShared;

public class EquipItemLevelDataContainer : JitDataContainerDefault<int, EquipItemLevelData>
{
	[MessagePackObject(false)]
	public class ContainerInfo
	{
		[Key(0)]
		public int MaxLevel { get; set; }
	}

	public const string TABLENAME = "EquipItemLevelTable";

	private JitRef<ContainerInfo> _containerInfo = new JitRef<ContainerInfo>();

	private JitDictionary<int, int> _optionOpenLevel = new JitDictionary<int, int>();

	public override TablePathDefine PathDefine => new TablePathDefine("EquipItemLevelTable");

	public EquipItemLevelDataContainer()
	{
		base.Packer.RegisterAndBind(_containerInfo);
		base.Packer.RegisterAndBind(_optionOpenLevel);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (EquipItemLevelData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (_containerInfo.JsonLoad_GetOrNew().MaxLevel == 0 || item.Level > _containerInfo.JsonLoad_GetOrNew().MaxLevel)
			{
				_containerInfo.JsonLoad_GetOrNew().MaxLevel = item.Level;
			}
			if (item.OptionOpen > 0)
			{
				_optionOpenLevel.JsonLoad_AddIfNotExist(item.OptionOpen, item.Level);
			}
		}
	}

	public int GetEquipItemOptionOpen(int level)
	{
		if (level > _containerInfo.Eval().MaxLevel)
		{
			level = _containerInfo.Eval().MaxLevel;
		}
		return GetData(level)?.OptionOpen ?? 0;
	}

	public int GetEquipItemOptionLevel(int optionCount)
	{
		return _optionOpenLevel.GetOrDefault(optionCount);
	}
}
