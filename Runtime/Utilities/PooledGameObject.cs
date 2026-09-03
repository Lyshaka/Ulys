using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ulys.Runtime.Utilities
{

[Serializable]
public class PooledGameObject : IDisposable
{
	[SerializeField] private GameObject prefab;
	
	[SerializeField] private Transform parent;
	[SerializeField, Min(1)] private int initialSize = 8;
	[SerializeField] private bool prewarm = true;

	[NonSerialized] private Stack<GameObject> _availables;
	[NonSerialized] private HashSet<GameObject> _actives;
	[NonSerialized] private Dictionary<GameObject, IPoolableGameObject[]> _poolables;

	[NonSerialized] private bool _isInitialized;
	[NonSerialized] private Transform _parent;
	
	public int AvailableInstances { get { Init(); return _availables.Count; } }
	public int ActiveInstances { get { Init(); return _actives.Count; } }
	public int TotalInstances { get { Init(); return _poolables.Count; } }

	private void Init()
	{
		if (_isInitialized)
			return;
		
		if (!prefab)
			throw new NullReferenceException("Prefab cannot be null.");
		
		_availables = new(initialSize);
		_actives = new(initialSize);
		_poolables = new(initialSize);

		_parent = parent ? parent : new GameObject($"# {prefab.name} Pool #").transform;

		if (prewarm)
		{
			for (int i = 0; i < initialSize; i++)
				CreateInstance();
		}
		
		_isInitialized = true;
	}

	public GameObject Get()
	{
		if (!_isInitialized)
			Init();
		
		if (_availables.Count == 0)
			CreateInstance();

		GameObject instance = _availables.Pop();
		_actives.Add(instance);

		foreach (IPoolableGameObject poolable in _poolables[instance])
			poolable.OnGet();

		instance.SetActive(true);
		return instance;
	}

	public void Release(GameObject instance)
	{
		if (!instance)
			throw new ArgumentNullException(nameof(instance));

		if (!BelongsToPool(instance))
			throw new ArgumentException("A GameObject not belonging to the pool was returned to it.", nameof(instance));
		
		if (!_actives.Remove(instance))
			throw new ArgumentException("An instanced GameObject was returned twice to the pool.", nameof(instance));
		
		foreach (IPoolableGameObject poolable in _poolables[instance])
			poolable.OnRelease();

		instance.SetActive(false);
		_availables.Push(instance);
	}

	public void ReleaseAll()
	{
		foreach (GameObject instance in _actives)
		{
			foreach (IPoolableGameObject poolable in _poolables[instance])
				poolable?.OnRelease();

			if (!instance)
				continue;

			instance.SetActive(false);
			_availables.Push(instance);
		}
		
		_actives.Clear();
	}

	public bool BelongsToPool(GameObject instance)
	{
		return _isInitialized && _poolables.ContainsKey(instance);
	}

	// ReSharper disable Unity.PerformanceAnalysis
	public void Dispose()
	{
		if (!_isInitialized)
			return;

		ReleaseAll();

		foreach (KeyValuePair<GameObject, IPoolableGameObject[]> pair in _poolables)
			Object.Destroy(pair.Key);
		
		_availables.Clear();
		_actives.Clear();
		_poolables.Clear();
		
		if (!parent && _parent && _parent.gameObject)
			Object.Destroy(_parent.gameObject);

		_isInitialized = false;
	}

	private void CreateInstance()
	{
		GameObject obj = Object.Instantiate(prefab, _parent);
		obj.name = $"{prefab.name}_{obj.GetEntityId()}";
		
		_poolables.Add(obj, obj.GetComponentsInChildren<IPoolableGameObject>(true));

		_availables.Push(obj);
		obj.SetActive(false);
	}
}

}
