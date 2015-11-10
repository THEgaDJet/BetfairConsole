using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;

namespace BetfairConsole
{
  class BetFairRestClient : IDisposable
  {    
    protected HttpClient client = new HttpClient();
    protected ServicePoint servicePoint = ServicePointManager.FindServicePoint(new Uri(Properties.Settings.Default.BetFairExchangeUri));

    public BetFairRestClient()
    {
      SetClientBaseAddress(Properties.Settings.Default.BetFairExchangeUri);
      SetClientDefaultHeaders();
      this.servicePoint.Expect100Continue = false;    
    }

    protected void SetClientBaseAddress(string address)
    {
      client.BaseAddress = new Uri(address);
    }

    protected void SetClientDefaultHeaders()
    {
      client.DefaultRequestHeaders.Accept.Clear();
      client.DefaultRequestHeaders.Add("X-Application", Properties.Settings.Default.XApplication);
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public BetFairRestClient(string baseAddress) : this()
    { client.BaseAddress = new Uri(baseAddress); }

    public void AddSessionToken(string sessionToken)
    {
      if (client != null)
      { client.DefaultRequestHeaders.Add("X-Authentication", sessionToken); }
    }

    public string GetPostResponseTyped<T>(string apiUrlParams, T body)
    {
      var httpResponse = client.PostAsJsonAsync(apiUrlParams, body).Result;
      httpResponse.EnsureSuccessStatusCode();

      return httpResponse.Content.ReadAsStringAsync().Result;
    }

    public string GetPostResponseFormEncoded(string apiUrlParams, FormUrlEncodedContent body)
    {
      var httpResponse = client.PostAsync(apiUrlParams, body).Result;
      httpResponse.EnsureSuccessStatusCode();

      return httpResponse.Content.ReadAsStringAsync().Result;
    }

    #region IDisposable Support
    private bool disposedValue = false; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects).
        }

        // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
        // TODO: set large fields to null.

        disposedValue = true;
      }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~BetFairRestClient() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
      // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
      Dispose(true);
      // TODO: uncomment the following line if the finalizer is overridden above.
      // GC.SuppressFinalize(this);
    }
    #endregion

  }

  class BetFairRestAuthClient : BetFairRestClient
  {
    public BetFairRestAuthClient()
    {
      var handler = new WebRequestHandler();
      var cert = new X509Certificate2(Properties.Settings.Default.CertPath, "");
      handler.ClientCertificates.Add(cert);
      client = new HttpClient(handler);
      SetClientBaseAddress(Properties.Settings.Default.BetFairLoginUri);
      SetClientDefaultHeaders();
    }
  }
}
