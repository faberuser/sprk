using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NGameSetting;
using NGame2.NGuild;
using NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar.NConstructor;
using NGame2.NUtil;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout.NPcAvatar;

public class PcAvatarManager : IAvatarContainer
{
	public class PcWrapper
	{
		public BasePcAvatar PcAvatar;

		public PcAvatarController pcAvatarController;
	}

	private List<PcWrapper> _pcList = new List<PcWrapper>();

	private Mediator _mediator;

	private void CreateAvaterFromStock(PcAvatarData avatarData, PcAvatarStock stock)
	{
		if (stock.IsValid())
		{
			BasePcMediator mediator = stock.Mediator;
			BasePcAvatar avatar = stock.Avatar;
			RenderFacadeBuilder renderFacadeBulider = stock.RenderFacadeBulider;
			long uid = stock.Uid;
			string name = stock.Name;
			PcAvatarController pcAvatarController = new PcAvatarController(mediator);
			mediator.Avatar = avatar;
			mediator.AvatarController = pcAvatarController;
			mediator.MapComponent = _mediator.MapComponent;
			RenderFacadeBuilder renderFacadeBuilder = renderFacadeBulider;
			avatar.SetRenderFacadeLoadLevel(CreatureStock.ELoadLevel.PcAvatar);
			avatar.Open(avatarData, mediator, renderFacadeBuilder);
			avatar.SetPcAvatarInfo(uid, name);
			avatar.SetInteractionComponent(avatarData);
			avatar.SetInteractionComponentName(name);
			PcWrapper item = new PcWrapper
			{
				PcAvatar = avatar,
				pcAvatarController = pcAvatarController
			};
			_pcList.Add(item);
		}
	}

	public void Open(Mediator mediator)
	{
		_mediator = mediator;
	}

	public void OnLoadMap()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		IList<PcAvatarData> datasByChapter = GameRoot.Instance.PcAvatarDataContainer.GetDatasByChapter(chapterIndex);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(datasByChapter); i++)
		{
			PcAvatarData pcAvatarData = datasByChapter[i];
			if (!IsAreadyCreated(pcAvatarData) && CheckCondition1(pcAvatarData.Condition1))
			{
				IPcAvatarFactory constructor = SingletonAuto<AvatarConstructorFactoryContainer>.instance.GetConstructor(pcAvatarData.ConstructorFactory);
				if (constructor != null)
				{
					CreateAvaterFromStock(pcAvatarData, constructor.CreateAvatar(pcAvatarData.FactoryValues));
				}
			}
		}
	}

	private void TestInitGuildMember()
	{
		List<GuildMemberInfo> list = new List<GuildMemberInfo>();
		for (int i = 1; i < 9; i++)
		{
			list.Add(CreateGuildMemeber(i));
		}
		GlobalAutoBehaviour<GuildManager>.instance.InitGuildMembers(list.ToArray());
	}

	private GuildMemberInfo CreateGuildMemeber(int heroIndex)
	{
		GuildMemberInfo obj = new GuildMemberInfo
		{
			AvatarHeroIndex = heroIndex * 10,
			HeroIndex = heroIndex,
			AccountId = 1000000 + heroIndex
		};
		obj.Nick = obj.AccountId.ToString();
		obj.IsOnline = true;
		return obj;
	}

	private bool IsAreadyCreated(PcAvatarData data)
	{
		if (data == null)
		{
			return true;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_pcList); i++)
		{
			if (data.Index == _pcList[i].PcAvatar.Index)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckCondition1(string[] values)
	{
		return SingletonAuto<ConditionChecker>.instance.Check(values);
	}

	public void UnLoadMap()
	{
		if (__Extension_Method_ICollection__.COUNT(_pcList) > 0)
		{
			_pcList.ForEach(delegate(PcWrapper e)
			{
				e.PcAvatar.Closed();
			});
			_pcList.Clear();
		}
		SingletonAuto<AvatarConstructorFactoryContainer>.instance.ClearCache();
	}

	public void ChainUpdate()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_pcList); i++)
		{
			_pcList[i].PcAvatar.ChainUpdate();
			_pcList[i].pcAvatarController.ChainUpdate();
		}
	}

	void IAvatarContainer.ForEach(Action<Avatar> action)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_pcList); i++)
		{
			Vespa_System_Action_Extension_Method.fire(action, _pcList[i].PcAvatar);
		}
	}

	public ITask AvatarShowTask()
	{
		EntryMenuData entryMenu = GameSetting.data.EntryMenu;
		SequenceTask sequenceTask = new SequenceTask();
		sequenceTask.AddTask(coAvatarShowOrHide(isShow: true, entryMenu.BlendTimeLength));
		return sequenceTask;
	}

	public ITask PcAvatarHideTask(EntryMenuSettingData menuAppearance)
	{
		SequenceTask sequenceTask = new SequenceTask();
		sequenceTask.AddTask(coAvatarShowOrHide(isShow: false, menuAppearance.NpcSlideTimeLength));
		return sequenceTask;
	}

	private IEnumerator coAvatarShowOrHide(bool isShow, float duration)
	{
		_pcList.ForEach(delegate(PcWrapper pc)
		{
			pc.PcAvatar.AvatarShowOrHide(isShow);
		});
		if (duration == 0f)
		{
			float result = (isShow ? 1f : 0f);
			_pcList.ForEach(delegate(PcWrapper pc)
			{
				pc.PcAvatar.SetExtraAlpha(result);
			});
			yield break;
		}
		float elapsedTime = 0f;
		do
		{
			yield return null;
			elapsedTime += Time.deltaTime;
			float num = Mathf.Clamp01(elapsedTime / duration);
			float offset = (isShow ? num : (1f - num));
			_pcList.ForEach(delegate(PcWrapper pc)
			{
				pc.PcAvatar.SetExtraAlpha(offset);
				pc.PcAvatar.AiInteractionComponent.SetExtraAlpha(offset);
			});
		}
		while (!(elapsedTime >= duration));
	}
}
