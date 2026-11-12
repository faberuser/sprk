using NShared.NJit;

namespace NShared;

public class CreatureStarDataContainer : JitDataContainerDefault<CreatureStarDataKey, CreatureStarData>
{
	public const string TABLENAME = "CreatureStarTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureStarTable");

	public CreatureStarData GetData(int star, int transcended)
	{
		return GetData(new CreatureStarDataKey(star, transcended));
	}

	public CreatureStarData GetNextData(int star, int transcended)
	{
		if (star == GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar"))
		{
			return GetData(star, transcended + 1);
		}
		return GetData(star + 1, transcended);
	}

	public int GetHeroMaxLevel(int star, int transcended)
	{
		return GetData(star, transcended)?.MaxHeroLevel ?? 1;
	}
}
