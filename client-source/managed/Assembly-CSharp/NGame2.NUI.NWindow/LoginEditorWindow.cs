using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class LoginEditorWindow : BaseWindow, IInspectorGuiDrawer
{
	public const string PrefabPath = "Resources/Prefab2/Window/Login/LoginEditorWindow";

	public UILabel ServerNameLabel;

	public UILabel ServerAddressLabel;

	public UILabel LoginButtonLabel;

	public UILabel PingButtonLabel;

	public string ForceLoginId;

	public int ForceAccountId;

	public string ForceVersion;

	public UIInput TestPrefixInput;

	public UILabel ForceLoginIdLabel;

	[HideInInspector]
	public List<string> ConfigNameList = new List<string>();

	[HideInInspector]
	public List<UILabel> TestAccountLabelList = new List<UILabel>();

	private LocalStorageString _testAccountPrefix = new LocalStorageString("test", "LoginMgr_TestAccountPrefix");
}
