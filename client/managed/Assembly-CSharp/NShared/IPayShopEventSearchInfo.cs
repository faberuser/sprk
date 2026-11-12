namespace NShared;

public interface IPayShopEventSearchInfo
{
	PayShopCategory PayShopCategory { get; }

	int Index { get; }

	string Group { get; }
}
