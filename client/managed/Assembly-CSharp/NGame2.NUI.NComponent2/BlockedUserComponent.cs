using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class BlockedUserComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClickUnblock(BlockedUserComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/GameOption/BlockedUserComponent";

	public static PrefabAuto<BlockedUserComponent> PrefabAuto = new PrefabAuto<BlockedUserComponent>("Prefab2/Component/GameOption/BlockedUserComponent");

	public UILabel NameLabel;

	private Listener _listener;

	public long AccountId { get; set; }

	public string UserName { get; set; }

	public void Open(Listener listener, long accountId, string userName)
	{
		_listener = listener;
		AccountId = accountId;
		UserName = userName;
		UIHelper.SetLabelText(NameLabel, userName);
	}

	public void OnClickUnblock()
	{
		if (_listener != null)
		{
			_listener.OnClickUnblock(this);
		}
	}
}
