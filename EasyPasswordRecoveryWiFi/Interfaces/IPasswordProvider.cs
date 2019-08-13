
namespace EasyPasswordRecoveryWiFi.Interfaces
{
	/// <summary>
	/// Password provider, provides passwords for the connection process.    
	/// </summary>
	public interface IPasswordProvider
	{
		string GetFirst();
		string GetNext();
		bool IsEmpty { get; }
	}
}
