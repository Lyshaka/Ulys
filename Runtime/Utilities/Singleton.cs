namespace Ulys.Runtime.Utilities
{

using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetInstance()
	{
		Instance = null;
	}

	protected virtual void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		
		Instance = this as T;
		//DontDestroyOnLoad(gameObject);

		Initialize();
	}

	protected virtual void Initialize() {}
}

}
