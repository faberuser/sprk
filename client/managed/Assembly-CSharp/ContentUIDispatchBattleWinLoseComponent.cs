using NGame2.NUI;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NPrefab2;

public class ContentUIDispatchBattleWinLoseComponent : LinkFactoryComponent
{
	public const string PATH_PREFAB = "Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchBattleWinLoseComponent";

	public UILabel LabelCount;

	public UILabel LabelWin;

	public UILabel LabelLose;

	public void Open(int count, bool isWin)
	{
		UIHelper.SetLabelText(LabelCount, LocalizationManager.GetString(DispatchBattle.DISPATCH_COUNT, count));
		UIHelper.EnableGameObject(LabelWin, isWin);
		UIHelper.EnableGameObject(LabelLose, !isWin);
	}
}
