namespace Ulys.Runtime.Utilities
{

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;

public class PooledObject<T> : IDisposable where T : MonoBehaviour
{
	private readonly GameObject _object;
	private Action _onUpdate;
	private readonly Transform _parent;
	private readonly float _lifetime;
	private readonly ObjectPool<T> _pool;
	private readonly Dictionary<T, float> _activeObjects;
	private readonly List<T> _objectsToRelease;
	
	public int ActiveObjectsCount => _activeObjects.Count;

	public PooledObject(GameObject obj, Action onUpdate, Transform parent = null, float lifetime = 10f, int defaultCapacity = 10, int maxSize = 100)
	{
		_object = obj;
		GameObject parentObj = new GameObject($"Pooled_{obj.name}");
		if (parent != null)
			parentObj.transform.SetParent(parent);
		_parent = parentObj.transform;
		_lifetime = lifetime;
		
		_pool = new
		(
			CreateItem,
			null,
			null,
			OnDestroyItem,
			true,
			defaultCapacity,
			maxSize
		);

		_activeObjects = new(defaultCapacity);
		_objectsToRelease = new(defaultCapacity);

		_onUpdate = onUpdate;
		_onUpdate += Tick;
	}
	
	public void Dispose()
	{
		_pool.Dispose();
		if (_parent != null)
			GameObject.Destroy(_parent.gameObject);
		_onUpdate -= Tick;
	}
	
	#region PUBLIC_METHODS

	public void Tick()
	{
		_objectsToRelease.Clear();
		
		foreach ((T item, float time) in _activeObjects)
		{
			if (Time.time - time > _lifetime)
				_objectsToRelease.Add(item);
		}

		foreach (T item in _objectsToRelease)
			Destroy(item);
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Spawn(Vector3 position)
	{
		return Spawn(position, Quaternion.identity);
	}
	
	public T Spawn(Vector3 position, Quaternion rotation)
	{
		T item = _pool.Get();
		item.transform.SetPositionAndRotation(position, rotation);
		_activeObjects.Add(item, Time.time);

		if (item is IPoolableObject poolable)
			poolable.OnSpawn();
		
		item.gameObject.SetActive(true);
		
		return item;
	}

	public void Destroy(T item)
	{
		if (_activeObjects.Remove(item))
		{
			if (item != null)
			{
				item.gameObject.SetActive(false);
			
				if (item is IPoolableObject poolable)
					poolable.OnRelease();
			}
			_pool.Release(item);
		}
	}
	
	#endregion

	private T CreateItem()
	{
		GameObject obj = GameObject.Instantiate(_object, Vector3.zero, Quaternion.identity, _parent);
		obj.name = $"{_parent.gameObject.name}_{obj.GetEntityId()}";
		obj.SetActive(false);
		T component = obj.GetComponent<T>();
		if (component == null)
			throw new NullReferenceException($"Can't create object of type {typeof(T).Name}");

		if (component is IPoolableObject poolable)
			poolable.OnCreate();
		
		return component;
	}

	private void OnDestroyItem(T item)
	{
		if (item is IPoolableObject poolable)
			poolable.OnDestroy();
		
		GameObject.Destroy(item.gameObject);
	}
}

public interface IPoolableObject
{
	public void OnCreate() { }
	public void OnSpawn() { }
	public void OnRelease() { }
	public void OnDestroy() { }
}

}
