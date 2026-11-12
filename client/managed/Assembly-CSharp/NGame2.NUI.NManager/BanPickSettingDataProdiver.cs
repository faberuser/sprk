using System.Collections.Generic;
using System.Linq;
using System.Text;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NVespa;

namespace NGame2.NUI.NManager;

public class BanPickSettingDataProdiver : MatchBanPick.IDataProvider
{
	private readonly LocalAccountIntArray _pickMemberLocalStorage;

	private readonly LocalAccountIntArray _banMemberLocalStorage;

	private int _pickSaveMaxCount;

	private int _banSaveMaxCount;

	private List<int> _pickSaveList = new List<int>();

	private List<int> _banSaveList = new List<int>();

	private MatchBanPick.IDataProvider _thisDataProvider;

	public BanPickSettingDataProdiver(string storageKey)
	{
		_pickMemberLocalStorage = new LocalAccountIntArray(0, storageKey + "_PickMember");
		_banMemberLocalStorage = new LocalAccountIntArray(0, storageKey + "_BanMember");
		_pickSaveMaxCount = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickSavePickMaxCount", 10);
		_banSaveMaxCount = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickSaveBanMaxCount", 4);
		_thisDataProvider = this;
		InitPickBanSaveList();
	}

	public List<int> GetPickSaveList()
	{
		return _pickSaveList;
	}

	public List<int> GetBanSaveList()
	{
		return _banSaveList;
	}

	public static void PrintListWtihHeroName(string prefix, List<int> heroList)
	{
		if (Define.IsLiveBuild())
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(prefix);
		int num = 0;
		foreach (int hero in heroList)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(hero);
			stringBuilder.Append($"[{num++}. [{data.Index}]{data.Name}] ");
		}
	}

	private void InitPickBanSaveList()
	{
		if (_thisDataProvider == null)
		{
			VespaDebug.LogError("MatchBanPick.IDataProvider :: Init Failed ");
			return;
		}
		for (int i = 0; i < _pickSaveMaxCount; i++)
		{
			int banPickMemberHeroIndex = _thisDataProvider.GetBanPickMemberHeroIndex(i, isPick: true);
			if (banPickMemberHeroIndex != 0)
			{
				_pickSaveList.Add(banPickMemberHeroIndex);
			}
			_pickSaveList = _pickSaveList.Distinct().ToList();
		}
		for (int j = 0; j < _banSaveMaxCount; j++)
		{
			int banPickMemberHeroIndex2 = _thisDataProvider.GetBanPickMemberHeroIndex(j, isPick: false);
			if (banPickMemberHeroIndex2 != 0)
			{
				_banSaveList.Add(banPickMemberHeroIndex2);
			}
			_banSaveList = _banSaveList.Distinct().ToList();
		}
	}

	public void SaveBanPickHeroIndex(List<int> pickHeroIndices, List<int> banHeroIndices)
	{
		pickHeroIndices.AddRange(_pickSaveList.Except(pickHeroIndices));
		_pickSaveList = pickHeroIndices;
		banHeroIndices.AddRange(_banSaveList.Except(banHeroIndices));
		_banSaveList = banHeroIndices;
		ClampList();
		SaveList();
		ClearList();
	}

	private void ClampList()
	{
		ClampList(_pickSaveList, _pickSaveMaxCount);
		ClampList(_banSaveList, _banSaveMaxCount);
	}

	private void ClampList(List<int> list, int maxCount)
	{
		if (list.Count > maxCount)
		{
			int count = list.Count - maxCount;
			list.RemoveRange(maxCount, count);
		}
	}

	private void SaveList()
	{
		if (_thisDataProvider == null)
		{
			VespaDebug.LogError("MatchBanPick.IDataProvider :: SaveSelectList Failed ");
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_pickSaveList); i++)
		{
			_thisDataProvider.SetBanPickMemberHeroIndex(i, _pickSaveList[i]);
		}
		PrintListWtihHeroName("SaveList :: PickList", _pickSaveList);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_banSaveList); j++)
		{
			_thisDataProvider.SetBanPickMemberHeroIndex(j, _banSaveList[j], isPick: false);
		}
		PrintListWtihHeroName("SaveList :: BanList", _banSaveList);
	}

	private void ClearList()
	{
		_pickSaveList.Clear();
		_banSaveList.Clear();
	}

	int MatchBanPick.IDataProvider.GetBanPickMemberHeroIndex(int slotIndex, bool isPick)
	{
		if (isPick)
		{
			return _pickMemberLocalStorage[slotIndex];
		}
		return _banMemberLocalStorage[slotIndex];
	}

	void MatchBanPick.IDataProvider.SetBanPickMemberHeroIndex(int slotIndex, int heroIndex, bool isPick)
	{
		if (isPick)
		{
			_pickMemberLocalStorage[slotIndex] = heroIndex;
		}
		else
		{
			_banMemberLocalStorage[slotIndex] = heroIndex;
		}
	}
}
