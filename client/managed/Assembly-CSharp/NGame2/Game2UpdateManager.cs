using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleView;
using NGame2.NBattleView.NUIOn3D;
using NGame2.NBattleView.NUIOn3d2;
using NGame2.NCutScene;
using NGame2.NMessage;
using NGame2.NNodeTree;
using NVespa;
using NVespa.NSingleton;

namespace NGame2;

public class Game2UpdateManager : SceneUpdateBehaviour<Game2UpdateManager>
{
	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		if (SceneAutoBehaviour<OfflinePlay>.isValidInstance)
		{
			SceneAutoBehaviour<OfflinePlay>.instance.ChainUpdate();
		}
		if (SceneAutoBehaviour<OnlinePlay>.isValidInstance)
		{
			SceneAutoBehaviour<OnlinePlay>.instance.ChainUpdate();
		}
		if (SceneAutoBehaviour<RePlay>.isValidInstance)
		{
			SceneAutoBehaviour<RePlay>.instance.ChainUpdate();
		}
		if (SingletonAuto<GlobalManager>.isValidInstance)
		{
			SingletonAuto<GlobalManager>.instance.ChainUpdate();
		}
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			BattleViewGenericSingleton<CreatureManager>.instance.ChainUpdate();
		}
		if (SceneAutoBehaviour<RuntimeGlobalManager>.isValidInstance)
		{
			SceneAutoBehaviour<RuntimeGlobalManager>.instance.ChainUpdate();
		}
		if (SceneAutoBehaviour<CutSceneManager>.isValidInstance)
		{
			SceneAutoBehaviour<CutSceneManager>.instance.ChainUpdate();
		}
		CameraUpdate();
		if (Manager.IsValidInstance)
		{
			Manager.instance.ChainUpdate();
		}
		if (WorldMapManager.IsValidInstance)
		{
			WorldMapManager.instance.ChainUpdate();
		}
		if (BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
		{
			BattleViewGenericSingleton<FieldMapManager2>.instance.ChainUpdate();
		}
		VespaDebug.Update();
	}

	protected override void ChainLateUpdate()
	{
		base.ChainLateUpdate();
		bool flag = false;
		if (GlobalUIRoot.isValidInstance)
		{
			flag |= GlobalUIRoot.instance.RefreshCameraEnable();
		}
		if (GameUIRoot.IsValidInstance)
		{
			flag |= GameUIRoot.instance.RefreshCameraEnable();
		}
		if (Game3DRoot.IsValidInstance)
		{
			flag |= Game3DRoot.instance.RefreshCameraEnable();
		}
		if (flag)
		{
			GlobalAutoBehaviour<GlobalCameraManager>.instance.AdjustCameraRenderAndClearFlags();
		}
	}

	private void CameraUpdate()
	{
		if (Game3DRoot.IsValidInstance)
		{
			Game3DRoot.instance.ChainUpdate();
		}
	}
}
