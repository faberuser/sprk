using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class OptionGradeComponent : LinkFactoryComponent
{
	public interface Listener
	{
		void OnClick(OptionSellData optionData, bool isChecked);
	}

	public const string PrefabPath = "Prefab2/Component/OptionSell/OptionGradeComponent";

	public static PrefabAuto<OptionGradeComponent> PrefabAuto = new PrefabAuto<OptionGradeComponent>("Prefab2/Component/OptionSell/OptionGradeComponent");

	public UILabel ComponentLabel;

	public UIWidget WidgetCheck;

	private Listener _listener;

	public OptionSellData OptionData { get; private set; }

	public bool IsChecked { get; private set; }

	public void Open(Listener listener, OptionSellData optionData, bool isChecked)
	{
		OptionData = optionData;
		IsChecked = isChecked;
		_listener = listener;
		Init();
	}

	private void Init()
	{
		if (OptionData.Category == OptionSellCategory.Grade || OptionData.Category == OptionSellCategory.RuneGrade || OptionData.Category == OptionSellCategory.RuneEquipPartType)
		{
			UIHelper.SetLabelText(ComponentLabel, LocalizationManager.GetStringDirect(OptionData.GradeStringKey));
		}
		UIHelper.EnableGameObject(WidgetCheck, IsChecked);
	}

	public void ForceCheck(bool isChecked)
	{
		IsChecked = isChecked;
		UIHelper.EnableGameObject(WidgetCheck, IsChecked);
	}

	public void OnClickCheck()
	{
		IsChecked = !IsChecked;
		UIHelper.EnableGameObject(WidgetCheck, IsChecked);
		if (_listener != null)
		{
			_listener.OnClick(OptionData, IsChecked);
		}
	}
}
