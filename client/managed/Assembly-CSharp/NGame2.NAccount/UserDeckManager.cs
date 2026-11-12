using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class UserDeckManager : NShared.UserDeckManager
{
	public void Apply(DeckInfo info)
	{
		if (info != null)
		{
			Add(info);
		}
	}

	public void Apply(IList<DeckInfo> info)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(info); i++)
		{
			Apply(info[i]);
		}
	}
}
