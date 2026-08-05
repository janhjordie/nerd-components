using TheNerdCollective.Blazor.FeatureFeedback;
using Xunit;

namespace TheNerdCollective.Blazor.FeatureFeedback.Tests;

public sealed class InMemoryFeatureFeedbackStoreTests
{
    [Fact]
    public async Task Create_and_vote_requires_user()
    {
        var store = new InMemoryFeatureFeedbackStore();
        var anon = await store.CreateAsync(new CreateFeatureIdeaRequest("Title here", "A longer description", "A"), "");
        Assert.False(anon.Success);
        Assert.Equal("unauthenticated", anon.ErrorCode);
    }

    [Fact]
    public async Task Create_auto_upvotes_and_toggle_works()
    {
        var store = new InMemoryFeatureFeedbackStore();
        var created = await store.CreateAsync(
            new CreateFeatureIdeaRequest("Dark mode export", "Please add CSV export for consent logs.", "Ada"),
            "user-1");

        Assert.True(created.Success);
        Assert.NotNull(created.Idea);
        Assert.Equal(1, created.Idea!.VoteCount);
        Assert.True(created.Idea.VotedByCurrentUser);

        var toggled = await store.ToggleVoteAsync(created.Idea.Id, "user-2");
        Assert.True(toggled.Success);
        Assert.Equal(2, toggled.Idea!.VoteCount);

        var removed = await store.ToggleVoteAsync(created.Idea.Id, "user-2");
        Assert.Equal(1, removed.Idea!.VoteCount);
    }

    [Fact]
    public async Task List_sorts_by_votes()
    {
        var store = new InMemoryFeatureFeedbackStore();
        var a = await store.CreateAsync(new CreateFeatureIdeaRequest("Idea Axx", "Description for A here", "A"), "u1");
        var b = await store.CreateAsync(new CreateFeatureIdeaRequest("Idea Bxx", "Description for B here", "B"), "u2");
        await store.ToggleVoteAsync(b.Idea!.Id, "u3");
        await store.ToggleVoteAsync(b.Idea.Id, "u4");

        var list = await store.ListAsync(FeatureIdeaSort.MostVoted);
        Assert.Equal(b.Idea.Id, list[0].Id);
        Assert.Equal(a.Idea!.Id, list[1].Id);
    }

    [Fact]
    public async Task Update_sets_status_and_planned_release()
    {
        var store = new InMemoryFeatureFeedbackStore();
        var created = await store.CreateAsync(
            new CreateFeatureIdeaRequest("Idea Cxx", "Description for C here", "C"),
            "u1");

        var updated = await store.UpdateAsync(
            created.Idea!.Id,
            new UpdateFeatureIdeaRequest(FeatureIdeaStatus.Planned, new DateOnly(2026, 9, 1)));

        Assert.True(updated.Success);
        Assert.Equal(FeatureIdeaStatus.Planned, updated.Idea!.Status);
        Assert.Equal(new DateOnly(2026, 9, 1), updated.Idea.PlannedReleaseDate);
    }
}
