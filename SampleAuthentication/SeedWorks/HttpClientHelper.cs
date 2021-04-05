using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SampleAuthentication.Helpers;

namespace SampleAuthentication.SeedWorks
{
	public class HttpClientHelper : IHttpClientHelper
	{
		private static HttpClient Client { get; set; }
		static HttpClientHelper()
		{
			Client = new HttpClient();
		}

		public Task<TResponse> PostAsync<TData, TResponse>(string uri, TData data, string contentTypeDomainModel = null,
			Token token = null, string acceptDomainModel = null)
		{
			return MakeRequest<TData, TResponse>(HttpMethod.Post, uri, data, acceptDomainModel, contentTypeDomainModel);
		}
		public Task<TResponse> PutAsync<TData, TResponse>(string uri, TData data, string contentTypeDomainModel = null,
			Token token = null, string acceptDomainModel = null)
		{
			return MakeRequest<TData, TResponse>(HttpMethod.Put, uri, data, acceptDomainModel, contentTypeDomainModel);
		}
		public Task<TResponse> GetAsync<TResponse>(string uri, string acceptDomainModel = null, Token token = null)
		{
			return MakeRequest<TResponse>(HttpMethod.Get, uri, acceptDomainModel, null, token);
		}
		public Task<TResponse> DeleteAsync<TResponse>(string uri)
		{
			return MakeRequest<TResponse>(HttpMethod.Delete, uri);
		}
		private Task<TResponse> MakeRequest<TData, TResponse>(HttpMethod method, string uri, TData data, string accept = null, string contentTypeDomainModel = null, Token token = null)
		{
			var request = new HttpRequestMessage(method, uri)
			{
				Content = new ObjectContent<TData>(data, new JsonMediaTypeFormatter())
			};
			SetAcceptHeader(request, accept);
			SetContentTypeHeader(request, contentTypeDomainModel);
			SetToken(request, token);
			return GetResponse<TResponse>(request);
		}
		private Task<TResponse> MakeRequest<TResponse>(HttpMethod method, string uri, string acceptDomainModel = null, string contentTypeDomainModel = null, Token token = null)
		{
			var request = new HttpRequestMessage(method, uri);
			SetAcceptHeader(request, acceptDomainModel);
			SetContentTypeHeader(request, contentTypeDomainModel);
			SetToken(request, token);

			return GetResponse<TResponse>(request);
		}
		private async Task<TResponse> GetResponse<TResponse>(HttpRequestMessage request)
		{
			var task = await Client.SendAsync(request);

			var result = await task.Content.ReadAsStringAsync();
			if (!task.IsSuccessStatusCode) throw new Exception(result);
			return JsonConvert.DeserializeObject<TResponse>(result);
		}
		private void SetAcceptHeader(HttpRequestMessage request, string domainModel)
		{
			if (domainModel != null) SetHeader(request, "Accept", $"application/json;domain-model={domainModel}");
		}

		private void SetContentTypeHeader(HttpRequestMessage request, string domainModel)
		{
			if (domainModel != null) SetHeader(request, "Content-Type", $"application/json;domain-model={domainModel}");
		}
		private void SetToken(HttpRequestMessage request, Token token)
		{
			if (token != null) SetHeader(request, "Authorization", $"{token.TokenType} {token.AccessToken}");
		}

		private void SetHeader(HttpRequestMessage request, string headerKey, string headerValue)
		{
			if (request.Content != null)
			{
				request.Content.Headers.Remove(headerKey);
			}
			else
			{
				request.Headers.Remove(headerKey);
			}
		}
	}
}
