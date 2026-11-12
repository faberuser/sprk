using System.Collections;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenTower : IActivity
{
	private int _towerIndex;

	public static IActivity Create(string[] values)
	{
		return new OpenTower(ArgParser.GetValueInt(values, 0));
	}

	public OpenTower(int towerIndex)
	{
		_towerIndex = towerIndex;
	}

	IEnumerator IActivity.coStart()
	{
		SceneAutoBehaviour<TowerManagement>.instance.OpenTower(_towerIndex);
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
