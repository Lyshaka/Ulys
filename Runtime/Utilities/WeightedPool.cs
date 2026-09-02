using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ulys.Runtime.Utilities
{

[Serializable]
public class WeightedPool<T>
{
	[SerializeField]
	private List<WeightedEntry> pool = new();
	
	[Serializable]
	public class WeightedEntry
	{
		public T item;
		public int weight;
	}
}


}
