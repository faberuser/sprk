using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NManager.ClassBuff;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NContents;

public class ClassBuffOpenCheck : IMenuOpenChecker
{
	bool IMenuOpenChecker.IsOpen()
	{
		if (!AccountManager.IsLogined())
		{
			return false;
		}
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		if (classBuffManager == null)
		{
			return false;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(classBuffManager.ClassBuffInfoDic))
		{
			return true;
		}
		Dictionary<CreatureTagType, long> classBuffPointDic = classBuffManager.ClassBuffPointDic;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(classBuffPointDic))
		{
			return false;
		}
		ClassBuffWrapper classBuffWrapper = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper;
		if (classBuffWrapper == null)
		{
			return false;
		}
		long openNeedClassBuffPoint = classBuffWrapper.GetOpenNeedClassBuffPoint();
		foreach (long value in classBuffPointDic.Values)
		{
			if (value >= openNeedClassBuffPoint)
			{
				return true;
			}
		}
		return false;
	}
}
