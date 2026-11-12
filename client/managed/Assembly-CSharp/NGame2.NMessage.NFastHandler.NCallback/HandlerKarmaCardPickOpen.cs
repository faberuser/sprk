using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NMessage.NFastHandler.NCallback;

public class HandlerKarmaCardPickOpen : FastHandler<CallbackMessage<KarmaCardSuffle>>
{
	protected override bool OnMessage(CallbackMessage<KarmaCardSuffle> message)
	{
		if (!SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			return false;
		}
		KarmaCardSelectWindow instance = SceneAutoBehaviour<BattleManagement>.instance.GetInstance<KarmaCardSelectWindow>(openedOnly: true);
		if (instance == null)
		{
			return false;
		}
		if (message == null || message.Callback == null)
		{
			return false;
		}
		instance.SetCardList(message.Callback.SuffledCardIndexes);
		return true;
	}
}
