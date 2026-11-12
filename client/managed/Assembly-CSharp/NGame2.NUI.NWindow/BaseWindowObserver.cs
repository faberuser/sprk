namespace NGame2.NUI.NWindow;

public class BaseWindowObserver<T> : LobbyTopmostMenu.ILobbyTopmostObserver<T>, LobbyTopmostMenu.IPublicObserver
{
	public LobbyTopmostMenu.PublicObserverType Type { get; set; }

	public T BaseValue { get; set; }
}
