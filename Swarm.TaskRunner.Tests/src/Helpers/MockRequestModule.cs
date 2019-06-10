using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Swarm.TaskRunner.Modules;

namespace Swarm.TaskRunner.Tests.Helpers {
  public class MockRequestModule : RequestModule {
    private readonly HttpResponseMessage response;

    public MockRequestModule(HttpResponseMessage response) {
      this.response = response;
    }

    protected override HttpClient CreateHttpClient() {
      using (var handler = new MockHttpMessageHandler(response)) {
        return new HttpClient(handler);
      }
    }
  }

  public class MockHttpMessageHandler : HttpMessageHandler {
    private readonly HttpResponseMessage response;

    public MockHttpMessageHandler(HttpResponseMessage response) {
      this.response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
      return Task.FromResult(response);
    }
  }
}
