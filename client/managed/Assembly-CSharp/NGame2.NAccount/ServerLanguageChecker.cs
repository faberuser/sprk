using System.Text.RegularExpressions;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class ServerLanguageChecker : SingletonAuto<ServerLanguageChecker>
{
	private static Regex _regexForJPN;

	public static bool CheckValidCharacter(string value)
	{
		string serverName = (GlobalAutoBehaviour<AccountManager>.isValidInstance ? GlobalAutoBehaviour<AccountManager>.instance.CountryServerName : string.Empty);
		string data = GameRoot.Instance.LanguagePatternDataContainer.GetData(serverName);
		if (String_Extension_Method.IsNullOrEmpty(data))
		{
			return true;
		}
		if (_regexForJPN == null)
		{
			_regexForJPN = new Regex(data);
		}
		for (int i = 0; i < value.Length; i++)
		{
			string input = value[i].ToString();
			if (!_regexForJPN.IsMatch(input))
			{
				return false;
			}
		}
		return true;
	}
}
