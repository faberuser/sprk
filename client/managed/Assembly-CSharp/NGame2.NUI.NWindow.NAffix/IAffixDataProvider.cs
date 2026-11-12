using NShared;

namespace NGame2.NUI.NWindow.NAffix;

public interface IAffixDataProvider
{
	EAffixCompState GetAffixCompState(AffixData affixData);
}
