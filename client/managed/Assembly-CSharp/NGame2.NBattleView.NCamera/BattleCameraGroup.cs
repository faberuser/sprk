using System.Collections.Generic;

namespace NGame2.NBattleView.NCamera;

public class BattleCameraGroup
{
	private List<BattleCamera> _listBattleCam;

	private int _currentCamIndex;

	public BattleCameraGroup(int camCount = 2)
	{
		if (camCount == 0)
		{
			camCount = 1;
		}
		_listBattleCam = new List<BattleCamera>();
		for (int i = 0; i < camCount; i++)
		{
			BattleCamera item = new BattleCamera
			{
				GroupIndex = i
			};
			_listBattleCam.Add(item);
		}
	}

	public void SetWaveIndex(int waveIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listBattleCam); i++)
		{
			_listBattleCam[i].WaveIndex = waveIndex;
		}
	}

	public BattleCamera GetNextBattleCamera()
	{
		if (_listBattleCam.Count <= _currentCamIndex + 1)
		{
			_currentCamIndex = 0;
		}
		else
		{
			_currentCamIndex++;
		}
		return _listBattleCam[_currentCamIndex];
	}

	public BattleCamera GetCurrentBattleCamera()
	{
		return _listBattleCam[_currentCamIndex];
	}
}
