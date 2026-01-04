namespace GrayjayPeerTube.Domain.Exceptions;

public class PeerTubeValidationException : Exception
{
    public PeerTubeValidationException(string message) : base(message)
    {
    }

    public PeerTubeValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
