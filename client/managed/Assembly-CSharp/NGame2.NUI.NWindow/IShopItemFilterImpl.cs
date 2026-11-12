using NShared;

namespace NGame2.NUI.NWindow;

public interface IShopItemFilterImpl
{
	bool IsNoneSelectedSort();

	void Clear();

	EShopItemFilterSelected Select(IShopItemFilterInfo info);
}
