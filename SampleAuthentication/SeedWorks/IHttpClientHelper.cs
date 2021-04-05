using System.Threading.Tasks;
using SampleAuthentication.Helpers;

namespace SampleAuthentication.SeedWorks
{
	public interface IHttpClientHelper
	{
		Task<TResponse> DeleteAsync<TResponse>(string uri);

		Task<TResponse> GetAsync<TResponse>(string uri, string acceptDomainModel = null, Token token = null);

		Task<TResponse> PostAsync<TData, TResponse>(string uri, TData data, string contentTypeDomainModel = null, Token token = null,
			string acceptDomainModel = null);

		Task<TResponse> PutAsync<TData, TResponse>(string uri, TData data, string contentTypeDomainModel = null, Token token = null,
			string acceptDomainModel = null);
	}
}