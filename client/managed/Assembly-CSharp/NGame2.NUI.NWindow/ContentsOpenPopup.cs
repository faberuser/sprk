using System;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ContentsOpenPopup : BaseWindow, IInspectorGuiDrawer
{
	[Serializable]
	public class IconData
	{
		public string nodeCode;

		public UIWidget IconWidget;

		public LocalizeLinkData LocalizeLink = new LocalizeLinkData();

		public bool AddOpenText;
	}

	[Serializable]
	public class HellModeData
	{
		public int ChapterIndex;

		public string nodeCode;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Achievement/ContentsOpenPopup";

	public UILabel TextLabel;

	[HideInInspector]
	public List<IconData> IconList = new List<IconData>();

	[HideInInspector]
	public List<HellModeData> HellModeList = new List<HellModeData>();

	public float OpenDuration;

	private float _openedTime;

	private Action _closeAction;

	public void Open(CampaignNodeData openNode)
	{
		if (openNode != null)
		{
			string nodeCode = ((__Extension_Method_ICollection__.COUNT(openNode.Code) > 0) ? openNode.Code[0] : string.Empty);
			Open(nodeCode);
		}
	}

	public void Open(string nodeCode, Action closeAction = null)
	{
		Open(delegate
		{
			InitInternal(nodeCode);
		}, null);
		_closeAction = closeAction;
	}

	public void Open(string openTextResorce, string localize, Action closeAction = null)
	{
		base.Open();
		_closeAction = closeAction;
		InitInternal(openTextResorce);
		SetMenuTitle(localize);
	}

	public void OpenHellMode(List<CampaignChapterData> chapterList)
	{
		int chapterIndex = 0;
		chapterList.ForEach(delegate(CampaignChapterData data)
		{
			chapterIndex = Mathf.Max(chapterIndex, data.Index);
		});
		HellModeData hellmodeData = HellModeList.Find((HellModeData data) => data.ChapterIndex == chapterIndex);
		if (hellmodeData != null)
		{
			Open(delegate
			{
				InitInternal(hellmodeData.nodeCode);
			}, null);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (IsOpen() && Time.time - _openedTime > OpenDuration)
		{
			Close();
			if (_closeAction != null)
			{
				Vespa_System_Action_Extension_Method.fire(_closeAction);
				_closeAction = null;
			}
		}
	}

	private void InitInternal(string nodeCode)
	{
		_openedTime = Time.time;
		string menuTitle = "";
		for (int i = 0; i < IconList.Count; i++)
		{
			IconData iconData = IconList[i];
			bool flag = string.Equals(iconData.nodeCode, nodeCode);
			UIHelper.EnableGameObject(iconData.IconWidget, flag);
			if (flag)
			{
				string stringDirect = LocalizationManager.GetStringDirect(iconData.LocalizeLink.Key);
				menuTitle = ((!iconData.AddOpenText) ? stringDirect : LocalizationManager.GetStringDirect("NOTI_CONTENTS", stringDirect));
			}
		}
		SetMenuTitle(menuTitle);
	}

	private void SetMenuTitle(string localize)
	{
		if (!String_Extension_Method.IsNullOrEmpty(localize))
		{
			UIHelper.SetLabelText(TextLabel, localize);
		}
	}
}
