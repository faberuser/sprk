using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NGame2.NAccount;
using NGame2.NNotification;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NGameAsset;
using NShared;
using NShared.SetPushNotification;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GameOptionBaseComponent : LinkFactoryComponent
{
	[Serializable]
	public class GameOptionRootInfo
	{
		public GameObject Root;

		public UILabel Label_OptionName;

		public GameObject ToolTipImage;
	}

	public interface Listener
	{
		void SetChildsValue(int[] childIndices, object value, bool childState, GameOptionValueInfo valueInfo);

		void CheckAllChildValue(int parentIndex, object value, GameOptionValueInfo valueInfo);
	}

	public const int toggleGroupIndex = 100000;

	public NShared.GameOptionData gameOptionData;

	public Collider[] Collider_Tooltip;

	public GameOptionRootInfo ParentRoot;

	public GameOptionRootInfo ChildRoot;

	private bool isParent = true;

	private bool enableUI = true;

	private int[] childIndices;

	private object currentValue;

	public Type dataType;

	private bool isPostProcessRunning;

	public Listener _listener;

	public object CurrentValue
	{
		get
		{
			return currentValue;
		}
		set
		{
			currentValue = value;
		}
	}

	public virtual void Refresh()
	{
	}

	public virtual GameOptionValueInfo GetCurrentOptionValueInfo()
	{
		return null;
	}

	public virtual GameOptionValueInfo GetDisableGroupOptionValueInfo(int groupIndex)
	{
		return null;
	}

	public virtual object GetObjectValue(GameOptionValueInfo valueInfo)
	{
		return null;
	}

	public bool IsParent()
	{
		return isParent;
	}

	public bool GetChildState()
	{
		return enableUI;
	}

	public int GetIndex()
	{
		if (gameOptionData == null)
		{
			return 0;
		}
		return gameOptionData.Index;
	}

	public void InitChildIndices()
	{
		if (gameOptionData != null && isParent)
		{
			IList<int> childList = GameRoot.Instance.GameOptionDataContainer.GetChildList(gameOptionData.Index);
			if (childList != null)
			{
				childIndices = childList.ToArray();
			}
		}
	}

	public int[] GetChildIndices()
	{
		return childIndices;
	}

	public void SetUIState(bool state)
	{
		enableUI = state;
	}

	public void SetType()
	{
		if (gameOptionData != null)
		{
			dataType = Type.GetType(gameOptionData.OptionDataType);
		}
	}

	public void SetUIRoot()
	{
		if (gameOptionData == null)
		{
			SprkGraphicsOptions.ConfigureGraphicsHelp(this);
			return;
		}
		if (gameOptionData.ChildRoot)
		{
			isParent = false;
		}
		if (ParentRoot == null || ChildRoot == null)
		{
			SprkGraphicsOptions.ConfigureGraphicsHelp(this);
			return;
		}
		if (ParentRoot.Root != null)
		{
			UIHelper.EnableGameObject(ParentRoot.Root, isParent);
		}
		if (ChildRoot.Root != null)
		{
			UIHelper.EnableGameObject(ChildRoot.Root, !isParent);
		}
		if (ParentRoot.ToolTipImage != null)
		{
			UIHelper.EnableGameObject(ParentRoot.ToolTipImage, !String_Extension_Method.IsNullOrEmpty(gameOptionData.ToolTipDesc));
		}
		if (ChildRoot.ToolTipImage != null)
		{
			UIHelper.EnableGameObject(ChildRoot.ToolTipImage, !String_Extension_Method.IsNullOrEmpty(gameOptionData.ToolTipDesc));
		}
		InitChildIndices();
		SprkGraphicsOptions.ConfigureGraphicsHelp(this);
	}

	public void SetChildValue(object value, GameOptionValueInfo valueInfo)
	{
		_listener.SetChildsValue(childIndices, value, enableUI, valueInfo);
	}

	public void SetOptionValue(object value, GameOptionValueInfo valueInfo, bool callBack)
	{
		SetOptionValue(gameOptionData.ClassName, gameOptionData.FieldName, value, valueInfo, callBack);
	}

	public void SetOptionValue(string className, string fieldName, object value, GameOptionValueInfo valueInfo, bool callBack = true)
	{
		if (gameOptionData.SaveValue)
		{
			if (Type.GetType(className) == null)
			{
				SprkTestPrefs.Save();
				return;
			}
			FieldInfo field = Type.GetType(className).GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
			if (!(field != null))
			{
				goto IL_0057;
			}
			try
			{
				field.SetValue(field, value);
			}
			catch (Exception obj)
			{
				VespaDebug.LogError(obj);
				goto IL_0057;
			}
		}
		goto IL_011b;
		IL_011b:
		SprkTestPrefs.Save();
		return;
		IL_0057:
		MemberInfo[] member = Type.GetType(className).GetMember(fieldName, BindingFlags.Static | BindingFlags.Public);
		if (member != null && __Extension_Method_ICollection__.COUNT(member) > 0)
		{
			CurrentValue = value;
			SetMemberValue(member[0], value);
			RunPostProcessFunction(value);
			if (isParent && childIndices != null && __Extension_Method_ICollection__.COUNT(childIndices) > 0 && _listener != null && callBack)
			{
				_listener.SetChildsValue(childIndices, value, enableUI, valueInfo);
			}
			if (gameOptionData.ParentNum > 0 && valueInfo != null && valueInfo.DisableGroup != null && __Extension_Method_ICollection__.COUNT(valueInfo.DisableGroup) > 0 && callBack)
			{
				_listener.CheckAllChildValue(gameOptionData.ParentNum, value, valueInfo);
			}
		}
		goto IL_011b;
	}

	public virtual void SetValueByChilds(object value, GameOptionValueInfo valueInfo)
	{
	}

	public virtual void SetValueByParent(object value, GameOptionValueInfo valueInfo)
	{
	}

	public virtual void SetGrayScale(bool enable)
	{
		UIHelper.SetWidgetGrayScale(base.gameObject.GetComponent<UIWidget>(), !enable, setColliders: true);
		if (Collider_Tooltip != null && __Extension_Method_ICollection__.COUNT(Collider_Tooltip) > 0)
		{
			Collider[] collider_Tooltip = Collider_Tooltip;
			for (int i = 0; i < collider_Tooltip.Length; i++)
			{
				collider_Tooltip[i].enabled = true;
			}
		}
	}

	public void SetMemberValue(MemberInfo member, object value)
	{
		switch (member.MemberType)
		{
		case MemberTypes.Field:
			try
			{
				((FieldInfo)member).SetValue(member, value);
				break;
			}
			catch (Exception obj2)
			{
				VespaDebug.LogError(obj2);
				break;
			}
		case MemberTypes.Property:
			try
			{
				((PropertyInfo)member).SetValue(member, value, null);
				break;
			}
			catch (Exception obj)
			{
				VespaDebug.LogError(obj);
				break;
			}
		default:
			throw new ArgumentException("MemberInfo is not of type FieldInfo or PropertyInfo", "member");
		}
	}

	public T GetGameOptionValue<T>(string value)
	{
		Type type = default(T).GetType();
		return (T)Convert.ChangeType(value, type);
	}

	public object GetOptionValue(string className, string fieldName)
	{
		if (String_Extension_Method.IsNullOrEmpty(className))
		{
			return null;
		}
		if (Type.GetType(className) == null)
		{
			return null;
		}
		FieldInfo field = Type.GetType(className).GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
		if (field != null)
		{
			return field.GetValue(null);
		}
		MemberInfo[] member = Type.GetType(className).GetMember(fieldName, BindingFlags.Static | BindingFlags.Public);
		if (member != null && __Extension_Method_ICollection__.COUNT(member) > 0)
		{
			return GetMemberValue(member[0], member[0]);
		}
		return null;
	}

	public object GetMemberValue(MemberInfo member, object target)
	{
		switch (member.MemberType)
		{
		case MemberTypes.Field:
			return ((FieldInfo)member).GetValue(target);
		case MemberTypes.Property:
			try
			{
				return ((PropertyInfo)member).GetValue(target, null);
			}
			catch (TargetParameterCountException innerException)
			{
				throw new ArgumentException("MemberInfo has index parameters", "member", innerException);
			}
		default:
			throw new ArgumentException("MemberInfo is not of type FieldInfo or PropertyInfo", "member");
		}
	}

	public void RunPostProcessFunction(object value)
	{
		if (!String_Extension_Method.IsNullOrEmpty(gameOptionData.PostProcessFunction) && !isPostProcessRunning)
		{
			isPostProcessRunning = true;
			VespaCoroutine.Start(coRunPostProcessFunction(value), delegate
			{
				isPostProcessRunning = false;
			});
		}
	}

	public IEnumerator coRunPostProcessFunction(object value)
	{
		MethodInfo method = GetType().GetMethod(gameOptionData.PostProcessFunction);
		object[] parameters = new object[1] { value };
		method.Invoke(this, parameters);
		yield return null;
	}

	public void OpenSkipRubyCountPopup(object value)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.OpenSkipCurrencyConfirmCountPopup(value);
	}

	public void ManageBlockedUser(object value)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.OpenManageBlockedUser();
	}

	public void SetTextureQuality(object value)
	{
		if (!SprkLoginSettings.SetTextureQuality(this, value))
		{
			object obj = Enum.Parse(dataType, value.ToString());
			if (obj != null)
			{
				SceneAutoBehaviour<LobbyManagement>.instance.SetTextureQuality((ESpecType)obj, gameOptionData.Index);
			}
		}
	}

	public void SetPlayBGM(object value)
	{
		if (!SprkLoginSettings.SetPlayBGM(this, value))
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
			if (data != null && !String_Extension_Method.IsNullOrEmpty(data.BgmCode))
			{
				SingletonAuto<BgmManager2>.instance.Request(data.BgmCode, isEvent: false);
			}
		}
	}

	public void OnToggleAllNoti(object value)
	{
		VespaCoroutine.Start(ShowCenterMessageAllNoti(value));
	}

	private IEnumerator ShowCenterMessageAllNoti(object value)
	{
		if (value.GetType() != typeof(bool))
		{
			yield return null;
		}
		yield return new WaitForSeconds(0.15f);
		DateTime now = DateTime.Now;
		_ = string.Empty;
		string message = ((!(bool)value) ? string.Format("{0}\n{1}", LocalizationManager.GetStringDirect("PUSH_DISAGREE_MESSAGE", now.Year, now.Month, now.Day), LocalizationManager.GetStringDirect("PUSH_NIGHT_DISAGREE_MESSAGE", now.Year, now.Month, now.Day)) : string.Format("{0}\n{1}", LocalizationManager.GetStringDirect("PUSH_AGREE_MESSAGE", now.Year, now.Month, now.Day), LocalizationManager.GetStringDirect("PUSH_NIGHT_AGREE_MESSAGE", now.Year, now.Month, now.Day)));
		CenterMessage.Open(message);
		yield return null;
	}

	public void RequestGuildSuppressNotificationValue(object value)
	{
		if (!(value.GetType() != typeof(bool)))
		{
			SingletonAuto<NotificationManager>.instance.RequestNotificationValue(PushNotificationType.GuildSuppress, (bool)value);
		}
	}

	public void RequestGuildRaidNotificationValue(object value)
	{
		if (!(value.GetType() != typeof(bool)))
		{
			SingletonAuto<NotificationManager>.instance.RequestNotificationValue(PushNotificationType.GuildRaid, (bool)value);
		}
	}

	public void RequestEventNotificationValue(object value)
	{
		if (!(value.GetType() != typeof(bool)))
		{
			if ((bool)value)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_AGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			}
			else
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_DISAGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			}
			SingletonAuto<NotificationManager>.instance.RequestNotificationValue(PushNotificationType.Event, (bool)value);
		}
	}

	public void RequestNightNotificationValue(object value)
	{
		if (!(value.GetType() != typeof(bool)))
		{
			if ((bool)value)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_NIGHT_AGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			}
			else
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("PUSH_NIGHT_DISAGREE_MESSAGE", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
			}
			SingletonAuto<NotificationManager>.instance.RequestNotificationValue(PushNotificationType.Night, (bool)value);
		}
	}

	public void OnShowTooltip()
	{
		if (!SprkGraphicsOptions.ShowGraphicsHelp(this) && !String_Extension_Method.IsNullOrEmpty(gameOptionData.ToolTipDesc))
		{
			GameOptionTooltip.Open(base.gameObject.GetComponent<UIWidget>(), new TooltipTextInfo
			{
				Desc = new StringProxy
				{
					Value = LocalizationManager.GetStringDirect(gameOptionData.ToolTipDesc)
				},
				Name = new StringProxy
				{
					Value = LocalizationManager.GetStringDirect(gameOptionData.OptionName)
				}
			});
		}
	}

	public void OnHideTooltip()
	{
		if (!SprkGraphicsOptions.HideGraphicsHelp(this) && !String_Extension_Method.IsNullOrEmpty(gameOptionData.ToolTipDesc))
		{
			GameOptionTooltip.Hide(base.gameObject.GetComponent<UIWidget>());
		}
	}
}
