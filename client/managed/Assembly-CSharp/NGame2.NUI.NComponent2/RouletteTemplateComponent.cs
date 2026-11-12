using System.Collections.Generic;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class RouletteTemplateComponent : LinkFactoryComponent, InitOnCreate, CurrencyIconComponent.Listener, ItemIconComponent.Listener
{
	public const string PrefabPath = "Prefab2/Component/MiniGame/RouletteTemplateComponent";

	public static PrefabAuto<RouletteTemplateComponent> PrefabAuto = new PrefabAuto<RouletteTemplateComponent>("Prefab2/Component/MiniGame/RouletteTemplateComponent");

	public int SectionIndex;

	public UISprite[] SprReward;

	public UILabel LabelCount;

	private List<Component> components = new List<Component>();

	private List<RouletteTemplateEffectComponent> effectComponents = new List<RouletteTemplateEffectComponent>();

	public void OnCreate()
	{
	}

	public void OnRelease()
	{
		effectComponents.ForEach(delegate(RouletteTemplateEffectComponent e)
		{
			e.SetEnable(isEnable: false);
		});
		FactoryBase.ReleaseAndClearList(effectComponents);
		FactoryBase.ReleaseAndClearList(components);
	}

	public void Open(string label, DbRewardData[] rewards, Vector3 eularAngle, int sectionIndex)
	{
		base.transform.eulerAngles = eularAngle;
		SectionIndex = sectionIndex;
		UIHelper.EnableGameObject(LabelCount, Roulette.LOCAL_TEST);
		UIHelper.SetLabelText(LabelCount, label);
		Component component = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(rewards); i++)
		{
			if (i < __Extension_Method_ICollection__.COUNT(SprReward))
			{
				if (rewards[i].Type == RewardType.None)
				{
					SprReward[i].gameObject.SetActive(value: false);
					continue;
				}
				RouletteTemplateEffectComponent rouletteTemplateEffectComponent = RouletteTemplateEffectComponent.PrefabAuto.Factory.Create(SprReward[i].transform);
				rouletteTemplateEffectComponent.Open(eularAngle);
				SprReward[i].gameObject.SetActive(value: true);
				component = CreateConponent(rewards[i], SprReward[i].transform);
				AssignComponentSize(component, SprReward[i].transform);
				effectComponents.Add(rouletteTemplateEffectComponent);
				components.Add(component);
			}
		}
	}

	public void PlayEffect()
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(effectComponents))
		{
			effectComponents.ForEach(delegate(RouletteTemplateEffectComponent e)
			{
				e.SetEnable(isEnable: true);
			});
		}
	}

	private Component CreateConponent(DbRewardData reward, Transform parent)
	{
		Component component = null;
		if (reward.Type != RewardType.Item)
		{
			component = CurrencyIconComponent.PrefabAuto.Factory.Create(parent);
			if (component == null)
			{
				return null;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(component.transform);
			CurrencyIconComponent currencyIconComponent = component as CurrencyIconComponent;
			if (currencyIconComponent == null)
			{
				return null;
			}
			CurrencyType currencyType = UIHelper.GetCurrencyType(reward.Type);
			int value = reward.Value;
			currencyIconComponent.Open(currencyType, value, this);
		}
		else
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(reward.Value);
			if (data == null)
			{
				return null;
			}
			component = ItemIconComponent.PrefabAuto.Factory.Create(parent);
			if (component == null)
			{
				return null;
			}
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(component.transform);
			ItemIconComponent itemIconComponent = component as ItemIconComponent;
			if (itemIconComponent == null)
			{
				return null;
			}
			if (data.Type == ItemType.Equip)
			{
				EquipItemInfo equipItemInfo = new EquipItemInfo();
				equipItemInfo.ItemIndex = reward.Value;
				equipItemInfo.Star = reward.Star;
				if (!itemIconComponent.Open(equipItemInfo, this))
				{
					FactoryBase.ReleaseComponent(component);
					return null;
				}
			}
			else
			{
				itemIconComponent.Open(data, this);
				int count = reward.Count;
				Color? color = null;
				int num = count;
				itemIconComponent.SetItemCount(num, color);
			}
		}
		return component;
	}

	private void AssignComponentSize(Component component, Transform parent)
	{
		UIWidget component2 = parent.GetComponent<UIWidget>();
		UIWidget component3 = component.GetComponent<UIWidget>();
		if (component2 == null || component3 == null)
		{
			component.transform.localScale = parent.transform.localScale;
			return;
		}
		component3.width = component2.width;
		component3.height = component2.height;
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent component)
	{
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent component)
	{
	}
}
