using System;
using System.Collections.Generic;
using NGameAsset.NEffect;
using NGameAsset.NSound;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGameAsset;

public class AssetUpdateManager : SingletonUpdater<AssetUpdateManager>
{
	public struct SoundInfo
	{
		public ISound Sound;

		public float DetectZombieTime;
	}

	public struct EffectEntry
	{
		public IAsyncLoad<IEffect> EffectAsync;

		public bool DetachParentOnRelease;

		public bool IsGlobal;

		public Action OnEnd;
	}

	public struct DelegateReleaseEffectEntry
	{
		public IAsyncLoad<IEffect> EffectAsync;

		public bool DetachParent;

		public bool ImmediatelyRelease;
	}

	public struct ReleaseEffectEntry
	{
		public EffectLoad EffectLoad;

		public float DetectZombieTime;
	}

	public const float MaxReleaseWaitTime = 10f;

	private Transform _dummyRootTm;

	private LinkedList<EffectEntry> EffectList = new LinkedList<EffectEntry>();

	private LinkedList<SoundInfo> SoundList = new LinkedList<SoundInfo>();

	private LinkedList<ReleaseEffectEntry> ReleaseEffectList = new LinkedList<ReleaseEffectEntry>();

	private Queue<Tuple<int, GameObject>> DestroyGoQueue = new Queue<Tuple<int, GameObject>>();

	private List<Action> DelegateActionList = new List<Action>();

	public static Transform DummyRootTm
	{
		get
		{
			if (!SingletonUpdater<AssetUpdateManager>.isValidInstance)
			{
				return null;
			}
			return SingletonUpdater<AssetUpdateManager>.instance._dummyRootTm;
		}
	}

	public AssetUpdateManager()
	{
		GameObject gameObject = new GameObject("AssetUpdateManager.DummyRoot");
		_dummyRootTm = gameObject.transform;
		_dummyRootTm.parent = GlobalAutoManager.SingletonComponent.transform;
		GlobalOnceBehaviour<GameAssetGui>.CreateInstance();
	}

	public void Add(IAsyncLoad<IEffect> effectAsync, bool isGlobal)
	{
		EffectList.AddLast(new EffectEntry
		{
			EffectAsync = effectAsync,
			DetachParentOnRelease = true,
			IsGlobal = isGlobal,
			OnEnd = null
		});
	}

	public void Add(IAsyncLoad<IEffect> effectAsync, bool detachParentOnRelease, bool isGlobal, Action onEnd)
	{
		EffectList.AddLast(new EffectEntry
		{
			EffectAsync = effectAsync,
			DetachParentOnRelease = detachParentOnRelease,
			IsGlobal = isGlobal,
			OnEnd = onEnd
		});
	}

	public static void DelegateReleaseEffect(IAsyncLoad<IEffect> asyncLoad, bool detachParent, bool immediatelyRelease)
	{
		if (!SystemStatus.IsApplicationQuit && SingletonUpdater<AssetUpdateManager>.isValidInstance)
		{
			SingletonUpdater<AssetUpdateManager>.instance.DelegateActionList.Add(delegate
			{
				EffectContainer.ReleaseEffect(asyncLoad, detachParent, immediatelyRelease);
			});
		}
	}

	public static void DelegateReleaseEffect(EffectInstance effect)
	{
		if (!SingletonUpdater<AssetUpdateManager>.isValidInstance)
		{
			return;
		}
		SingletonUpdater<AssetUpdateManager>.instance.DelegateActionList.Add(delegate
		{
			if (effect != null)
			{
				effect.Close(detachParent: true);
				effect.Release();
			}
		});
	}

	public static void PendingReleaseFromEffectContainer(EffectLoad effectLoad)
	{
		if (SingletonUpdater<AssetUpdateManager>.isValidInstance)
		{
			SingletonUpdater<AssetUpdateManager>.instance.ReleaseEffectList.AddLast(new ReleaseEffectEntry
			{
				EffectLoad = effectLoad,
				DetectZombieTime = MainTimer.time + 10f
			});
		}
	}

	public static void PendingDestroyGo(GameObject go)
	{
		if (!(go == null))
		{
			go.transform.parent = null;
			if (!SingletonUpdater<AssetUpdateManager>.isValidInstance)
			{
				VespaObject.DestroyGo(go);
				return;
			}
			go.SetActive(value: false);
			SingletonUpdater<AssetUpdateManager>.instance.DestroyGoQueue.Enqueue(Tuple.Create(Time.frameCount, go));
		}
	}

	public void Add(ISound sound, float detectZombieTime)
	{
		SoundInfo value = new SoundInfo
		{
			Sound = sound,
			DetectZombieTime = MainTimer.time + detectZombieTime
		};
		SoundList.AddLast(value);
	}

	public ISound GetSound(string soundCode)
	{
		foreach (SoundInfo sound in SoundList)
		{
			if (sound.Sound.Code == soundCode)
			{
				return sound.Sound;
			}
		}
		return null;
	}

	public bool IsEmptySoundList()
	{
		return __Extension_Method_ICollection__.IsNullOrEmpty(SoundList);
	}

	public void DestroyOnSceneChanged()
	{
		LinkedListNode<EffectEntry> linkedListNode = EffectList.First;
		while (linkedListNode != null)
		{
			EffectEntry value = linkedListNode.Value;
			if (value.IsGlobal)
			{
				linkedListNode = linkedListNode.Next;
				continue;
			}
			LinkedListNode<EffectEntry> linkedListNode2 = linkedListNode;
			linkedListNode = linkedListNode2.Next;
			EffectContainer.ReleaseEffect(value.EffectAsync, value.DetachParentOnRelease);
			EffectList.Remove(linkedListNode2);
		}
	}

	public override void ChainUpdate()
	{
		UpdateDelegateAction();
		UpdateEffectList();
		UpdateSoundList();
		UpdateReleaseEffectList();
		UpdateDestroyGoQueue();
		if (SingletonAuto<BgmManager2>.isValidInstance)
		{
			SingletonAuto<BgmManager2>.instance.ChainUpdate();
		}
		if (SingletonAuto<AudioClipContainer>.isValidInstance)
		{
			SingletonAuto<AudioClipContainer>.instance.DestroyUnusedLastOne();
		}
		BundleAssetManager.DestroyUnused(unusedAll: false);
		BundleManager.DestroyUnused(unusedAll: false);
		if (SingletonAuto<TableDataManager>.isValidInstance)
		{
			SingletonAuto<TableDataManager>.instance.ChainUpdate();
		}
	}

	private void UpdateDelegateAction()
	{
		for (int i = 0; i < DelegateActionList.Count; i++)
		{
			Vespa_System_Action_Extension_Method.fire(DelegateActionList[i]);
		}
		DelegateActionList.Clear();
	}

	private void UpdateEffectList()
	{
		LinkedListNode<EffectEntry> linkedListNode = EffectList.First;
		while (linkedListNode != null)
		{
			EffectEntry value = linkedListNode.Value;
			value.EffectAsync.Value.Update();
			if (!value.EffectAsync.Value.IsEnd())
			{
				linkedListNode = linkedListNode.Next;
				continue;
			}
			LinkedListNode<EffectEntry> linkedListNode2 = linkedListNode;
			linkedListNode = linkedListNode2.Next;
			EffectContainer.ReleaseEffect(value.EffectAsync, value.DetachParentOnRelease);
			Vespa_System_Action_Extension_Method.fire(value.OnEnd);
			EffectList.Remove(linkedListNode2);
		}
	}

	private void UpdateSoundList()
	{
		float time = MainTimer.time;
		LinkedListNode<SoundInfo> linkedListNode = SoundList.First;
		while (linkedListNode != null)
		{
			linkedListNode.Value.Sound.Update();
			if (time < linkedListNode.Value.DetectZombieTime && !linkedListNode.Value.Sound.IsEnd())
			{
				linkedListNode = linkedListNode.Next;
				continue;
			}
			LinkedListNode<SoundInfo> linkedListNode2 = linkedListNode;
			linkedListNode = linkedListNode2.Next;
			SoundContainer.ReleaseSound(linkedListNode2.Value.Sound);
			SoundList.Remove(linkedListNode2);
		}
	}

	private void UpdateReleaseEffectList()
	{
		float time = MainTimer.time;
		LinkedListNode<ReleaseEffectEntry> linkedListNode = ReleaseEffectList.First;
		while (linkedListNode != null)
		{
			if (time < linkedListNode.Value.DetectZombieTime && !linkedListNode.Value.EffectLoad.IsEmpty())
			{
				linkedListNode.Value.EffectLoad.Value.Update();
				linkedListNode = linkedListNode.Next;
				continue;
			}
			LinkedListNode<ReleaseEffectEntry> linkedListNode2 = linkedListNode;
			linkedListNode = linkedListNode.Next;
			linkedListNode2.Value.EffectLoad.Release();
			ReleaseEffectList.Remove(linkedListNode2);
		}
	}

	private void UpdateDestroyGoQueue()
	{
		while (DestroyGoQueue.Count > 0)
		{
			Tuple<int, GameObject> tuple = DestroyGoQueue.Peek();
			if (tuple.Item1 + 1 < Time.frameCount)
			{
				DestroyGoQueue.Dequeue();
				VespaObject.DestroyGo(tuple.Item2);
				continue;
			}
			break;
		}
	}
}
