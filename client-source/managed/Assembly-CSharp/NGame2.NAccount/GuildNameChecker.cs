using NShared;
using NShared.CheckGuildName;
using NVespa;

namespace NGame2.NAccount;

public class GuildNameChecker : SingletonAuto<GuildNameChecker>
{
	public ResultType Check(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return ResultType.EmptyName;
		}
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MinGuildNameLength");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxGuildNameLength");
		if (name.Length < valueInt)
		{
			return ResultType.TooShortName;
		}
		if (name.Length > valueInt2)
		{
			return ResultType.TooLongName;
		}
		if (!ServerLanguageChecker.CheckValidCharacter(name))
		{
			return ResultType.InvalidGuildNameInThisServer;
		}
		switch (GameRoot.Instance.CensoredWordDataContainer.IsCensoredWord(name))
		{
		case CensoredWordDataContainer.CensoredResult.Censored:
			return ResultType.CensoredWord;
		case CensoredWordDataContainer.CensoredResult.Sensitive:
			return ResultType.SensitiveWord;
		default:
			if (!NickChecker.CheckValidCharacter(name))
			{
				return ResultType.InvalidName;
			}
			return ResultType.Success;
		}
	}
}
