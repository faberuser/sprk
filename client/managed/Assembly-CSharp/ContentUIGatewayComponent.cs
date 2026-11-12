using System.Collections.Generic;
using NGame2.NUI;
using NGame2.NUI.NComponent2;
using NGame2.NUtil.NDataHelper;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

public class ContentUIGatewayComponent : LinkFactoryComponent
{
	public enum State
	{
		Close,
		Open,
		Cleared
	}

	public enum Mode
	{
		Normal,
		HeroReward
	}

	public interface IListener
	{
		void OnClickEnter(ContentUIGatewayComponent comp, GatewayToTrialData gatewayData);
	}

	public const string PATH_PREFAB = "Resources/Prefab2/Component/Tower/ContentUIGatewayComponent";

	public UILabel LabelName;

	public UIButton ButtonEnter;

	public UIScrollGrid ScrollGridRewards;

	public GameObjectsEnabler[] EnablerState;

	public GameObjectsEnabler[] EnablerMode;

	private State _state;

	private Mode _mode;

	private GatewayToTrialData _data;

	private IListener _listener;

	private PrefabAuto<ItemIconComponent> _compPrefabAuto = new PrefabAuto<ItemIconComponent>("Prefab2/Component/Icon/ItemIconComponent");

	private List<RewardElementData> _dataList = new List<RewardElementData>();

	private List<ItemIconComponent> _compList = new List<ItemIconComponent>();

	public GatewayToTrialData Data => _data;

	public void OnClickEnter()
	{
		if (_listener != null)
		{
			_listener.OnClickEnter(this, _data);
		}
	}

	public void Open(GatewayToTrialData data, IListener listener)
	{
		_data = data;
		_listener = listener;
		ChangeMode(Mode.Normal);
		UpdateState();
		UpdateDataList();
		UpdateUI();
	}

	public void ChangeMode(Mode mode)
	{
		_mode = mode;
		UpdateEnabler(EnablerMode, (int)_mode);
		if (_mode != Mode.Normal)
		{
			_ = 1;
		}
	}

	public void ChangeState(State state)
	{
		_state = state;
		UpdateEnabler(EnablerState, (int)_state);
		switch (_state)
		{
		}
	}

	private void UpdateUI()
	{
		UpdateUIDungeonName();
		UpdateUIList();
	}

	private void UpdateUIDungeonName()
	{
		if (_data != null)
		{
			UIHelper.SetLabelText(LabelName, LocalizationManager.GetStringDirect(_data.DungeonName, _data.Step));
		}
	}

	private void UpdateState()
	{
		if (GatewayDataHelper.IsCleared(_data))
		{
			ChangeState(State.Cleared);
		}
		else if (GatewayDataHelper.IsOpened(_data))
		{
			ChangeState(State.Open);
		}
		else
		{
			ChangeState(State.Close);
		}
	}

	private void UpdateDataList()
	{
		_dataList.Clear();
		if (_data == null)
		{
			return;
		}
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(_data.RewardIndex);
		if (data == null)
		{
			return;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(rewardItemList); i < num; i++)
		{
			RewardElementData rewardElementData = rewardItemList[i];
			if (rewardElementData != null)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
				if (dataByCode != null && dataByCode.Type == ItemType.Hero && _mode == Mode.Normal)
				{
					ChangeMode(Mode.HeroReward);
				}
				_dataList.Add(rewardElementData);
			}
		}
		_dataList.Reverse();
	}

	private void UpdateUIList()
	{
		if (ScrollGridRewards == null)
		{
			return;
		}
		FactoryBase.ReleaseAndClearList(_compList);
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_dataList); i < num; i++)
		{
			RewardElementData rewardElementData = _dataList[i];
			if (rewardElementData == null)
			{
				continue;
			}
			ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
			if (dataByCode != null)
			{
				ItemIconComponent itemIconComponent = ScrollGridRewards.AddGridItem(_compPrefabAuto);
				if (!(itemIconComponent == null))
				{
					itemIconComponent.Open(dataByCode);
					int itemMin = rewardElementData.ItemMin;
					Color? color = null;
					int num2 = itemMin;
					itemIconComponent.SetItemCount(num2, color);
					_compList.Add(itemIconComponent);
				}
			}
		}
		ScrollGridRewards.RepositionGrid();
	}

	private bool IsHeroReward()
	{
		if (GameRoot.Instance.RewardDataContainer.GetData(_data.RewardIndex) == null)
		{
			return false;
		}
		return true;
	}

	private bool IsExistHeroReward(int rewardIndex)
	{
		return false;
	}

	private void UpdateEnabler(GameObjectsEnabler[] enablers, int value)
	{
		int i = 0;
		for (int num = enablers.Length; i < num; i++)
		{
			GameObjectsEnabler gameObjectsEnabler = enablers[i];
			if (!(gameObjectsEnabler == null))
			{
				if (i == value)
				{
					gameObjectsEnabler.SetEnableAll(enable: true);
				}
				else
				{
					gameObjectsEnabler.SetEnableAll(enable: false);
				}
			}
		}
	}
}
