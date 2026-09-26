using Crystal.Pipelines;

namespace Crystal.Tests;

public sealed class AsyncPipelineTests
{
    [Fact]
    public async Task MiddlewareRunsInDeclaredOrderAndPreservesRequestAndToken()
    {
        var transitions = new List<string>();
        var request = new object();
        using var source = new CancellationTokenSource();

        AsyncOperation<object, object> terminal = (actualRequest, token) =>
        {
            Assert.Same(request, actualRequest);
            Assert.Equal(source.Token, token);
            transitions.Add("terminal");
            return Task.FromResult(actualRequest);
        };

        AsyncMiddleware<object, object> first = next => async (actualRequest, token) =>
        {
            transitions.Add("first before");
            var response = await next(actualRequest, token).ConfigureAwait(false);
            transitions.Add("first after");
            return response;
        };

        AsyncMiddleware<object, object> second = next => async (actualRequest, token) =>
        {
            transitions.Add("second before");
            var response = await next(actualRequest, token).ConfigureAwait(false);
            transitions.Add("second after");
            return response;
        };

        var middleware = new List<AsyncMiddleware<object, object>> { first, second };
        var pipeline = new AsyncPipeline<object, object>(terminal, middleware);
        middleware.Clear();

        var result = await pipeline.InvokeAsync(request, source.Token);

        Assert.Same(request, result);
        string[] expected =
            ["first before", "second before", "terminal", "second after", "first after"];
        Assert.Equal(
            expected,
            transitions);
    }

    [Fact]
    public async Task TerminalFailureIsNotConvertedToAResponse()
    {
        var failure = new InvalidOperationException("Terminal failed.");
        var pipeline = new AsyncPipeline<string, string>(
            (_, _) => Task.FromException<string>(failure),
            []);

        var observed = await Assert.ThrowsAsync<InvalidOperationException>(
            () => pipeline.InvokeAsync("input"));

        Assert.Same(failure, observed);
    }

    [Fact]
    public void ConstructionRejectsNullMiddlewareEntry()
    {
        AsyncMiddleware<string, string>[] middleware = [null!];

        var failure = Assert.Throws<ArgumentException>(() =>
            new AsyncPipeline<string, string>(
                (request, _) => Task.FromResult(request),
                middleware));

        Assert.Equal("middleware", failure.ParamName);
    }
}
