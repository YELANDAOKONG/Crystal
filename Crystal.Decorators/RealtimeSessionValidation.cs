using Crystal.Pipelines;
using Crystal.Realtime;

namespace Crystal.Decorators;

/// <summary>Provides opt-in preflight for declared live-session capabilities.</summary>
public static class RealtimeSessionValidation
{
    /// <summary>
    /// Rejects individually undeclared session requirements before opening a
    /// provider session. Conditional model rules remain adapter-owned.
    /// </summary>
    /// <param name="capabilities">The configured client's declared support.</param>
    /// <returns>Middleware for a live-session open operation.</returns>
    public static AsyncMiddleware<RealtimeSessionRequest, IRealtimeMediaSession>
        RequireDeclaredCapabilities(RealtimeSessionCapabilities capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);

        return next =>
        {
            ArgumentNullException.ThrowIfNull(next);

            return (request, cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(request);
                cancellationToken.ThrowIfCancellationRequested();

                if (request.OutputModalities.Any(modality =>
                    !capabilities.Outputs.Any(output => output.Modality == modality)))
                {
                    throw new ArgumentException(
                        "The realtime session requests an undeclared output modality.",
                        nameof(request));
                }

                if ((request.TurnMode == RealtimeTurnMode.Automatic
                        && !capabilities.SupportsAutomaticTurnDetection)
                    || (request.TurnMode == RealtimeTurnMode.Explicit
                        && !capabilities.SupportsExplicitTurnEnd))
                {
                    throw new ArgumentException(
                        "The realtime session requests an undeclared turn mode.",
                        nameof(request));
                }

                if (request.Tools.Count > 0 && !capabilities.SupportsTools)
                {
                    throw new ArgumentException(
                        "The realtime session requests undeclared tool support.",
                        nameof(request));
                }

                if (request.Reasoning is not null
                    && !capabilities.SupportsReasoningOptions)
                {
                    throw new ArgumentException(
                        "The realtime session requests undeclared reasoning support.",
                        nameof(request));
                }

                return next(request, cancellationToken);
            };
        };
    }
}
