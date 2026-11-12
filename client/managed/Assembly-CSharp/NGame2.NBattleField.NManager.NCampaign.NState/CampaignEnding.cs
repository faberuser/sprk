using NGame2.NUI.NManager.NBattle;
using NVespa.NSingleton;

namespace NGame2.NBattleField.NManager.NCampaign.NState;

public class CampaignEnding : StateBase
{
	public override void OnStart(IListener listener)
	{
		base.OnStart(listener);
		SceneAutoBehaviour<BattleManagement>.instance.CampaignEnd = true;
		_listener.OnCampaignEnd();
	}
}
