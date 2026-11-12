using NVespa.NPrefab2;

namespace NGame2.NUI;

public class ButtonListComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickButton(string key);
	}

	private string _key;

	private IListener _listener;

	void InitOnCreate.OnCreate()
	{
		SetOpen(isOpen: false, isInit: true);
	}

	public virtual void OnRelease()
	{
		SetOpen(isOpen: false, isInit: true);
	}

	public void Init(IListener listener, string key)
	{
		_key = key;
		_listener = listener;
	}

	public void OnClickButton()
	{
		if (_listener != null)
		{
			_listener.OnClickButton(_key);
		}
	}

	public virtual void SetOpen(bool isOpen, bool isInit = false)
	{
	}

	public void SetPosition(float y)
	{
		base.transform.localPosition = Vespa_Unity_Vector3_Extension_Method.ReplaceY(base.transform.localPosition, y);
	}
}
