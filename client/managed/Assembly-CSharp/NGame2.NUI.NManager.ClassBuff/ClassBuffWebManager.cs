using System;
using NGame2.NAccount;
using NGame2.NTutorial;
using NGame2.NUI.NWindow;
using NShared;
using NShared.GetClassBuffInfo;
using NShared.InitClassBuff;
using NShared.LevelUpClassBuff;
using NVespa;
using NVespa.NSingleton;
using NVespa.NWeb;

namespace NGame2.NUI.NManager.ClassBuff;

public class ClassBuffWebManager
{
	private ClassBuffManagement _management;

	public void Init(ClassBuffManagement management)
	{
		_management = management;
	}

	public void RequestGetClassBuffInfo(Action responseComplete)
	{
		if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return;
		}
		NShared.GetClassBuffInfo.Request request = new NShared.GetClassBuffInfo.Request();
		SingletonUpdater<WebServiceRequester>.instance.Request(request, delegate(NShared.GetClassBuffInfo.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetClassBuffInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager.Apply(response.ClassBuffInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager.Apply(response.ClassBuffPointInfos);
				if (responseComplete != null)
				{
					Vespa_System_Action_Extension_Method.fire(responseComplete);
				}
			}
		});
	}

	public void RequestLevelUpClassBuff(ClassBuffData data, Action responseComplete)
	{
		NShared.LevelUpClassBuff.Request request = new NShared.LevelUpClassBuff.Request
		{
			TagType = (int)data.TagType,
			ClassBuffIndex = data.ClassBuffIndex,
			ClassBuffLevel = (byte)data.ClassBuffLevel
		};
		SingletonUpdater<WebServiceRequester>.instance.Request(request, delegate(NShared.LevelUpClassBuff.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.LevelUpClassBuff.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager.Apply(response.ClassBuffInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager.Apply(response.ClassBuffPointInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager.ApplyPrevPoint();
				if (responseComplete != null)
				{
					Vespa_System_Action_Extension_Method.fire(responseComplete);
				}
			}
		});
	}

	public void RequestResetClassBuff(CreatureTagType creatureType, Action responseComplete, Action<NShared.InitClassBuff.ResultType> responseError)
	{
		NShared.InitClassBuff.Request request = new NShared.InitClassBuff.Request
		{
			TagType = (int)creatureType
		};
		SingletonUpdater<WebServiceRequester>.instance.Request(request, delegate(NShared.InitClassBuff.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.InitClassBuff.ResultType.Success)
			{
				if (responseError != null)
				{
					Vespa_System_Action_Extension_Method.fire(responseError, response.Result);
				}
				else
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResult);
				GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager.Apply(response.ClassBuffInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager.Apply(response.ClassBuffPointInfos);
				GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager.ApplyPrevPoint();
				if (responseComplete != null)
				{
					Vespa_System_Action_Extension_Method.fire(responseComplete);
				}
			}
		});
	}
}
