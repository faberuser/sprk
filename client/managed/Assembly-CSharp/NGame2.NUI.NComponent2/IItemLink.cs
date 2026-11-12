using NShared.NMessage.NProtocol;

namespace NGame2.NUI.NComponent2;

public interface IItemLink
{
	void OnPressLinkButton();

	void SetupEquipItemInfo(LinkedItemInfo equipItem);
}
