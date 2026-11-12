using MessagePack;
using NShared.NJit;

namespace NShared;

public class PunishmentRuneDataContainer : JitDataContainerDefault<int, PunishmentRuneData>
{
	[MessagePackObject(false)]
	[JitKey]
	public struct PunishmentRuneKey
	{
		[Key(0)]
		public int VoidRuneIndex;

		[Key(1)]
		public int LegendRuneIndex;

		public PunishmentRuneKey(int voidRuneIndex, int legendRuneIndex)
		{
			VoidRuneIndex = voidRuneIndex;
			LegendRuneIndex = legendRuneIndex;
		}

		public bool Equals(PunishmentRuneKey other)
		{
			if (VoidRuneIndex == other.VoidRuneIndex)
			{
				return LegendRuneIndex == other.LegendRuneIndex;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JitOption.BuildHash(VoidRuneIndex, LegendRuneIndex);
		}

		public override string ToString()
		{
			return $"{GetType().Name}({VoidRuneIndex}, {LegendRuneIndex})";
		}
	}

	public const string TABLENAME = "PunishmentRuneTable";

	private JitDictionary<PunishmentRuneKey, PunishmentRuneData> _dicData = new JitDictionary<PunishmentRuneKey, PunishmentRuneData>();

	public override TablePathDefine PathDefine => new TablePathDefine("PunishmentRuneTable");

	public PunishmentRuneDataContainer()
	{
		base.Packer.RegisterAndBind(_dicData);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (PunishmentRuneData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_dicData.JsonLoad_AddIfNotExist(new PunishmentRuneKey(item.VoidRuneIndex, item.LegendRuneIndex), item);
		}
	}

	public PunishmentRuneData GetData(int voidRuneIndex, int legendRuneIndex)
	{
		return _dicData.GetOrDefault(new PunishmentRuneKey(voidRuneIndex, legendRuneIndex));
	}
}
