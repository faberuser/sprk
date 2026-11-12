using NGame2.NAccount;

namespace NGame2.NUI.NWindow;

public class BaseWindowUserStatObserver : LobbyTopmostMenu.IUserStatObserver
{
	public UserManager.UserStatType StatType { get; set; }
}
