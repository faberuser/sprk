using NShared;

namespace NGame2.NUI.NManager;

internal class EntryMenuListData
{
	public string MenuType;

	private EntryMenuSettingData data => GameRoot.Instance.EntryMenuSettingDataContainer.GetDataByMenuType(MenuType);

	public EntryMenuListData(string menuType)
	{
		MenuType = menuType;
	}
}
