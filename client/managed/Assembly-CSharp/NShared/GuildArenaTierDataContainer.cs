using NShared.NJit;

namespace NShared;

public class GuildArenaTierDataContainer : JitDataContainerDefault<int, GuildArenaTierData>
{
	public const string TABLENAME = "GuildArenaTierTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildArenaTierTable");

	public GuildArenaTierData GetHighestTierData()
	{
		GuildArenaTierData guildArenaTierData = null;
		foreach (GuildArenaTierData allDatum in GetAllData())
		{
			if (guildArenaTierData == null)
			{
				guildArenaTierData = allDatum;
			}
			else if (allDatum.Tier > guildArenaTierData.Tier)
			{
				guildArenaTierData = allDatum;
			}
		}
		return guildArenaTierData;
	}

	public GuildArenaTierData GetData(GuildArenaTierType tierType)
	{
		foreach (GuildArenaTierData allDatum in GetAllData())
		{
			if (tierType == allDatum.Tier)
			{
				return allDatum;
			}
		}
		return null;
	}
}
