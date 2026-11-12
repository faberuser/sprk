using System.Collections.Generic;
using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class OptionSellComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(OptionSellData optionData, bool onHead);

		List<int> GetSelectedGrade();
	}

	public const string PrefabPath = "Prefab2/Component/OptionSell/OptionSellComponent";

	public static PrefabAuto<OptionSellComponent> PrefabAuto = new PrefabAuto<OptionSellComponent>("Prefab2/Component/OptionSell/OptionSellComponent");

	public UISpriteSelector BackGroundSelector;

	public UILabel ComponentLabel;

	public UIWidget StarWidget;

	public UILabel StarLabel;

	public UISprite DisableSprite;

	public UIWidget TMStarWidget;

	public UILabel TMStarLabel;

	private Listener _listener;

	private bool _onHead;

	public OptionSellData OptionData { get; private set; }

	public void Open(Listener listener, OptionSellData optionData, bool onHead = false, int width = 0)
	{
		OptionData = optionData;
		_listener = listener;
		_onHead = onHead;
		UIWidget component = GetComponent<UIWidget>();
		if (component != null)
		{
			component.width = width;
			component.ForceUpdateAnchors(includeChildren: true);
		}
		Init();
	}

	public void Refresh(OptionSellData optionData)
	{
		OptionData = optionData;
		Init();
	}

	public void SetOptionData(OptionSellData changeData)
	{
		OptionData = changeData;
	}

	private void Init()
	{
		if (_listener != null)
		{
			bool flag = false;
			List<int> selectedGrade = _listener.GetSelectedGrade();
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(OptionData.GradeDependency))
			{
				int[] gradeDependency = OptionData.GradeDependency;
				foreach (int item in gradeDependency)
				{
					flag = selectedGrade.Contains(item);
					if (flag)
					{
						break;
					}
				}
			}
			else
			{
				flag = true;
			}
			UIHelper.EnableGameObject(DisableSprite, !flag);
		}
		UIHelper.EnableGameObject(ComponentLabel, OptionData.Category != OptionSellCategory.Star && OptionData.Category != OptionSellCategory.TMStar);
		UIHelper.EnableGameObject(StarWidget, OptionData.Category == OptionSellCategory.Star);
		UIHelper.EnableGameObject(TMStarWidget, OptionData.Category == OptionSellCategory.TMStar);
		BackGroundSelector.SelectInt(OptionData.BackGroundIndex);
		switch (OptionData.Category)
		{
		case OptionSellCategory.Tier:
			if (!String_Extension_Method.IsNullOrEmpty(OptionData.ReplaceKey))
			{
				UIHelper.SetLabelText(ComponentLabel, LocalizationManager.GetStringDirect(OptionData.ReplaceKey));
				break;
			}
			UIHelper.SetLabelText(ComponentLabel, LocalizationManager.GetStringDirect("EQUIP_TIER_RANGE", OptionData.Value));
			break;
		case OptionSellCategory.Grade:
			UIHelper.SetLabelText(ComponentLabel, LocalizationManager.GetStringDirect(OptionData.GradeStringKey));
			break;
		case OptionSellCategory.Star:
			UIHelper.SetLabelText(StarLabel, LocalizationManager.GetStringDirect("STAR_UNDER_TIER", OptionData.Value));
			break;
		case OptionSellCategory.TMStar:
			UIHelper.SetLabelText(TMStarLabel, LocalizationManager.GetStringDirect("STAR_UNDER_TIER", OptionData.Value));
			break;
		case OptionSellCategory.RuneGrade:
		case OptionSellCategory.RuneEquipPartType:
			break;
		}
	}

	public void OnClick()
	{
		if (!UIHelper.IsEnabledGameObjet(DisableSprite) && _listener != null)
		{
			_listener.OnClick(OptionData, _onHead);
		}
	}

	public int GetValue()
	{
		return OptionData.Value;
	}
}
