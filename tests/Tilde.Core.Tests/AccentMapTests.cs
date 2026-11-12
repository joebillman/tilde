using Tilde.Core;

namespace Tilde.Core.Tests;

/// <summary>AccentMap tests A1–A5 from docs/TESTING.md §2.</summary>
public class AccentMapTests
{
    private readonly AccentMap _es = AccentMap.Load("es");

    [Theory]
    [InlineData('a', "á")]
    [InlineData('e', "é")]
    [InlineData('i', "í")]
    [InlineData('o', "ó")]
    [InlineData('u', "úü")]
    [InlineData('n', "ñ")]
    [InlineData('?', "¿")]
    [InlineData('!', "¡")]
    public void A1_Spanish_HasVariantsFromProductSpec(char typed, string variants)
    {
        Assert.Equal(variants.ToCharArray(), _es.GetVariants(typed));
    }

    [Theory]
    [InlineData('A', "Á")]
    [InlineData('E', "É")]
    [InlineData('U', "ÚÜ")]
    [InlineData('N', "Ñ")]
    public void A2_Uppercase_IsDerived(char typed, string variants)
    {
        Assert.Equal(variants.ToCharArray(), _es.GetVariants(typed));
    }

    [Fact]
    public void A3_Punctuation_GetsNoExtraEntries()
    {
        var expected = "aeiounAEIOUN?!".ToHashSet();
        Assert.Equal(expected, _es.MappedCharacters.ToHashSet());
    }

    [Theory]
    [InlineData('x')]
    [InlineData('1')]
    [InlineData(' ')]
    public void A4_UnmappedCharacter_HasNoVariants(char typed)
    {
        Assert.False(_es.IsMapped(typed));
        Assert.Empty(_es.GetVariants(typed));
    }

    [Theory]
    [InlineData("é")]   // e + combining acute: looks like é, but two units
    [InlineData("́")]    // a lone combining mark
    [InlineData("😀")]        // a surrogate pair
    public void A5_MultiUnitVariant_IsRejected(string badVariant)
    {
        string json = $$"""{ "code": "xx", "map": { "e": ["{{badVariant}}"] } }""";

        var ex = Assert.Throws<InvalidDataException>(() => AccentMap.FromJson(json));
        Assert.Contains("exactly one precomposed character", ex.Message);
    }

    [Fact]
    public void Load_UnknownLanguage_Throws()
    {
        Assert.Throws<ArgumentException>(() => AccentMap.Load("zz"));
    }
}
