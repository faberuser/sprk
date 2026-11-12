namespace NGame2.NUI.NWindow;

public class LoginCouponSelect : BaseWindow
{
	public enum EventType
	{
		Default,
		PreregisterCoupon
	}

	public interface Listener
	{
		void OnSelect(LoginCouponSelect caller, EventType type);

		void OnClickBack(LoginCouponSelect caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginCouponSelect";

	private Listener _listener;

	public void Open(Listener listener)
	{
		base.Open();
		_listener = listener;
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
	}

	public void OnClickDefault()
	{
		if (_listener != null)
		{
			_listener.OnSelect(this, EventType.Default);
		}
	}

	public void OnClickPreregisterCoupon()
	{
		if (_listener != null)
		{
			_listener.OnSelect(this, EventType.PreregisterCoupon);
		}
	}
}
