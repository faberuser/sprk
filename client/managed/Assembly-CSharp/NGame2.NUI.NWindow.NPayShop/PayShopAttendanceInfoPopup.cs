using NGame2.NContentsInfo;
using NShared;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopAttendanceInfoPopup : PayShopBaseInfoPopup
{
	public new interface IListener : PayShopBaseInfoPopup.IListener
	{
		void OnClickView(AttendanceKeywordGroup attendanceGroup);
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopAttendanceInfoPopup";

	[Header("PayShopAttendanceInfoPopup")]
	public UIScrollView ScrollView_Desc;

	public UILabel Label_Desc;

	public UIBundleTexSelector UIBundleTexSelector;

	private AttendanceKeywordGroup _attendanceGroup;

	private new IListener _listener;

	public override void Open(PayShopBaseInfoPopup.IListener listener, int index)
	{
		_listener = listener as IListener;
		base.Open(listener, index);
	}

	protected override void Init(NewPayShopProductInfo productInfo)
	{
		base.Init(productInfo);
		int index_MainAttendance = productInfo.SelectAttendanceIndices[0];
		_attendanceGroup = new AttendanceKeywordGroup(index_MainAttendance);
		UIBundleTexSelector.Select(_attendanceGroup.KeywordMain.ToString());
		string stringDirect = LocalizationManager.GetStringDirect(productInfo.ProductDetailDesc);
		UIHelper.SetLabelText(Label_Desc, stringDirect);
		if ((bool)ScrollView_Desc)
		{
			ScrollView_Desc.ResetPosition();
		}
	}

	public void OnClickView()
	{
		if (_listener != null)
		{
			_listener.OnClickView(_attendanceGroup);
		}
	}
}
