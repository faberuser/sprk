using MessagePack;
using NShared.NJit;

namespace NShared;

public class ThreatDataContainer : JitDataContainerDefault<int, ThreatData>
{
	[MessagePackObject(false)]
	[JitKey]
	public struct ThreatKey
	{
		[Key(0)]
		public CreatureTagType TagType;

		[Key(1)]
		public ThreatType ThreatType;

		public ThreatKey(CreatureTagType tagType, ThreatType threatType)
		{
			TagType = tagType;
			ThreatType = threatType;
		}

		public bool Equals(ThreatKey other)
		{
			if (TagType == other.TagType)
			{
				return ThreatType == other.ThreatType;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return JitOption.BuildHash(TagType, ThreatType);
		}

		public override string ToString()
		{
			return $"{GetType().Name}({TagType}, {ThreatType})";
		}
	}

	public const string TABLENAME = "ThreatTable";

	private JitDictionary<ThreatKey, int> _cachedData = new JitDictionary<ThreatKey, int>();

	public override TablePathDefine PathDefine => new TablePathDefine("ThreatTable");

	public ThreatDataContainer()
	{
		base.Packer.RegisterAndBind(_cachedData);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (ThreatData item in base.BaseList.JsonLoad_GetOrNew())
		{
			_cachedData.JsonLoad_AddIfNotExist(new ThreatKey(item.ClassType, item.ThreatType), item.Multiplier);
		}
	}

	public int GetThreatMultiplier(CreatureTagType classType, ThreatType threatType)
	{
		return _cachedData.GetOrDefault(new ThreatKey(classType, threatType));
	}
}
