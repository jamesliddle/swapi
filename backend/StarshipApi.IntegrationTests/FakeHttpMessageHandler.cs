using System.Net;
using System.Text;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    public string ResponseJson { get; set; } =
        """
        {
            "results": [
                { "title": "A New Hope", "episode_id": 4 },
                { "title": "The Empire Strikes Back", "episode_id": 5 }
            ]
        }
        """;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string url = request.RequestUri!.ToString().ToLower();

        if (url.Contains("films"))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json")
            });
        }

        if (url.Contains("people"))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    { "results": [{ "name": "Luke Skywalker" }] }
                    """,
                    Encoding.UTF8,
                    "application/json")
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
