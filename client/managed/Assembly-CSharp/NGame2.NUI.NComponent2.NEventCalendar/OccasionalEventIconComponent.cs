using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NEventCalendar;

public class OccasionalEventIconComponent : MonoBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/EventCalendar/OccasionalEventIconComponent";

	public static PrefabAuto<OccasionalEventIconComponent> PrefabAuto = new PrefabAuto<OccasionalEventIconComponent>("Resources/Prefab2/Component/EventCalendar/OccasionalEventIconComponent");

	public UIWidget NamedRoot;

	public UIWidget NamedColorWidget;

	public UIWidget EmptyRoot;

	public UIWidget EmptyColorWidget;

	public UILabel NameTagLabel;

	private bool _isNamed;

	public void Open(EventCalendarData data, EventCalendarLocalInfo localInfo, bool isTodayStarted)
	{
		string text = ((isTodayStarted && localInfo != null) ? localInfo.Nametag : null);
		bool flag = !String_Extension_Method.IsNullOrEmpty(text);
		Color color = NGUIText.ParseColor(data.Color.Replace("#", ""), 0);
		_isNamed = flag;
		UIHelper.EnableGameObject(NamedRoot, flag);
		UIHelper.EnableGameObject(EmptyRoot, !flag);
		UIHelper.SetWidgetColor(NamedColorWidget, color);
		UIHelper.SetWidgetColor(EmptyColorWidget, color);
		UIHelper.SetLabelText(NameTagLabel, text);
	}

	public int GetHeight()
	{
		if (_isNamed)
		{
			if (!NamedRoot)
			{
				return 0;
			}
			return NamedRoot.height;
		}
		if (!EmptyRoot)
		{
			return 0;
		}
		return EmptyRoot.height;
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}
}
