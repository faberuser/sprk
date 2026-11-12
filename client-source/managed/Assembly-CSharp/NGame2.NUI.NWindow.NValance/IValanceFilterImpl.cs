using NShared;

namespace NGame2.NUI.NWindow.NValance;

public interface IValanceFilterImpl
{
	bool IsNoneSelected();

	void Clear();

	EValanceFilterSelected Select(IValanceFilterInfo info);
}
