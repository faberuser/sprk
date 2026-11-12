using NShared;

namespace NGame2.NAccount;

public class UserPlayRecordManager : NShared.UserPlayRecordManager
{
	public void Apply(PlayRecordInfo resultInfo)
	{
		if (resultInfo != null)
		{
			Add(resultInfo);
		}
	}

	public void Apply(PlayRecordInfo[] resultInfos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(resultInfos); i++)
		{
			Apply(resultInfos[i]);
		}
	}
}
