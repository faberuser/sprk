using NVespa.NSingleton;

namespace NGame2.NSocket;

public class SocketServiceManager : GlobalAutoBehaviour<SocketServiceManager>
{
	public BattleSocketConnector BattleConnector;

	public MessageSocketConnector MessageConnector;

	public RoomSocketConnector RoomConnector;

	protected override void Awake()
	{
		base.Awake();
		BattleConnector = base.gameObject.AddComponent<BattleSocketConnector>();
		MessageConnector = base.gameObject.AddComponent<MessageSocketConnector>();
		RoomConnector = base.gameObject.AddComponent<RoomSocketConnector>();
	}
}
