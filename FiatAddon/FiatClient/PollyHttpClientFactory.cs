using AwsSignatureVersion4;
using Flurl.Http.Configuration;
using Polly;
using Serilog;

namespace FiatChamp;

public class PollyHttpClientFactory : DefaultHttpClientFactory
{
  private class PolicyHandler : DelegatingHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      var retryPolicy = Policy
        .HandleResult<HttpResponseMessage>(m => !m.IsSuccessStatusCode)
        .Or<HttpRequestException>(e => true)
        .WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) },
          (delegateResult, time, retryCount, ctx) =>
          {
            var ex = delegateResult.Exception as HttpRequestException;
            var result = delegateResult.Result?.StatusCode.ToString() ?? ex?.StatusCode.ToString() ?? ex?.Message;
            
            Log.Warning("Error connecting to {0}. Result: {1}. Retrying in {2}", 
              request.RequestUri, result, time);
          });

      return retryPolicy.ExecuteAsync(ct => { return base.SendAsync(request, ct); }, cancellationToken);
    }
  }

  public override HttpMessageHandler CreateMessageHandler()
  {
    return new PolicyHandler()
    {
      InnerHandler = base.CreateMessageHandler()
    };
  }
}