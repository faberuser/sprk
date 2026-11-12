using NShared.NContentsDefine;

public static class __Extension_Method_ContentsDefine_ReturnUserJumping_MissionCategoryKey__
{
	public static bool IsEmpty(this ReturnUserJumping.MissionCategoryKey This)
	{
		if (This == null)
		{
			return true;
		}
		if (This.Main == 0)
		{
			return This.Sub == 0;
		}
		return false;
	}
}
