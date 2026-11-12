using System;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class GameOptionButtonTextComponent : GameOptionBaseComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GameOption/GameOptionButtonTextComponent";

	public static PrefabAuto<GameOptionButtonTextComponent> PrefabAuto = new PrefabAuto<GameOptionButtonTextComponent>("Prefab2/Component/GameOption/GameOptionButtonTextComponent");

	public UILabel Label_Button;

	public UIButton Button;

	public UILabel Label_ExtraValue;

	public void Open(Listener listener, NShared.GameOptionData _gameOptionData)
	{
		_listener = listener;
		gameOptionData = _gameOptionData;
		if (gameOptionData != null)
		{
			SetUIRoot();
			SetLabel();
			SetExtraLabel();
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

	public void SetExtraLabel()
	{
		if (gameOptionData != null)
		{
			bool flag = !String_Extension_Method.IsNullOrEmpty(gameOptionData.ValueLocalKey2);
			UIHelper.EnableGameObject(Label_ExtraValue, flag);
			if (flag)
			{
				object optionValue = GetOptionValue(gameOptionData.ClassName, gameOptionData.FieldName);
				UIHelper.SetLabelText(Label_ExtraValue, optionValue);
			}
		}
	}

	public void OnClickButton()
	{
		Action value = delegate
		{
			Refresh();
		};
		RunPostProcessFunction(value);
	}

	public override void Refresh()
	{
		base.Refresh();
		SetExtraLabel();
	}

	private void OnDisable()
	{
		Refresh();
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
