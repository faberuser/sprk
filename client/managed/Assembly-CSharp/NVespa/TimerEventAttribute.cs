using System;
using System.Reflection;

namespace NVespa;

public class TimerEventAttribute : Attribute
{
	public enum EPauseType
	{
		None,
		Pause,
		Playing
	}

	internal static TimerEventAttribute _nullAttribute;

	internal static IncreaseDic<ETimerEventFlag, TimerEventAttribute> _dic;

	public bool IsLocal { get; set; }

	public EPauseType PauseType { get; set; }

	static TimerEventAttribute()
	{
		_nullAttribute = new TimerEventAttribute();
		_dic = new IncreaseDic<ETimerEventFlag, TimerEventAttribute>();
		Type typeFromHandle = typeof(ETimerEventFlag);
		EnumLookup<ETimerEventFlag>.ValueData[] valueArray = EnumLookup<ETimerEventFlag>.ValueArray;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(valueArray); i++)
		{
			EnumLookup<ETimerEventFlag>.ValueData valueData = valueArray[i];
			MemberInfo[] member = typeFromHandle.GetMember(valueData.Name);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(member))
			{
				object[] customAttributes = member[0].GetCustomAttributes(typeof(TimerEventAttribute), inherit: false);
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(customAttributes) && customAttributes[0] is TimerEventAttribute value)
				{
					_dic.Add(valueData.EnumValue, value);
				}
			}
		}
		_dic.SortByKey((ETimerEventFlag lhs, ETimerEventFlag rhs) => lhs.CompareTo(rhs));
	}

	public static TimerEventAttribute GetAttribute(ETimerEventFlag eventFlag)
	{
		return _dic.GETVALUE(eventFlag, _nullAttribute);
	}
}
