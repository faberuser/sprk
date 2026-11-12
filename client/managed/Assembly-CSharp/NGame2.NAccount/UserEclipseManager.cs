using System.Collections.Generic;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount;

public class UserEclipseManager
{
	private EclipseDeckResult[] SavedDeckInfos;

	private EclipseInfo _eclipseInfo;

	private EclipseDungeonInfo _rewardInfo;

	public int MaxGameSpeed => 3;

	public void ClearSavedDeckInfos()
	{
		SavedDeckInfos = null;
	}

	public EclipseDeckResult[] GetSavedDeckInfos()
	{
		return SavedDeckInfos;
	}

	public EclipseDungeonInfo GetEclipseRewardInfo()
	{
		return _rewardInfo;
	}

	public EclipseInfo GetEclipseInfo()
	{
		return _eclipseInfo;
	}

	public void Apply(EclipseDeckResult[] deckInfos)
	{
		SavedDeckInfos = deckInfos;
	}

	public void Apply(EclipseDungeonInfo rewardInfo)
	{
		_rewardInfo = rewardInfo;
	}

	public void Apply(EclipseInfo eclipseInfo)
	{
		if (eclipseInfo != null)
		{
			_eclipseInfo = eclipseInfo;
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(eclipseInfo.EclipseStaminaResult);
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(eclipseInfo.EclipsePointResult);
			if (eclipseInfo.DeckResults != null && !eclipseInfo.DeckResults.Equals(SavedDeckInfos))
			{
				Apply(eclipseInfo.DeckResults);
			}
		}
	}

	public void SetDeckIndex(int deckIndex)
	{
		if (_eclipseInfo != null)
		{
			_eclipseInfo.DeckIndex = deckIndex;
		}
	}

	public bool IsLastDeck()
	{
		if (_eclipseInfo == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(SavedDeckInfos) == 0)
		{
			return false;
		}
		return __Extension_Method_ICollection__.COUNT(SavedDeckInfos) <= _eclipseInfo.DeckIndex;
	}

	public int GetSavedDeckInfoCount()
	{
		return __Extension_Method_ICollection__.COUNT(SavedDeckInfos);
	}

	public HeroInfo[] GetCurrentHeroInfos()
	{
		if (SavedDeckInfos == null)
		{
			return null;
		}
		if (_eclipseInfo == null)
		{
			return null;
		}
		if (_eclipseInfo.DeckIndex - 1 > __Extension_Method_ICollection__.COUNT(SavedDeckInfos))
		{
			return null;
		}
		EclipseDeckResult eclipseDeckResult = SavedDeckInfos[_eclipseInfo.DeckIndex - 1];
		if (eclipseDeckResult == null)
		{
			return null;
		}
		if (__Extension_Method_ICollection__.COUNT(eclipseDeckResult.CachedHeroInfos) == 0)
		{
			return null;
		}
		return eclipseDeckResult.CachedHeroInfos;
	}

	public List<int> GetHeroIndexList(int deckIndex)
	{
		deckIndex--;
		if (SavedDeckInfos.Length <= deckIndex || SavedDeckInfos[deckIndex] == null || String_Extension_Method.IsNullOrEmpty(SavedDeckInfos[deckIndex].HeroIndices))
		{
			return null;
		}
		List<int> list = new List<int>();
		list.Clear();
		string[] array = SavedDeckInfos[deckIndex].HeroIndices.Split(',');
		int result = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (int.TryParse(array[i], out result) && result > 0)
			{
				list.Add(result);
			}
		}
		return list;
	}

	public int GetBestTeamClearWaveIndex()
	{
		if (SavedDeckInfos == null)
		{
			return 0;
		}
		EclipseDeckResult eclipseDeckResult = null;
		int num = 0;
		for (int i = 0; i < SavedDeckInfos.Length; i++)
		{
			eclipseDeckResult = SavedDeckInfos[i];
			if (eclipseDeckResult != null && num < eclipseDeckResult.ClearMaxWaveIndex)
			{
				num = eclipseDeckResult.ClearMaxWaveIndex;
			}
		}
		return num;
	}

	public void SetGameSpeed(int gameSpeed)
	{
		if (_rewardInfo == null)
		{
			_rewardInfo = new EclipseDungeonInfo();
		}
		_rewardInfo.OnlineGameSpeedRatio = gameSpeed;
	}

	public void SetStartWaveIndex(int waveIndex)
	{
		if (_rewardInfo == null)
		{
			_rewardInfo = new EclipseDungeonInfo();
		}
		_rewardInfo.StartWaveIndex = waveIndex;
	}

	public bool CheckAdvanceWave(int clearWaveIndex)
	{
		if (_eclipseInfo == null)
		{
			return false;
		}
		EclipseStartData data = GameRoot.Instance.EclipseStartDataContainer.GetData(_eclipseInfo.MaxWaveIndex);
		if (data == null)
		{
			return false;
		}
		return data.StartWave <= clearWaveIndex;
	}
}
