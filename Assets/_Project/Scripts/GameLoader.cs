using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameLoader : MonoBehaviour
{
	[Tooltip("Must implement the ILoadable interface.")]
	[SerializeField] private Object[] m_loadable = Array.Empty<Object>();

	private void Awake()
	{
		LoadObjects();
	}

	private void LoadObjects()
	{
		int count = m_loadable.Length;
		for (int i = 0; i < count; ++i)
		{
			ILoadable loadable = m_loadable[i] as ILoadable;
			Assertions.EnsureTrue(
				loadable != null, 
				$"{nameof(GameLoader)}::{nameof(LoadObjects)} -> Array \"{nameof(m_loadable)}\" at \"{i}\" contains a non-{nameof(ILoadable)} object.");
			
			loadable.Load();
		}
	}
}
