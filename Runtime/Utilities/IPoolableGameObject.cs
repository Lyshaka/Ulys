namespace Ulys.Runtime.Utilities
{

public interface IPoolableGameObject
{
	void OnGet();
	void OnRelease();
}

}
