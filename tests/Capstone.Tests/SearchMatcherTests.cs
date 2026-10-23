using SharpQuest.Contracts;
using Xunit;

namespace Capstone.Tests;

public class SearchMatcherTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   \t\n")]
    public void Empty_or_whitespace_query_matches_any_non_null_text(string query)
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Queen - Bohemian Rhapsody", query));
    }

    [Fact]
    public void Matching_ignores_case()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Pink Floyd", "PINK"));
        Assert.True(matcher.Matches("queen", "QUEEN"));
    }

    [Theory]
    [InlineData("Beyoncé Halo", "beyonce halo")]
    [InlineData("Beyonce Halo", "beyoncé halo")]
    [InlineData("Phoenix Şir de cocori", "Sir de cocori")]
    [InlineData("Phoenix Sir de cocori", "şir de cocori")]
    public void Matching_ignores_diacritics_both_ways(string text, string query)
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches(text, query));
    }

    [Fact]
    public void Every_query_word_must_be_present()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Queen Bohemian Rhapsody", "Queen Rhapsody"));
        Assert.False(matcher.Matches("Queen Bohemian Rhapsody", "Queen Beatles"));
    }

    [Fact]
    public void Word_order_does_not_matter()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Queen Bohemian Rhapsody", "rhapsody queen"));
        Assert.True(matcher.Matches("Pink Floyd Dark Side", "side dark floyd"));
    }

    [Fact]
    public void Word_can_occur_inside_another_word()
    {
        var matcher = SearchMatcher.Create();
        Assert.True(matcher.Matches("Supermassive Black Hole", "massive"));
        Assert.True(matcher.Matches("Technologic", "logic"));
    }

    [Fact]
    public void Null_text_returns_false()
    {
        var matcher = SearchMatcher.Create();
        Assert.False(matcher.Matches(null, "queen"));
        Assert.False(matcher.Matches(null, ""));
    }

    [Fact]
    public void Null_query_throws_ArgumentNullException()
    {
        var matcher = SearchMatcher.Create();
        Assert.Throws<ArgumentNullException>(() => matcher.Matches("queen", null!));
    }
}