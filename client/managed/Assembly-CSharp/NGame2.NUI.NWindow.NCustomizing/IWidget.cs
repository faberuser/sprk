using NGame2.NAccount;
using NGame2.NPayShopItemEvent;

namespace NGame2.NUI.NWindow.NCustomizing;

public interface IWidget
{
	IWidget SetRenderFacadeController(IRenderFacadeController renderFacadeController);

	void Open(ToggleType toggleType);

	void Reset();

	void OnChanged(UserHeroManager.ChangeHeroInfo changeHeroInfo);

	void OnChanged(PayShopItemEventManager.EventChangedInfo info);

	void OnButtonEvent(ButtonClickEvent buttonEvent);
}
