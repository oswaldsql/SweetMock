namespace SweetMock;

using System.Collections.Generic;
using System.Linq;

public class Arguments
{
    private readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

    public static Arguments Empty => new Arguments();

    public static Arguments With(string key, object? value) =>
        new Arguments().And(key, value);

    public Arguments And(string key, object? value)
    {
        this.values[key] = value;
        return this;
    }

    public object? this[string key]
    {
        get
        {
            if (this.values.TryGetValue(key, out var value))
            {
                return value;
            }

            throw new KeyNotFoundException(key);
        }
    }

    public override string ToString() =>
        this.values.Count == 0 ? "" : string.Join(", ", this.values.Select(t => $"{t.Key} : '{t.Value}'"));
}