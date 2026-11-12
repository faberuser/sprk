using System;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class SkipCurrencyConfirmCountPopup : BaseWindow
{
	public interface Listener
	{
		void SetSkipCurrencyCount(int currencyCount);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GameOption/SkipCurrencyConfirmCountPopup";

	public UILabel CurrencyCountLabel;

	public UIWidget CurrencyIconWidget;

	public UIInput CountInput;

	public UIButton YesButton;

	private IconProxy _iconProxy;

	private int _curVal;

	private int _maxVal = 100000;

	private Listener _listener;

	private Action _onClose;

	public void Open(Listener listener, object value = null)
	{
		base.Open();
		_listener = listener;
		if (value.GetType() == typeof(Action))
		{
			_onClose = (Action)value;
		}
		Init();
	}

	private void Init()
	{
		_maxVal = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxSkipPopupRuby", _maxVal);
		UIHelper.SetLabelText(CurrencyCountLabel, _maxVal);
		if (_iconProxy == null)
		{
			_iconProxy = IconProxy.Create(CurrencyIconWidget);
		}
		_iconProxy.SetupCurrencyNoBg(CurrencyType.Gem);
		_curVal = SystemOptionData.RubyPopup_Skip_Count;
		CountInput.value = _curVal.ToString();
		UpdateState();
	}

	public void UpdateState()
	{
		bool flag = string.IsNullOrEmpty(CountInput.value);
		object.Equals(CountInput.value, _curVal);
		int num = Vespa_String_Extension_Method.TOINT(CountInput.value, 0);
		if (_maxVal < num)
		{
			CountInput.value = (_maxVal - 1).ToString();
		}
		UIHelper.EnableButton(YesButton, !flag, grayscaleIfDisable: true);
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.SafeReleaseComponent(ref _iconProxy);
		_onClose = null;
	}

	public override void Close()
	{
		Vespa_System_Action_Extension_Method.fire(_onClose);
		base.Close();
	}

	public void OnSubmit()
	{
		UpdateState();
	}

	public void OnChange()
	{
		UpdateState();
	}

	public void OnClickYes()
	{
		if (_listener != null && !string.IsNullOrEmpty(CountInput.value))
		{
			int skipCurrencyCount = int.Parse(CountInput.value);
			_listener.SetSkipCurrencyCount(skipCurrencyCount);
			Close();
		}
	}

	public void OnClickExitButton()
	{
		Close();
	}
}
