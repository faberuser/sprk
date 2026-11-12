using System;
using NNguiExtension;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ProgressComponent : UIScrollItemBehaviour
{
	public interface Listener
	{
		void OnInputValue(GuildConstructionResource resType, int value);
	}

	public const string PrefabPath = "Prefab2/Component/Guild/HeadQuarter/ProgressComponent";

	public static PrefabAuto<ProgressComponent> PrefabAuto = new PrefabAuto<ProgressComponent>("Prefab2/Component/Guild/HeadQuarter/ProgressComponent");

	public const int RAISE_PERCENT = 10;

	public UISpriteSelector Sprite_Selector;

	public UILabel Label_Name;

	public UISprite Sprite_Bar;

	public UISprite Sprita_AddBar;

	public UILabel Label_Amount;

	public UIInput Input_AddValue;

	public UILabel Label_AddValue;

	public UIButton Button_Plus;

	public UIButton Button_Minus;

	public UIButton Button_Max;

	private GuildConstructionResource _resType;

	private Listener _listener;

	private int _current;

	private int _max;

	private int _addValue;

	private bool _hasAuthority;

	public void Open(Listener listener, GuildConstructionResource resType, bool hasAuthority)
	{
		_listener = listener;
		_resType = resType;
		_hasAuthority = hasAuthority;
		SetButton();
		SelectIcon();
		SetResName();
		SetBar();
		SetAddBar();
	}

	private void SetButton()
	{
		bool hasAuthority = _hasAuthority;
		UIHelper.EnableButton(Button_Plus, hasAuthority, grayscaleIfDisable: true);
		UIHelper.EnableButton(Button_Minus, hasAuthority, grayscaleIfDisable: true);
		UIHelper.EnableButton(Button_Max, hasAuthority, grayscaleIfDisable: true);
		UIHelper.EnableCollider(Input_AddValue, hasAuthority);
	}

	private void SelectIcon()
	{
		Sprite_Selector.Select(_resType.ToString());
	}

	private void SetResName()
	{
		UIHelper.SetLabelText(Label_Name, _resType.ToString());
	}

	private void SetBar()
	{
		UIHelper.SetLabelText(Label_Amount, _current + "/" + _max);
		float fillAmount = (float)_current / (float)_max;
		UIHelper.SetSpriteFillAmount(Sprite_Bar, fillAmount);
	}

	private void SetAddBar()
	{
		SetInputText(Input_AddValue, _addValue);
		float fillAmount = (float)(_current + _addValue) / (float)_max;
		UIHelper.SetSpriteFillAmount(Sprita_AddBar, fillAmount);
	}

	private void SetInputText(UIInput uiInput, object value)
	{
		if (!(uiInput == null))
		{
			uiInput.value = ((value != null) ? value.ToString() : string.Empty);
		}
	}

	private bool invalidValue()
	{
		if (_max == 0 || _current > _max)
		{
			return true;
		}
		return false;
	}

	public void SetProgress(int current, int max, int addValue)
	{
		_current = current;
		_max = max;
		_addValue = addValue;
		if (_max == 0)
		{
			UIHelper.SetLabelText(Label_Amount, "-");
			UIHelper.SetSpriteFillAmount(Sprite_Bar, 0f);
			SetInputText(Input_AddValue, "-");
			UIHelper.SetSpriteFillAmount(Sprita_AddBar, 0f);
		}
		else
		{
			SetBar();
			SetAddBar();
		}
	}

	public void OnClickPlus()
	{
		if (_listener != null && !invalidValue())
		{
			int addValue = _addValue;
			int val = _max - _current;
			addValue += GetAddAmount();
			addValue = Math.Min(addValue, val);
			_listener.OnInputValue(_resType, addValue);
		}
	}

	public void OnClickMinus()
	{
		if (_listener != null && !invalidValue())
		{
			int addValue = _addValue;
			addValue -= GetAddAmount();
			addValue = Math.Max(addValue, 0);
			_listener.OnInputValue(_resType, addValue);
		}
	}

	private int GetAddAmount()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildBuildingInvestRaisePercent", 10);
		return (int)((float)_max * ((float)valueInt / 100f));
	}

	public void OnInputValue()
	{
		if (_listener != null && _hasAuthority && !invalidValue())
		{
			int num = 0;
			int max = _max - _current;
			int result = 0;
			if (int.TryParse(Input_AddValue.value, out result))
			{
				int value = Mathf.Clamp(result, num, max);
				_listener.OnInputValue(_resType, value);
			}
			else
			{
				_listener.OnInputValue(_resType, num);
			}
		}
	}

	public void OnClickMax()
	{
		if (_listener != null && !invalidValue())
		{
			int value = _max - _current;
			_listener.OnInputValue(_resType, value);
		}
	}
}
