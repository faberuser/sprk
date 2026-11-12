using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

public class ContentUIDispatchWinLoseTable : MonoBehaviour, IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>, IUserObserver
{
	public UIScrollGrid ScrollGridWinTable;

	public UILabel LabelWinRate;

	public UILabel LabelCurrentBattleCount;

	public bool ShowOnlyRate = true;

	private PrefabAuto<ContentUIDispatchBattleWinLoseComponent> _compPrefabAuto = new PrefabAuto<ContentUIDispatchBattleWinLoseComponent>("Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchBattleWinLoseComponent");

	private WCL_DispatchBattleInfo _info;

	private bool _rateOnly;

	private List<bool> _dataList = new List<bool>();

	private List<ContentUIDispatchBattleWinLoseComponent> _compList = new List<ContentUIDispatchBattleWinLoseComponent>();

	public void SetData(WCL_DispatchBattleInfo info, bool showOnlyRate = false)
	{
		_info = info;
		_rateOnly = showOnlyRate;
	}

	public void UpdateUI(bool isFixScroll = false)
	{
		UpdateDataList();
		UpdateUIList(isFixScroll);
		DispatchUIHelper.UpdateWinRate(_info.LastWinCount, _info.LastLoseCount, LabelWinRate, 0.4f, Color.red, Color.white, _rateOnly);
		UpdateProgressingBattleCount();
	}

	private void OnEnable()
	{
		VespaCoroutine.Start(this, CoProcessingUpdateTable());
	}

	private void OnDisable()
	{
		_dataList.Clear();
		FactoryBase.ReleaseAndClearList(_compList);
		UIHelper.SetLabelText(LabelWinRate, "");
		UIHelper.SetLabelText(LabelCurrentBattleCount, "");
	}

	private void UpdateDataList()
	{
		_dataList.Clear();
		if (_info != null)
		{
			int i = 0;
			for (int num = __Extension_Method_ICollection__.COUNT(_info.ServerResultList); i < num && AddData(i); i++)
			{
			}
		}
	}

	private bool AddData(int index)
	{
		if (!Vespa_Generic_IList_Extension_Method.IsValidIndex(_info.ServerResultList, index))
		{
			return false;
		}
		if (_info.ServerResultList[index] == 1)
		{
			_dataList.Add(item: true);
		}
		else
		{
			_dataList.Add(item: false);
		}
		return true;
	}

	private void UpdateUIList(bool useFixedScroll)
	{
		FactoryBase.ReleaseAndClearList(_compList);
		ContentUIDispatchBattleWinLoseComponent comp = null;
		int i = 0;
		for (int count = _dataList.Count; i < count; i++)
		{
			bool isWin = _dataList[i];
			comp = AddItem(i, isWin);
		}
		if (ScrollGridWinTable != null)
		{
			ScrollGridWinTable.RepositionGridWithFixableScroll(useFixedScroll);
			if (!useFixedScroll)
			{
				ScrollGridWinTable.ScrollTo(comp, 0f, 0f);
			}
		}
	}

	private void UpdateProgressingBattleCount()
	{
		if (_info != null)
		{
			int num = __Extension_Method_ICollection__.COUNT(_info.ServerResultList);
			switch (_info.State)
			{
			case DispatchState.Battle:
			{
				int num2 = Mathf.Min(_info.RepeatCount, num + 1);
				UIHelper.SetLabelText(LabelCurrentBattleCount, LocalizationManager.GetString(DispatchBattle.DISPATCH_COUNT_PROGRESSING, num2));
				break;
			}
			case DispatchState.ReadyToComplete:
				UIHelper.SetLabelText(LabelCurrentBattleCount, LocalizationManager.GetString(DispatchBattle.DISPATCH_COUNT_DONE, num));
				break;
			}
		}
	}

	private ContentUIDispatchBattleWinLoseComponent AddItem(int index, bool isWin)
	{
		if (ScrollGridWinTable == null)
		{
			return null;
		}
		ContentUIDispatchBattleWinLoseComponent contentUIDispatchBattleWinLoseComponent = ScrollGridWinTable.AddGridItem(_compPrefabAuto);
		if (contentUIDispatchBattleWinLoseComponent == null)
		{
			return null;
		}
		int count = index + 1;
		contentUIDispatchBattleWinLoseComponent.Open(count, isWin);
		_compList.Add(contentUIDispatchBattleWinLoseComponent);
		return contentUIDispatchBattleWinLoseComponent;
	}

	private IEnumerator CoProcessingUpdateTable()
	{
		WaitForSeconds waitTime = new WaitForSeconds(1f);
		yield return new WaitUntil(() => _info != null);
		UpdateUI();
		yield return waitTime;
		while (true)
		{
			if (_info == null)
			{
				yield return waitTime;
			}
			int num = __Extension_Method_ICollection__.COUNT(_dataList);
			int num2 = __Extension_Method_ICollection__.COUNT(_info.ServerResultList) - __Extension_Method_ICollection__.COUNT(_dataList);
			if (num2 == 0)
			{
				yield return waitTime;
				continue;
			}
			if (num2 > 0)
			{
				ContentUIDispatchBattleWinLoseComponent comp = null;
				int num3 = 0;
				for (int num4 = num2; num3 < num4; num3++)
				{
					int index = num + num3;
					AddData(index);
					if (Vespa_Generic_IList_Extension_Method.IsValidIndex(_dataList, index))
					{
						comp = AddItem(index, _dataList[index]);
					}
				}
				ScrollGridWinTable.RepositionGridWithFixableScroll(useFix: true);
				ScrollGridWinTable.ScrollTo(comp, 0f, 0f);
				DispatchUIHelper.UpdateWinRate(_info.LastWinCount, _info.LastLoseCount, LabelWinRate, 0.4f, Color.red, Color.white, _rateOnly);
				UpdateProgressingBattleCount();
			}
			else if (num2 < 0)
			{
				UpdateUI(isFixScroll: true);
			}
			yield return waitTime;
		}
	}

	void IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>.OnChanged(UserDispatchBattleInfoManager.Observer.BroadCastInfo changedInfo)
	{
		if (changedInfo.Reason == UserDispatchBattleInfoManager.Observer.BroadCastReason.OnUpdate)
		{
			SetData(changedInfo.Info);
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}
}
