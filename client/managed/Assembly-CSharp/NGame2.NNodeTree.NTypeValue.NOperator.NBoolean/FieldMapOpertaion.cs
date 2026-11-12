using NGame2.NBattleView;
using NGame2.NNodeTree.NRuntime;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NBoolean;

public static class FieldMapOpertaion
{
	public static bool ExistFieldMapRenderFacacde(IVariable variable, long uid)
	{
		if (!BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
		{
			return false;
		}
		if (BattleViewGenericSingleton<FieldMapManager2>.instance.GetRenderFacade(uid) == null)
		{
			return false;
		}
		return true;
	}
}
