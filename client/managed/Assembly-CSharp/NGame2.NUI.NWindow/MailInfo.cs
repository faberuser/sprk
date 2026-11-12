using NGame2.NScene.NPreloader;
using NShared;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class MailInfo : BaseWindow
{
	public interface Listener
	{
		bool OnCloseRequest(MailInfo caller);

		void OnReceive(MailInfo caller);

		void OnDelete(MailInfo caller);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Mail/MailInfoNormal";

	public UILabel ContentLabel;

	public UILabel TitleLabel;

	public UILabel SenderNameLabel;

	public UILabel DotLabel;

	private Listener _listener;

	public NShared.MailInfo Info { get; private set; }

	public void Open(NShared.MailInfo mailInfo, bool isNew, Listener listener)
	{
		Open(delegate
		{
			Init(mailInfo, isNew, listener);
		}, null);
	}

	public virtual void Init(NShared.MailInfo mailInfo, bool isNew, Listener listener)
	{
		Info = mailInfo;
		_listener = listener;
		UIHelper.EnableGameObject(ContentLabel, enable: true);
		UIHelper.SetLabelText(ContentLabel, LocalizationManager.LocalizeCode(mailInfo.Content));
		UIHelper.SetLabelText(SenderNameLabel, LocalizationManager.LocalizeCode(mailInfo.SenderName));
		string text = LocalizationManager.LocalizeCode(mailInfo.Title);
		UIHelper.SetLabelText(TitleLabel, text);
		DotLabel.gameObject.SetActive(TitleLabel.CalculateOffsetToFit(text) > 0);
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		_listener = null;
		UIHelper.EnableGameObject(ContentLabel, enable: false);
	}

	private static void UpdateAttachedCurrency(GameObject currencyObject, UILabel currencyLabel, int currency, ref int curPositionX)
	{
		if (currency > 0)
		{
			UIHelper.EnableGameObject(currencyObject, enable: true);
			UIHelper.SetLabelText(currencyLabel, Vespa_Int_Extension_Method.ToCommaString(currency));
			currencyObject.transform.localPosition = new Vector3(curPositionX, currencyObject.transform.localPosition.y, 0f);
			UIWidget component = currencyObject.GetComponent<UIWidget>();
			curPositionX += (component ? ((int)component.localSize.x) : 0);
		}
		else
		{
			UIHelper.EnableGameObject(currencyObject, enable: false);
		}
	}

	public void OnClickClose()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public void OnClickReceive()
	{
		if (_listener != null)
		{
			_listener.OnReceive(this);
		}
	}

	public void OnClickDelete()
	{
		if (_listener != null)
		{
			_listener.OnDelete(this);
		}
	}
}
