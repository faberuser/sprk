using System;
using System.Collections.Generic;
using NGame2.NUI.NGame3DWidget;
using NGame2.NUI.NGame3DWidgetFromRes;
using NVespa;
using UnityEngine;

namespace NGame2.NUI;

public class UIGame3DWidgetFromRes : UIWidgetExtraRenderQ
{
	public enum EContentsType
	{
		Effect,
		Sound
	}

	[Serializable]
	public class EntryData
	{
		public EContentsType ContentsType;

		public UnityEngine.Object ResourceObject;

		public float DelayTime;

		public TransformData ExtraTransform = new TransformData(100f);

		[NonSerialized]
		public NGame2.NUI.NGame3DWidgetFromRes.IContents Contents;

		[NonSerialized]
		public bool AutoDestroy;
	}

	[HideInInspector]
	public List<EntryData> EntryList = new List<EntryData>();

	private bool _reserveContents;

	private RenderQueueVisitor _renderQueueVisitor = new RenderQueueVisitor();

	private ILocalTimer _localTimer;

	public ILocalTimer LocalTimer
	{
		get
		{
			if (_localTimer == null)
			{
				return UnityLocalTimer.Default;
			}
			return _localTimer;
		}
		set
		{
			_localTimer = value;
		}
	}

	public bool IsPlaying
	{
		get
		{
			for (int i = 0; i < EntryList.Count; i++)
			{
				EntryData entryData = EntryList[i];
				if (entryData.Contents != null && entryData.Contents.GetInstanceCount() > 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void PlayEntry(int entryIndex, Action onComplete)
	{
		ReserveContents();
		EntryData valid = Vespa_Generic_IList_Extension_Method.GetValid(EntryList, entryIndex);
		if (valid == null)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
		}
		else if (valid.Contents != null)
		{
			valid.Contents.Play(valid.ExtraTransform, LocalTimer, onComplete);
		}
	}

	public void Play(Vector3 offset, Action onComplete)
	{
		ReserveContents();
		if (__Extension_Method_ICollection__.COUNT(EntryList) == 0)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			return;
		}
		int playCount = EntryList.Count;
		int completeCount = 0;
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents == null)
			{
				playCount--;
				continue;
			}
			TransformData transformData = entryData.ExtraTransform;
			if (!NVespa.Math.Approximately(offset, Vector3.zero))
			{
				transformData = transformData.Clone();
				transformData.Position += offset;
			}
			entryData.Contents.Play(transformData, LocalTimer, delegate
			{
				completeCount++;
				if (playCount == completeCount)
				{
					Vespa_System_Action_Extension_Method.fire(onComplete);
				}
			});
		}
	}

	public void Play(Action onComplete)
	{
		Play(Vector3.zero, onComplete);
	}

	public void PlayEntryPhase(int entryIndex, string phaseName)
	{
		EntryData valid = Vespa_Generic_IList_Extension_Method.GetValid(EntryList, entryIndex);
		if (valid != null && valid.Contents != null)
		{
			valid.Contents.PlayPhase(phaseName);
		}
	}

	public void PlayPhase(string phaseName)
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents != null)
			{
				entryData.Contents.PlayPhase(phaseName);
			}
		}
	}

	public void StopEntry(int entryIndex, bool immediatelyClear)
	{
		EntryData valid = Vespa_Generic_IList_Extension_Method.GetValid(EntryList, entryIndex);
		if (valid != null && valid.Contents != null)
		{
			valid.Contents.Stop(immediatelyClear);
		}
	}

	public void Stop(bool immediatelyClear)
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents != null)
			{
				entryData.Contents.Stop(immediatelyClear);
			}
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		ReserveContents();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents != null)
			{
				entryData.Contents.ChainUpdate();
				if (entryData.Contents.GetInstanceCount() == 0 && entryData.AutoDestroy)
				{
					entryData.Contents.ReleaseAll();
					EntryList.RemoveAt(i);
					i--;
				}
			}
		}
	}

	protected override void OnDisable()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDisable();
			Stop(immediatelyClear: true);
		}
	}

	private void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			ReleaseAllContents();
		}
	}

	private void ReleaseAllContents()
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents != null)
			{
				entryData.Contents.ReleaseAll();
				entryData.Contents = null;
			}
		}
	}

	public void ReserveContents()
	{
		if (_reserveContents)
		{
			return;
		}
		_reserveContents = true;
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents == null)
			{
				if (entryData.ContentsType == EContentsType.Effect)
				{
					entryData.Contents = new NGame2.NUI.NGame3DWidgetFromRes.ContentsEffect(this, entryData.ResourceObject);
				}
				else if (entryData.ContentsType == EContentsType.Sound)
				{
					entryData.Contents = new NGame2.NUI.NGame3DWidgetFromRes.ContentsSound(this, entryData.ResourceObject, entryData.DelayTime);
				}
				else
				{
					entryData.Contents = new NGame2.NUI.NGame3DWidgetFromRes.ContentsInvalid();
				}
			}
		}
	}

	public override int UpdateExtraRenderQueue(UIPanel panel, int renderQueue, bool isExplicit)
	{
		for (int i = 0; i < EntryList.Count; i++)
		{
			EntryData entryData = EntryList[i];
			if (entryData.Contents != null)
			{
				entryData.Contents.UpdateExtraRenderQueue(panel, _renderQueueVisitor);
			}
		}
		return _renderQueueVisitor.UpdateExtraRenderQueue(renderQueue, isExplicit);
	}
}
