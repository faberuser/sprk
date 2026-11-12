using NGame2.NUI.NDescription;
using NShared;
using NVespa;

namespace NGame2.NUtil.NStringProxy;

public class SkillDescBuilder : IBuilder
{
	private int _skillIndex;

	private HeroInfo _heroInfo;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public SkillDescBuilder(int skillIndex, HeroInfo heroInfo, bool appendLine)
	{
		_skillIndex = skillIndex;
		_heroInfo = heroInfo;
		_appendLine = appendLine;
	}

	public string Build()
	{
		return SingletonAuto<SkillDescContainer>.instance.GetByHeroInfo(_skillIndex, _heroInfo);
	}
}
