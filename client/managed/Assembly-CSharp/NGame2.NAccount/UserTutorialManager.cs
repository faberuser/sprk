using NShared;

namespace NGame2.NAccount;

public class UserTutorialManager : NShared.UserTutorialManager
{
	public void Apply(TutorialInfo resultInfo)
	{
		if (resultInfo != null)
		{
			Add(resultInfo);
		}
	}

	public void Apply(TutorialInfo[] resultInfos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(resultInfos); i++)
		{
			Apply(resultInfos[i]);
		}
	}
}
