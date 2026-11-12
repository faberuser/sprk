namespace NGame2.NUI.NWindow;

public class LoginUseCouponPopup : BaseWindow
{
	public interface Listener
	{
		void OnUseCoupon(LoginUseCouponPopup caller, string couponCode);

		void OnClickBack(LoginUseCouponPopup caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginUseCouponPopup";

	public UIInput CodeInput;

	public UIButton UseButton;

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
		UpdateState();
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
	}

	public void OnUseCoupon()
	{
		if (!string.IsNullOrEmpty(CodeInput.value) && _listener != null)
		{
			_listener.OnUseCoupon(this, CodeInput.value);
		}
	}

	public void OnClickBack()
	{
		if (_listener != null)
		{
			_listener.OnClickBack(this);
		}
	}

	private void UpdateState()
	{
		UIHelper.EnableButton(UseButton, !string.IsNullOrEmpty(CodeInput.value));
	}

	public void OnSubmit()
	{
		UpdateState();
	}

	public void OnChange()
	{
		UpdateState();
	}

	public override void OnSystemBackButton()
	{
		OnClickBack();
	}
}
