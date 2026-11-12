using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public static class Common
{
	public class HeroPortraitAnim
	{
		public enum EAnimation
		{
			None,
			Insert,
			StepAside,
			Disappear
		}

		public HeroPortraitComponent Component;

		public bool IsMainParty = true;

		public EAnimation Animation { get; private set; }

		public Vector3 StartPos { get; private set; }

		public Vector3 EndPos { get; private set; }

		public float StartTime { get; private set; }

		public int HeroIndex
		{
			get
			{
				if (Component == null)
				{
					return 0;
				}
				if (Component.HeroInfo == null)
				{
					return 0;
				}
				return Component.HeroInfo.HeroIndex;
			}
		}

		public HeroPortraitAnim()
		{
			Animation = EAnimation.None;
			StartPos = Vector3.zero;
			EndPos = Vector3.zero;
			StartTime = 0f;
		}

		public void StartAnimation(EAnimation animation, Vector3 startPos, Vector3 endPos)
		{
			Animation = animation;
			StartPos = startPos;
			EndPos = endPos;
			StartTime = Time.time;
		}

		public void StopAnimation()
		{
			Animation = EAnimation.None;
			UpdateAnchor();
		}

		private void UpdateAnchor()
		{
			UITexture uITexture = Component.HeroBundleTex.UITEX();
			if (uITexture != null)
			{
				uITexture.ResetAndUpdateAnchors();
				uITexture.ForceUpdateAnchors(includeChildren: true);
			}
			Component.SubCompContainer.ForEach<BaseSubComponent<HeroPortraitComponent>>(delegate(BaseSubComponent<HeroPortraitComponent> e)
			{
				UIWidget root = e.Root;
				if (!(root == null))
				{
					root.ResetAndUpdateAnchors();
					root.ForceUpdateAnchors(includeChildren: true);
				}
			});
		}

		public bool UpdateAnimation()
		{
			if (Animation == EAnimation.None)
			{
				return false;
			}
			float epsilon = Mathf.Epsilon;
			AnimationCurve animationCurve = null;
			if (Animation == EAnimation.Insert)
			{
				animationCurve = GameSetting.data.Ui.PartySetting.EnterCurve;
				epsilon = GameSetting.data.Ui.PartySetting.EnterTime;
			}
			else if (Animation == EAnimation.StepAside)
			{
				animationCurve = GameSetting.data.Ui.PartySetting.StepAsideCurve;
				epsilon = GameSetting.data.Ui.PartySetting.EnterTime;
			}
			else
			{
				if (Animation != EAnimation.Disappear)
				{
					Component.transform.localPosition = EndPos;
					StopAnimation();
					return false;
				}
				animationCurve = GameSetting.data.Ui.PartySetting.DisappearCurve;
				epsilon = GameSetting.data.Ui.PartySetting.DisappearTime;
			}
			float num = Time.time - StartTime;
			if (num > epsilon)
			{
				Component.transform.localPosition = EndPos;
				StopAnimation();
				return false;
			}
			float num2 = num / Vespa_Float_Extension_Method.NoneZero(epsilon);
			float num3 = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(animationCurve, num2, num2);
			Vector3 localPosition = default(Vector3);
			localPosition.x = NVespa.Math.LerpOver(StartPos.x, EndPos.x, num3);
			localPosition.y = NVespa.Math.LerpOver(StartPos.y, EndPos.y, num3);
			localPosition.z = NVespa.Math.LerpOver(StartPos.z, EndPos.z, num3);
			Component.transform.localPosition = localPosition;
			if (Animation == EAnimation.Disappear)
			{
				Component.WidgetThis.alpha = Mathf.Clamp01(1f - num3);
			}
			return true;
		}
	}

	public class HeroInfoKey : Tuple<long, int>
	{
		public HeroInfoKey(long AccountId, int HeroIndex)
			: base(AccountId, HeroIndex)
		{
		}
	}

	public static HeroPortraitAnim FindInParty(HeroPortraitComponent heroComponent, List<HeroPortraitAnim> _partyMemberList)
	{
		return _partyMemberList.Find(delegate(HeroPortraitAnim partyMember)
		{
			if (heroComponent.HeroInfo == null)
			{
				return false;
			}
			return heroComponent.HeroInfo.HeroIndex == partyMember.HeroIndex && heroComponent.OwnerId == partyMember.Component.OwnerId;
		});
	}

	public static void SortMemberByWavePosition(List<HeroPortraitAnim> memberList)
	{
		Dictionary<int, long> backupScore = new Dictionary<int, long>();
		memberList.Sort(delegate(HeroPortraitAnim lhs, HeroPortraitAnim rhs)
		{
			CachedHeroInfo cachedHeroInfo = lhs?.Component.HeroInfo;
			CachedHeroInfo cachedHeroInfo2 = rhs?.Component.HeroInfo;
			long num = 0L;
			long num2 = 0L;
			if (backupScore.ContainsKey(cachedHeroInfo.HeroIndex))
			{
				num = backupScore[cachedHeroInfo.HeroIndex];
			}
			else
			{
				num = StatCalculator.GetCreatureWavePositionScore(cachedHeroInfo);
				backupScore[cachedHeroInfo.HeroIndex] = num;
			}
			if (backupScore.ContainsKey(cachedHeroInfo2.HeroIndex))
			{
				num2 = backupScore[cachedHeroInfo2.HeroIndex];
			}
			else
			{
				num2 = StatCalculator.GetCreatureWavePositionScore(cachedHeroInfo2);
				backupScore[cachedHeroInfo2.HeroIndex] = num2;
			}
			return num2.CompareTo(num);
		});
		backupScore.Clear();
		backupScore = null;
	}

	public static void AnimateMemberPosition(HeroPortraitAnim partyMember, Vector3 destination, bool isInsertMember)
	{
		if (partyMember == null)
		{
			return;
		}
		if (Vector3.Distance(partyMember.Component.transform.position, destination) < Mathf.Epsilon)
		{
			partyMember.StopAnimation();
			return;
		}
		Vector3 localPosition = partyMember.Component.transform.localPosition;
		Vector3 vector = partyMember.Component.transform.parent.InverseTransformPoint(destination);
		if (isInsertMember)
		{
			partyMember.StartAnimation(HeroPortraitAnim.EAnimation.Insert, localPosition, vector);
		}
		else if (partyMember.Animation == HeroPortraitAnim.EAnimation.None || !(Vector3.Distance(partyMember.EndPos, vector) < Mathf.Epsilon))
		{
			partyMember.StartAnimation(HeroPortraitAnim.EAnimation.StepAside, localPosition, vector);
		}
	}

	public static DateTime GetUTCNow()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
	}

	public static int GetUniqueItemArchiveTotalCount(ArchiveSubType subType, CreatureTagType tagType = CreatureTagType.None)
	{
		int itemCount = 0;
		PlayerArchiveData[] playerArchiveDatas = GameRoot.Instance.PlayerArchiveDataContainer.GetDataBySubType(subType);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(playerArchiveDatas))
		{
			return 0;
		}
		GameRoot.Instance.EquipItemDataContainer.ForEachData(delegate(EquipItemData equipItemData)
		{
			int[] array = null;
			PlayerArchiveData[] array2 = playerArchiveDatas;
			foreach (PlayerArchiveData playerArchiveData in array2)
			{
				if (playerArchiveData.ConditionKey3.Equals("DetailIndex"))
				{
					array = playerArchiveData.ConditionValue3;
				}
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(array))
			{
				bool flag = false;
				int[] array3 = array;
				for (int i = 0; i < array3.Length; i++)
				{
					if (array3[i] == equipItemData.DetailIndex)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(equipItemData.EquipCode);
					if (dataByCode != null && dataByCode.IsArchive && (__Extension_Method_ICollection__.IsNullOrEmpty(equipItemData.TagType) || tagType == CreatureTagType.None || equipItemData.TagType[0] == tagType))
					{
						itemCount++;
					}
				}
			}
		});
		return itemCount;
	}

	public static int GetEquipItemArchiveTotalCount()
	{
		int itemCount = 0;
		PlayerArchiveData[] playerArchiveDatas = GameRoot.Instance.PlayerArchiveDataContainer.GetDataBySubType(ArchiveSubType.EquipItem);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(playerArchiveDatas))
		{
			return 0;
		}
		GameRoot.Instance.ItemDataContainer.ForEachData(delegate(ItemData itemData)
		{
			if (itemData.IsArchive && itemData.Type == ItemType.Equip)
			{
				List<int> list = new List<int>();
				PlayerArchiveData[] array = playerArchiveDatas;
				foreach (PlayerArchiveData playerArchiveData in array)
				{
					if (playerArchiveData.ConditionKey2.Equals("ItemGrade"))
					{
						list.AddRange(playerArchiveData.ConditionValue2);
					}
				}
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
				{
					bool flag = false;
					foreach (int item in list)
					{
						if (item == itemData.Grade)
						{
							flag = true;
							break;
						}
					}
					if (flag && itemData.GetSubData<EquipItemData>() != null)
					{
						itemCount++;
					}
				}
			}
		});
		return itemCount;
	}

	public static int GetNormlaItemArchiveTotalCount()
	{
		PlayerArchiveData[] playerArchiveDatas = GameRoot.Instance.PlayerArchiveDataContainer.GetDataBySubType(ArchiveSubType.NormalItem);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(playerArchiveDatas))
		{
			return 0;
		}
		int itemCount = 0;
		GameRoot.Instance.ItemDataContainer.ForEachData(delegate(ItemData itemData)
		{
			if (itemData.IsArchive)
			{
				List<ItemType> list = new List<ItemType>();
				PlayerArchiveData[] array = playerArchiveDatas;
				foreach (PlayerArchiveData playerArchiveData in array)
				{
					if (playerArchiveData.ConditionKey1.Equals("ItemType"))
					{
						list.AddRange(playerArchiveData.ConditionValue1);
					}
				}
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
				{
					bool flag = false;
					foreach (ItemType item in list)
					{
						if (item == itemData.Type)
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						itemCount++;
					}
				}
			}
		});
		return itemCount;
	}
}
