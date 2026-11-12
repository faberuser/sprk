using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GameOptionButtonComponent : GameOptionBaseComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GameOption/GameOptionButtonComponent";

	public static PrefabAuto<GameOptionButtonComponent> PrefabAuto = new PrefabAuto<GameOptionButtonComponent>("Prefab2/Component/GameOption/GameOptionButtonComponent");

	public UILabel Label_Button;

	public UIButton Button;

	public void Open(Listener listener, NShared.GameOptionData _gameOptionData)
	{
		_listener = listener;
		gameOptionData = _gameOptionData;
		if (gameOptionData != null)
		{
			SetUIRoot();
			SetLabel();
		}
	}

	public void SetLabel()
	{
		if (gameOptionData != null)
		{
			UIHelper.SetLabelText(ParentRoot.Label_OptionName, LocalizationManager.GetStringDirect(gameOptionData.OptionName));
			UIHelper.SetLabelText(ChildRoot.Label_OptionName, LocalizationManager.GetStringDirect(gameOptionData.OptionName));
			UIHelper.SetLabelText(Label_Button, LocalizationManager.GetStringDirect(gameOptionData.ValueLocalKey1));
		}
	}

	public void OnClickButton()
	{
		RunPostProcessFunction(null);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
