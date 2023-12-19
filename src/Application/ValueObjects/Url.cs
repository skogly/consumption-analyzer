namespace Application.ValueObjects;

public class Url
{
    private readonly string _value;

    public Url(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("URL cannot be null or whitespace.", nameof(value));
        }

        if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
        {
            throw new ArgumentException("URL is not well-formed.", nameof(value));
        }

        _value = value;
    }

    public override string ToString()
    {
        return _value;
    }
}