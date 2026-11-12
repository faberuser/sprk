using NGame2.NAccount;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;

namespace NGame2.NUtil.NStringProxy.NTypeText;

public class SkillBuilder : IBuilder
{
	private Helper.SkillBasicInfo _skillBaicInfo;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public SkillBuilder(Helper.SkillBasicInfo skillBaicInfo, bool appendLine)
	{
		_skillBaicInfo = skillBaicInfo;
		_appendLine = appendLine;
	}

	public string Build()
	{
		return GetText(_skillBaicInfo);
	}

	public static string GetCooltimeDisplayText(int cooltimeMs)
	{
		int num = cooltimeMs / 1000;
		int num2 = (cooltimeMs - num * 1000) / 100;
		string text = ((num2 == 0) ? num.ToString() : (num + "." + num2));
		return LocalizationManager.GetString(HeroSkill.SKILL_COOLTIME_DESC, text);
	}

	public static string GetText(Helper.SkillBasicInfo skillInfo)
	{
		if (skillInfo.TableData == null)
		{
			return "";
		}
		return LocalizationManager.GetString(skillInfo.TableData.TriggerType switch
		{
			SkillTriggerType.OnStart => HeroSkill.SKILL_TYPE_PASSIVE, 
			SkillTriggerType.OnAuto => HeroSkill.SKILL_TYPE_AUTO, 
			SkillTriggerType.None => HeroSkill.SKILL_TYPE_ACTIVE, 
			_ => Common.COMMON_ETC, 
		});
	}
}
