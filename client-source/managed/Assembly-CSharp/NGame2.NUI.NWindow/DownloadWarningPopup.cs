using NGameAsset;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class DownloadWarningPopup : BaseWindow
{
	public interface Listener
	{
		void OnShowDownloadSelectPopup();

		void OnConfirmAndStartDownload(ESpecType specType);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Login/DownloadWarningPopup";

	public GameObject Label_LowSpec;

	public GameObject Label_MidSpec;

	private ESpecType specType;

	private Listener _listener;

	public void Open(Listener listener, ESpecType specType)
	{
		base.Open();
		_listener = listener;
		this.specType = specType;
		UIHelper.EnableGameObject(Label_LowSpec, enable: false);
		UIHelper.EnableGameObject(Label_MidSpec, enable: false);
		if (this.specType == ESpecType.Low)
		{
			UIHelper.EnableGameObject(Label_LowSpec, enable: true);
		}
		else
		{
			UIHelper.EnableGameObject(Label_MidSpec, enable: true);
		}
	}

	public void OnClickYes()
	{
		if (_listener != null)
		{
			Close();
			_listener.OnConfirmAndStartDownload(specType);
		}
	}

	public void OnClickNo()
	{
		if (_listener != null)
		{
			Close();
			_listener.OnShowDownloadSelectPopup();
		}
	}
}
