using System.Collections.Generic;
using NGame2.NUI.NComponent2.NPet;
using NShared;
using NVespa;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow.NPet;

public class PetIncubatorRewardPopup : BaseWindow, PetIncubatorRewardComponent.IIncubatorRewardDataProvider
{
	public static string PrefabPath = "Prefab2/Window/Pet/Popup/PetIncubatorRewardPopup";

	public static PrefabAuto<PetIncubatorRewardPopup> PrefabAuto = new PrefabAuto<PetIncubatorRewardPopup>(PrefabPath);

	public UITable Table_RewardList;

	public UIScrollView PetListScrollView;

	private List<PetIncubatorRewardComponent> _incubatorRewardComponents = new List<PetIncubatorRewardComponent>();

	private Dictionary<byte, List<int>> _rewardPetListDic = new Dictionary<byte, List<int>>();

	private int _eggIndex;

	public void Open(int eggIndex)
	{
		base.Open();
		_eggIndex = eggIndex;
		Init();
	}

	private void Init()
	{
		InitPetListDic();
		InitComponents();
	}

	private void InitPetListDic()
	{
		PetEggData data = GameRoot.Instance.PetEggDataContainer.GetData(_eggIndex);
		if (data == null)
		{
			VespaDebug.LogError($"EggData is null : index {_eggIndex}");
		}
		ItemGroupData data2 = GameRoot.Instance.ItemGroupDataContainer.GetData(data.ItemGroupCode);
		if (data2 == null)
		{
			VespaDebug.LogError($"itemgroupData is null : code {data.ItemGroupCode}");
		}
		foreach (RewardElementData rewardItem in data2.GetRewardItemList())
		{
			if (rewardItem == null)
			{
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardItem.ItemCode);
			if (dataByCode != null)
			{
				int index = dataByCode.Index;
				byte grade = GameRoot.Instance.CreatureDataContainer.GetData(index).Grade;
				if (!_rewardPetListDic.ContainsKey(grade))
				{
					_rewardPetListDic[grade] = new List<int>();
				}
				_rewardPetListDic[grade].Add(index);
			}
		}
	}

	private void InitComponents()
	{
		FactoryBase.ReleaseAndClearList(_incubatorRewardComponents);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_rewardPetListDic))
		{
			return;
		}
		byte b = 3;
		for (byte b2 = 6; b2 >= b; b2--)
		{
			if (_rewardPetListDic.ContainsKey(b2) && !__Extension_Method_ICollection__.IsNullOrEmpty(_rewardPetListDic[b2]))
			{
				PetIncubatorRewardComponent petIncubatorRewardComponent = PetIncubatorRewardComponent.PrefabAuto.Factory.Create(Table_RewardList.transform);
				if (!(petIncubatorRewardComponent == null))
				{
					petIncubatorRewardComponent.Open(this, (CreatureGrade)b2);
					_incubatorRewardComponents.Add(petIncubatorRewardComponent);
				}
			}
		}
		Table_RewardList.Reposition();
		PetListScrollView.ResetPosition();
	}

	List<int> PetIncubatorRewardComponent.IIncubatorRewardDataProvider.GetRewardPetIndexList(CreatureGrade grade)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_rewardPetListDic))
		{
			return null;
		}
		return _rewardPetListDic[(byte)grade];
	}

	public override void Close()
	{
		base.Close();
		foreach (KeyValuePair<byte, List<int>> item in _rewardPetListDic)
		{
			item.Value.Clear();
		}
		_rewardPetListDic.Clear();
	}
}
