using StarshipApi.Controllers;

public class SafeArrayTests
{
    [Fact]
    public void SafeArray_ReturnsValidJsonArray()
    {
        string json = "[\"a\",\"b\"]";

        var method = typeof(StarshipsController)
            .GetMethod("SafeArray", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        var result = (string[])method.Invoke(null, new object?[] { json! });

        Assert.Equal(new[] { "a", "b" }, result);
    }

    [Fact]
    public void SafeArray_InvalidJson_ReturnsEmpty()
    {
        string json = "not valid";

        var method = typeof(StarshipsController)
            .GetMethod("SafeArray", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        var result = (string[])method.Invoke(null, new object?[] { json! });

        Assert.Empty(result);
    }
}
