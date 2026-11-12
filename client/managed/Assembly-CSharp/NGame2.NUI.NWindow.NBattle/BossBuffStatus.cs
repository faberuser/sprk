using NGame2.NUtil;
using NVespa.NNGUIExtension;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BossBuffStatus : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BossBuffStatus";

	public UISimpleGrid BuffGrid;

	private BattleStateUiController _stateUiController = new BattleStateUiController();

	public void Open(int creatureUid, Transform parent)
	{
		Open(delegate
		{
			Init(creatureUid, parent);
		}, null);
	}

	public void Init(int creatureUid, Transform parent)
	{
		_stateUiController.Open(creatureUid, BuffGrid);
		base.transform.parent = parent;
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(base.transform);
	}

	public override void Reset()
	{
		base.Reset();
		_stateUiController.Reset();
	}

	public override void Close()
	{
		_stateUiController.Reset();
		base.Close();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		_stateUiController.ChainUpdate();
	}
}
