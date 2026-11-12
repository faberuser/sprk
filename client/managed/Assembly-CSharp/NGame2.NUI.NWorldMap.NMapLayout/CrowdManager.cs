using System;
using System.Collections.Generic;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NUI.NPet;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class CrowdManager : IAvatarContainer, IAiAvatarListener
{
	public class AvatarInfo
	{
		public int ChapterIndex { get; set; }

		public int DungeonIndex { get; set; }

		public int CostumeIndex { get; set; }

		public int CreatureIndex { get; set; }
	}

	private Mediator _myMediator;

	private CrowdMediator _mainPetAvatarMediator;

	private IncreaseDic<CrowdKey, CrowdMediator> _crowdDictionary = new IncreaseDic<CrowdKey, CrowdMediator>();

	private List<Action<CrowdCollisionInfo>> _collisionListenerList = new List<Action<CrowdCollisionInfo>>();

	private float _collisionRadius;

	private Vector2 DefaultPetDistance => new Vector2(-40f, -25f);

	private Vector2 DefaultDistance => new Vector2(-70f, 55f);

	public void Open(Mediator mediator, float collisionRadius)
	{
		_myMediator = mediator;
		_collisionRadius = collisionRadius;
	}

	public void AddCollisionListener(Action<CrowdCollisionInfo> onCollision)
	{
		_collisionListenerList.Add(onCollision);
	}

	public void RemoveCollisionListener(Action<CrowdCollisionInfo> onCollision)
	{
		_collisionListenerList.Remove(onCollision);
	}

	public void OnUnloadMap()
	{
		ClearCrowd();
	}

	public void OnLoadMap()
	{
		InitCrowd();
	}

	public void Close()
	{
		ClearCrowd();
		_myMediator = null;
		CloseMainPetAvatarMediator();
	}

	private void CloseMainPetAvatarMediator()
	{
		if (_mainPetAvatarMediator != null)
		{
			_mainPetAvatarMediator.Close();
			_mainPetAvatarMediator = null;
		}
	}

	public void ChainUpdate()
	{
		UpdateCrowd();
		UpdateCrowdCollision();
	}

	private void InitCrowd()
	{
		ClearCrowd();
		InitWantedCrowd();
	}

	private void ClearCrowd()
	{
		if (!_crowdDictionary.IsNullOrEmpty())
		{
			for (int i = 0; i < _crowdDictionary.Count; i++)
			{
				_crowdDictionary.GetValueByIndex(i).Close();
			}
			_crowdDictionary.Clear();
		}
	}

	private bool RemoveCrowd(CrowdKey key)
	{
		_crowdDictionary.TryGetValue(key, out var o_value);
		if (o_value != null)
		{
			_crowdDictionary.RemoveSlow(key);
			o_value.Close();
			return true;
		}
		return false;
	}

	public void RemovePartyMemberAvatars()
	{
		if (_crowdDictionary.IsNullOrEmpty())
		{
			return;
		}
		CrowdKey crowdKey = null;
		for (int i = 0; i < _crowdDictionary.Count; i++)
		{
			crowdKey = _crowdDictionary.GetKeyByIndex(i);
			if (crowdKey != null && crowdKey.Item1 == CrowdType.PartyMember)
			{
				_crowdDictionary.RemoveByIndex(i);
				i = 0;
			}
		}
	}

	private void RemoveMainPetAvatar()
	{
		if (_crowdDictionary.IsNullOrEmpty())
		{
			return;
		}
		CrowdKey crowdKey = null;
		for (int i = 0; i < _crowdDictionary.Count; i++)
		{
			crowdKey = _crowdDictionary.GetKeyByIndex(i);
			if (crowdKey != null && crowdKey.Item1 == CrowdType.Pet)
			{
				_crowdDictionary.RemoveByIndex(i);
				break;
			}
		}
	}

	private Avatar GetAvatar(CrowdType type, long id)
	{
		return GetCrowd(type, id)?.Avatar;
	}

	private CrowdMediator GetCrowd(CrowdType type, long id)
	{
		CrowdKey key = new CrowdKey(type, id);
		_crowdDictionary.TryGetValue(key, out var o_value);
		return o_value;
	}

	private void UpdateCrowd()
	{
		if (_crowdDictionary.IsNullOrEmpty())
		{
			return;
		}
		for (int i = 0; i < _crowdDictionary.Count; i++)
		{
			CrowdMediator valueByIndex = _crowdDictionary.GetValueByIndex(i);
			if (valueByIndex != null && valueByIndex.Avatar != null && valueByIndex.AvatarController != null)
			{
				valueByIndex.Avatar.ChainUpdate();
				valueByIndex.AvatarController.ChainUpdate();
			}
		}
	}

	private void UpdateCrowdCollision()
	{
		if (_crowdDictionary.IsNullOrEmpty())
		{
			return;
		}
		Vector3 localPosition = _myMediator.Avatar.GetLocalPosition();
		for (int i = 0; i < _crowdDictionary.Count; i++)
		{
			KeyValuePair<CrowdKey, CrowdMediator> keyValuePairByIndex = _crowdDictionary.GetKeyValuePairByIndex(i);
			CrowdKey key = keyValuePairByIndex.Key;
			CrowdMediator value = keyValuePairByIndex.Value;
			if (value == null || value.Avatar == null)
			{
				continue;
			}
			Vector3 localPosition2 = value.Avatar.GetLocalPosition();
			bool flag = Vector3.Distance(localPosition, localPosition2) <= _collisionRadius;
			if (flag != value.IsCollided)
			{
				value.IsCollided = flag;
				CrowdCollisionInfo collisionInfo = new CrowdCollisionInfo
				{
					Type = key.Item1,
					Id = key.Item2,
					Crowd = value
				};
				_collisionListenerList.ForEach(delegate(Action<CrowdCollisionInfo> e)
				{
					e(collisionInfo);
				});
			}
		}
	}

	private void InitWantedCrowd()
	{
	}

	public void CreatePartyMemberAvatar(AvatarInfo info)
	{
		if (info != null)
		{
			CrowdMediator crowdMediator = CreateAvatar(info, CrowdType.PartyMember);
			if (crowdMediator != null)
			{
				_myMediator.AvatarController.SetAiAvatarControllerListener(crowdMediator);
				crowdMediator.Avatar.SetForcedDirection(_myMediator.Avatar.CurrentDirection);
			}
		}
	}

	public void CreateCrowdAvatar(AvatarInfo info)
	{
		if (info != null)
		{
			CreateAvatar(info, CrowdType.Crowd);
		}
	}

	public void CreateMainPetAvatar(AvatarInfo info)
	{
		if (info == null)
		{
			return;
		}
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(info.CreatureIndex);
		if (data == null)
		{
			return;
		}
		PetInfo petInfo = GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(data.Index);
		if (petInfo != null)
		{
			MapComponent mapComponent = _myMediator.MapComponent;
			PetAvatar petAvatar = new PetAvatar();
			_mainPetAvatarMediator = new CrowdMediator();
			AvatarController avatarController = new AvatarController(_mainPetAvatarMediator);
			_mainPetAvatarMediator.Init(new CrowdMediator.CrowdMediatorInfo
			{
				ChapterIndex = info.ChapterIndex,
				DungeonIndex = info.DungeonIndex,
				Avatar = petAvatar,
				AvatarController = avatarController,
				MapComponent = mapComponent,
				CrowdType = CrowdType.Pet
			});
			RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, data.CodeName, petInfo.Star);
			if (renderFacadeBuilder != null)
			{
				_crowdDictionary.Add(new CrowdKey(CrowdType.Pet, info.CreatureIndex), _mainPetAvatarMediator);
				PetAvatar.PetAvatarInfo petAvatarInfo = new PetAvatar.PetAvatarInfo
				{
					PetInfo = petInfo,
					AvatarInfo = new Avatar.AvatarInfo
					{
						AvatarController = avatarController,
						MapComponent = mapComponent,
						RenderFacadeBuilder = renderFacadeBuilder,
						Distance = GetAvatarDistance(_crowdDictionary.Count - 1)
					},
					AddCheckHIt = false,
					TouchAction = null
				};
				petAvatar.Open(petAvatarInfo, mapComponent);
				petAvatar.RenderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
				int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("WorldMapPetScalePercentage", 150);
				petAvatar.SetScale(valueInt);
				_mainPetAvatarMediator.Avatar.SetPositionByDungeonIndex(info.DungeonIndex);
				_mainPetAvatarMediator.Avatar.SetForcedDirection(_myMediator.Avatar.CurrentDirection);
				_myMediator.AvatarController.SetAiAvatarControllerListener(_mainPetAvatarMediator);
			}
		}
	}

	public void ChangeMainPetAvatar(AvatarInfo info)
	{
		if (info != null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(info.CreatureIndex);
			if (data != null && GlobalAutoBehaviour<AccountManager>.instance.PetManager.Get(data.Index) != null)
			{
				RemoveAvatarPet();
				CreateMainPetAvatar(info);
			}
		}
	}

	public void RemoveAvatarPet()
	{
		CloseMainPetAvatarMediator();
		RemoveMainPetAvatar();
	}

	private CrowdMediator CreateAvatar(AvatarInfo info, CrowdType crowdType)
	{
		if (info == null)
		{
			return null;
		}
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(info.CostumeIndex);
		if (data == null)
		{
			return null;
		}
		MapComponent mapComponent = _myMediator.MapComponent;
		PartyAvatar partyAvatar = new PartyAvatar();
		CrowdMediator crowdMediator = new CrowdMediator();
		AvatarController avatarController = new AvatarController(crowdMediator);
		crowdMediator.Init(new CrowdMediator.CrowdMediatorInfo
		{
			ChapterIndex = info.ChapterIndex,
			DungeonIndex = info.DungeonIndex,
			Avatar = partyAvatar,
			AvatarController = avatarController,
			MapComponent = mapComponent,
			CrowdType = crowdType
		});
		if (crowdMediator == null || crowdMediator.Avatar == null)
		{
			return null;
		}
		RenderFacadeBuilder renderFacadeBuilder = UIAvatarHelper.CreateRenderFacadeBuilder(new UIAvatarHelper.AvatarInfo(data).AvatarIndex);
		_crowdDictionary.Add(new CrowdKey(crowdType, info.CostumeIndex), crowdMediator);
		partyAvatar.Open(new Avatar.AvatarInfo
		{
			AvatarController = crowdMediator.AvatarController,
			MapComponent = crowdMediator.MapComponent,
			RenderFacadeBuilder = renderFacadeBuilder,
			Distance = GetAvatarDistance(_crowdDictionary.Count - 1)
		});
		crowdMediator.Avatar.SetPositionByDungeonIndex(info.DungeonIndex);
		crowdMediator.Avatar.SetForcedDirection(Vector2.zero);
		return crowdMediator;
	}

	public void ChangeCrowdToPartyMember(int costumeIndex)
	{
		CrowdKey key = new CrowdKey(CrowdType.Crowd, costumeIndex);
		CrowdMediator o_value = new CrowdMediator();
		if (_crowdDictionary.TryGetValue(key, out o_value))
		{
			_crowdDictionary.RemoveSlow(key);
			_crowdDictionary.Add(new CrowdKey(CrowdType.PartyMember, costumeIndex), o_value);
			_myMediator.AvatarController.SetAiAvatarControllerListener(o_value);
		}
	}

	public void PartyMemberAvatarMoveToDungeon(int targetDungeonIndex)
	{
		if (_crowdDictionary.IsNullOrEmpty())
		{
			return;
		}
		CrowdKey crowdKey = null;
		CrowdMediator crowdMediator = null;
		for (int i = 0; i < _crowdDictionary.Count; i++)
		{
			KeyValuePair<CrowdKey, CrowdMediator> keyValuePairByIndex = _crowdDictionary.GetKeyValuePairByIndex(i);
			crowdKey = keyValuePairByIndex.Key;
			if (crowdKey != null && crowdKey.Item1 == CrowdType.PartyMember)
			{
				crowdMediator = keyValuePairByIndex.Value;
				if (crowdMediator != null && crowdMediator.Avatar != null)
				{
					crowdMediator.Avatar.MoveToDungeonIndex(targetDungeonIndex, userInput: false);
				}
			}
		}
	}

	private void TouchMainPetAvatar(bool isPress)
	{
		if (!isPress)
		{
			return;
		}
		UICamera.MouseOrTouch currentTouch = UICamera.currentTouch;
		if (currentTouch == null)
		{
			return;
		}
		Ray ray = Vespa_Unity_Camera_Extension_Method.VespaScreenPointToRay(UICamera.currentCamera, currentTouch.pos);
		Debug.DrawRay(ray.origin, ray.direction * 100f);
		Physics.Raycast(ray, out var hitInfo, 100f);
		if (!(hitInfo.collider == null) && _mainPetAvatarMediator != null && _mainPetAvatarMediator.Avatar != null && !(_mainPetAvatarMediator.Avatar.TouchInteractionComponent == null))
		{
			BoxCollider boxCollider = _mainPetAvatarMediator.Avatar.TouchInteractionComponent.BoxCollider;
			if (hitInfo.collider.Equals(boxCollider))
			{
				_mainPetAvatarMediator.Avatar.TouchInteractionComponent.Click();
			}
		}
	}

	public void Touch(bool isPress)
	{
		TouchMainPetAvatar(isPress);
	}

	void IAvatarContainer.ForEach(Action<Avatar> action)
	{
		if (_crowdDictionary.IsNullOrEmpty())
		{
			return;
		}
		for (int i = 0; i < _crowdDictionary.Count; i++)
		{
			CrowdMediator valueByIndex = _crowdDictionary.GetValueByIndex(i);
			if (valueByIndex != null)
			{
				Vespa_System_Action_Extension_Method.fire(action, valueByIndex.Avatar);
			}
		}
	}

	void IAiAvatarListener.EnableAllAiAvatar(bool enable)
	{
		if (_crowdDictionary.IsNullOrEmpty())
		{
			return;
		}
		for (int i = 0; i < _crowdDictionary.Count; i++)
		{
			CrowdMediator valueByIndex = _crowdDictionary.GetValueByIndex(i);
			if (valueByIndex != null && valueByIndex.Avatar != null && !(valueByIndex.Avatar.RenderFacade == null) && !(valueByIndex.Avatar.RenderFacade.transform == null))
			{
				UIHelper.EnableGameObject(valueByIndex.Avatar.RenderFacade.transform.parent, enable);
			}
		}
	}

	private int GetCrowdCountByCrowdType(CrowdType type)
	{
		if (_crowdDictionary.IsNullOrEmpty())
		{
			return 0;
		}
		CrowdKey crowdKey = null;
		int num = 0;
		for (int i = 0; i < _crowdDictionary.Count; i++)
		{
			crowdKey = _crowdDictionary.GetKeyByIndex(i);
			if (crowdKey != null && crowdKey.Item1 == type)
			{
				num++;
			}
		}
		return num;
	}

	private Vector2 GetAvatarDistance(int index)
	{
		CrowdKey keyByIndex = _crowdDictionary.GetKeyByIndex(index);
		Vector2 result = DefaultDistance;
		if (keyByIndex == null)
		{
			return result;
		}
		CrowdType item = keyByIndex.Item1;
		if (item == CrowdType.Pet)
		{
			result = DefaultPetDistance;
		}
		if (_crowdDictionary.IsNullOrEmpty())
		{
			return result;
		}
		int crowdCountByCrowdType = GetCrowdCountByCrowdType(item);
		result.x *= crowdCountByCrowdType + 1;
		result.y *= crowdCountByCrowdType + 1;
		return result;
	}
}
