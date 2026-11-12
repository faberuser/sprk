using System.Diagnostics;
using NVespa;

namespace NGame2.NConfig;

public class ClientDebugEvent : SingletonAuto<ClientDebugEvent>
{
	public enum EEventType
	{
		CampaignStartBattle,
		OnWaveOpening,
		OnSceneLoaded
	}

	private LocalStorageBool _skipBossIntro = new LocalStorageBool(i_default: false, typeof(ClientDebugEvent).FullName + "_skipBossIntro");

	private IncreaseDic<EEventType, LocalStorageBool> _dic = new IncreaseDic<EEventType, LocalStorageBool>();

	public bool SkipBossIntro
	{
		get
		{
			return _skipBossIntro.Value;
		}
		set
		{
			_skipBossIntro.Value = value;
		}
	}

	public ClientDebugEvent()
	{
		EnumLookup<EEventType>.ValueData[] valueArray = EnumLookup<EEventType>.ValueArray;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(valueArray); i++)
		{
			EnumLookup<EEventType>.ValueData valueData = valueArray[i];
			_dic.Add(valueData.EnumValue, new LocalStorageBool(i_default: false, typeof(ClientDebugEvent).FullName + valueData.Name));
		}
	}

	[Conditional("UNITY_EDITOR")]
	public static void OnEvent(EEventType eventType)
	{
		_ = SingletonAuto<ClientDebugEvent>.instance._dic.GETVALUE(eventType, null)?.Value;
	}
}
