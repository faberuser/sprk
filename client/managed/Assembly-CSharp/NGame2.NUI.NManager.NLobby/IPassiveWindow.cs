using NGame2.NUI.NWindow;

namespace NGame2.NUI.NManager.NLobby;

internal interface IPassiveWindow
{
	EPassiveWindowOpenFlag OpenFlag { get; }

	BaseWindow GetWindow(bool openedOnly);

	void Open();

	void Close();
}
