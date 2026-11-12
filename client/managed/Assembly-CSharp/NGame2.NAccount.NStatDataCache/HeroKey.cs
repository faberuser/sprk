using System.Linq;
using NShared;

namespace NGame2.NAccount.NStatDataCache;

public struct HeroKey
{
	public int HeroIndex { get; private set; }

	public int Level { get; private set; }

	public int Star { get; private set; }

	public int Transcended { get; private set; }

	public string TranscendSkills { get; private set; }

	public HeroKey(HeroInfo info)
	{
		HeroIndex = info.HeroIndex;
		Level = info.Level;
		Star = info.Star;
		Transcended = info.Transcended;
		TranscendSkillPageInfo transcendSkillPageInfo = info.GetTranscendSkillPageInfo(info.SkillPage);
		TranscendSkills = string.Join(",", transcendSkillPageInfo.TranscendSkillDatas.Select((CreatureData.TranscendSkill data) => (data.Transcended * 1000 + data.Index * 100 + data.Level).ToString()).ToArray());
	}

	public static bool IsEquals(HeroKey x, HeroKey y)
	{
		if (x.HeroIndex != y.HeroIndex)
		{
			return false;
		}
		if (x.Level != y.Level)
		{
			return false;
		}
		if (x.Star != y.Star)
		{
			return false;
		}
		if (x.Transcended != y.Transcended)
		{
			return false;
		}
		if (x.TranscendSkills != y.TranscendSkills)
		{
			return false;
		}
		return true;
	}

	public static int GetHashCode(HeroKey obj)
	{
		return ((((31 * 17 + obj.HeroIndex) * 17 + obj.Level) * 17 + obj.Star) * 17 + obj.Transcended) * 17 + obj.TranscendSkills.GetHashCode();
	}
}
