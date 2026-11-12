using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GameOptionSliderComponent : GameOptionBaseComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/GameOption/GameOptionSliderComponent";

	public static PrefabAuto<GameOptionSliderComponent> PrefabAuto = new PrefabAuto<GameOptionSliderComponent>("Prefab2/Component/GameOption/GameOptionSliderComponent");

	public UISlider Slider;

	public UILabel Label_SliderValue;

	public void Open(Listener listener, NShared.GameOptionData _gameOptionData)
	{
		_listener = listener;
		gameOptionData = _gameOptionData;
		if (gameOptionData != null)
		{
			SetType();
			SetUIRoot();
			SetSlider();
			SetLabel();
		}
	}

	private void OnDisable()
	{
		Refresh();
	}

	public void SetLabel()
	{
		UIHelper.SetLabelText(ParentRoot.Label_OptionName, LocalizationManager.GetStringDirect(gameOptionData.OptionName));
		UIHelper.SetLabelText(ChildRoot.Label_OptionName, LocalizationManager.GetStringDirect(gameOptionData.OptionName));
	}

	public void SetSlider()
	{
		object optionValue = GetOptionValue(gameOptionData.ClassName, gameOptionData.FieldName);
		if (!(Slider == null))
		{
			Slider.value = (float)optionValue;
			UIHelper.SetLabelText(Label_SliderValue, Mathf.RoundToInt(Slider.value * 100f));
		}
	}

	public void OnSliderValueChanged()
	{
		object optionValue = GetOptionValue(gameOptionData.ClassName, gameOptionData.FieldName);
		float value = Slider.value;
		if (!Mathf.Approximately((float)optionValue, value))
		{
			UIHelper.SetLabelText(Label_SliderValue, Mathf.RoundToInt(value * 100f));
			SetOptionValue(value, null, callBack: true);
		}
	}

	private void Reset()
	{
	}

	void InitOnCreate.OnCreate()
	{
		Reset();
	}

	void InitOnCreate.OnRelease()
	{
		Reset();
	}
}
