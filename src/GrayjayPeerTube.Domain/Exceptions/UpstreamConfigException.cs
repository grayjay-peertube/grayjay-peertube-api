namespace GrayjayPeerTube.Domain.Exceptions;

public class UpstreamConfigException : Exception
{
    public UpstreamConfigException(string message) : base(message)
    {
    }

    public UpstreamConfigException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
