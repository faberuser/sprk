using System.Collections.Generic;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NGuildAttendance;

public class GuildAttendanceRewardListComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/Reward/GuildAttendanceRewardListComponent";

	public static PrefabAuto<GuildAttendanceRewardListComponent> PrefabAuto = new PrefabAuto<GuildAttendanceRewardListComponent>("Prefab2/Component/Reward/GuildAttendanceRewardListComponent");

	public Transform IconRootTransform;

	public UILabel DayLabel;

	public UILabel RewardDescLabel;

	private Component _component;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.SafeReleaseComponent(ref _component);
	}

	public void Open(GuildAttendanceRewardData data)
	{
		Init(data);
	}

	private void Init(GuildAttendanceRewardData data)
	{
		UIHelper.SetLabelText(DayLabel, (data != null) ? data.Day.ToString() : "-");
		SetRewardInfo(data);
	}

	private void SetRewardInfo(GuildAttendanceRewardData data)
	{
		if (IconRootTransform == null || data == null)
		{
			return;
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.RewardIndex);
		if (data2 == null)
		{
			return;
		}
		List<RewardElementData> elementList = data2.GetElementList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(elementList))
		{
			return;
		}
		RewardElementData rewardElementData = null;
		for (int i = 0; i < elementList.Count; i++)
		{
			rewardElementData = elementList[i];
			if (rewardElementData != null)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElementData.ItemCode);
				if (dataByCode != null)
				{
					UIHelper.SetLabelText(RewardDescLabel, dataByCode.Name + "  x" + rewardElementData.ItemMin);
					CreateRewardIconComponent(rewardElementData, dataByCode, IconRootTransform);
				}
			}
		}
	}

	private void CreateRewardIconComponent(RewardElementData rewardElementData, ItemData itemData, Transform rootTransform)
	{
		if (rewardElementData == null || itemData == null || rootTransform == null)
		{
			return;
		}
		_component = null;
		if (rewardElementData.IsGold() || rewardElementData.IsGem())
		{
			_component = CurrencyIconComponent.PrefabAuto.Factory.Create(rootTransform);
			if (_component == null)
			{
				return;
			}
			CurrencyIconComponent obj = _component as CurrencyIconComponent;
			CurrencyType currencyType = rewardElementData.GetCurrencyType();
			int itemMin = rewardElementData.ItemMin;
			CurrencyIconComponent.Listener listener = null;
			int num = itemMin;
			obj.Open(currencyType, num, listener);
		}
		else
		{
			_component = ItemIconComponent.PrefabAuto.Factory.Create(rootTransform);
			if (_component == null)
			{
				return;
			}
			(_component as ItemIconComponent).Open(itemData);
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_component.transform);
		float num2 = 0.6f;
		_component.transform.localScale = new Vector3(num2, num2, num2);
	}
}
