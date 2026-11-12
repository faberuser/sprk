using System;
using System.Collections.Generic;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class HeroNotifyComponent : LinkFactoryComponent
{
	public enum ENotifyInfo
	{
		TrialChallengable,
		Purifiable,
		CanSkillExtend,
		CanEquipChange,
		CanLimitBreak
	}

	public interface Listener
	{
		void OnClickComponent(ENotifyInfo notifyInfo);
	}

	[Serializable]
	public class NotifyInfo
	{
		public ENotifyInfo Notifyinfo;

		public UIWidget WidgetNotify;
	}

	public const string PrefabPath = "Prefab2/Component/Hero/HeroNotifyComponent";

	public static PrefabAuto<HeroNotifyComponent> PrefabAuto = new PrefabAuto<HeroNotifyComponent>("Prefab2/Component/Hero/HeroNotifyComponent");

	public const string PrefabSmallPath = "Prefab2/Component/Hero/HeroNotifyComponentSmall";

	public static PrefabAuto<HeroNotifyComponent> PrefabSmallAuto = new PrefabAuto<HeroNotifyComponent>("Prefab2/Component/Hero/HeroNotifyComponentSmall");

	public List<NotifyInfo> ListNotifyInfos = new List<NotifyInfo>();

	private Listener _listener;

	public ENotifyInfo DisplayNotifyInfo { get; private set; }

	public void Open(Listener listener, ENotifyInfo notifyInfo)
	{
		_listener = listener;
		DisplayNotifyInfo = notifyInfo;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ListNotifyInfos); i++)
		{
			UIHelper.EnableGameObject(ListNotifyInfos[i].WidgetNotify, ListNotifyInfos[i].Notifyinfo == notifyInfo);
		}
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClickComponent(DisplayNotifyInfo);
		}
	}
}
