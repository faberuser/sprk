using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ProbabilityTabComponent : UIScrollItemBehaviour, InitOnCreate
{
	public new interface IListener : UIScrollItemBehaviour.IListener
	{
		void OnClickTab(int index);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/PayShop2/PayShopProbabilityTabComponent";

	public static PrefabAuto<ProbabilityTabComponent> PrefabAuto = new PrefabAuto<ProbabilityTabComponent>("Resources/Prefab2/Component/PayShop2/PayShopProbabilityTabComponent");

	public static bool IsBlockToggleEvent = false;

	public UILabel Label_Name;

	public UIToggle Tab_Toggle;

	private int tabIndex;

	private IListener _listener;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(int Index, string name, IListener listener)
	{
		tabIndex = Index;
		_listener = listener;
		SelectToggle(Index == 0);
		UIHelper.SetLabelText(Label_Name, name);
	}

	public void OnClickToggle()
	{
		if (!(Tab_Toggle == null) && Tab_Toggle.value && !IsBlockToggleEvent && _listener != null)
		{
			_listener.OnClickTab(tabIndex);
		}
	}

	public void SelectToggle(bool isSelect, bool noEvent = false)
	{
		if (noEvent)
		{
			IsBlockToggleEvent = true;
		}
		if (Tab_Toggle != null)
		{
			if (!isSelect)
			{
				Tab_Toggle.optionCanBeNone = true;
				Tab_Toggle.value = isSelect;
				Tab_Toggle.optionCanBeNone = false;
			}
			else
			{
				Tab_Toggle.value = isSelect;
			}
		}
		if (noEvent)
		{
			IsBlockToggleEvent = false;
		}
	}
}
