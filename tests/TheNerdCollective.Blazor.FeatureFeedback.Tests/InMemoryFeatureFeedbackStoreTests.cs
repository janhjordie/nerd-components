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

    [Fact]
    public async Task Soft_delete_hides_from_public_list_and_restore_works()
    {
        var store = new InMemoryFeatureFeedbackStore();
        var created = await store.CreateAsync(
            new CreateFeatureIdeaRequest("Idea Dxx", "Description for D here", "D"),
            "u1");

        var soft = await store.SoftDeleteAsync(created.Idea!.Id);
        Assert.True(soft.Success);
        Assert.Equal(FeatureIdeaStatus.Deleted, soft.Idea!.Status);

        var publicList = await store.ListAsync();
        Assert.DoesNotContain(publicList, idea => idea.Id == created.Idea.Id);

        var adminList = await store.ListAsync(includeDeleted: true);
        Assert.Contains(adminList, idea => idea.Id == created.Idea.Id && idea.Status == FeatureIdeaStatus.Deleted);

        var restored = await store.RestoreAsync(created.Idea.Id);
        Assert.True(restored.Success);
        Assert.Equal(FeatureIdeaStatus.Open, restored.Idea!.Status);
        Assert.Contains(await store.ListAsync(), idea => idea.Id == created.Idea.Id);
    }

    [Fact]
    public async Task Hard_delete_removes_permanently()
    {
        var store = new InMemoryFeatureFeedbackStore();
        var created = await store.CreateAsync(
            new CreateFeatureIdeaRequest("Idea Exx", "Description for E here", "E"),
            "u1");

        var hard = await store.HardDeleteAsync(created.Idea!.Id);
        Assert.True(hard.Success);

        var adminList = await store.ListAsync(includeDeleted: true);
        Assert.DoesNotContain(adminList, idea => idea.Id == created.Idea.Id);
        Assert.False((await store.HardDeleteAsync(created.Idea.Id)).Success);
    }

    [Fact]
    public async Task Toggle_vote_enforces_max_votes_per_user()
    {
        var store = new InMemoryFeatureFeedbackStore();
        var first = await store.CreateAsync(new CreateFeatureIdeaRequest("Idea F1", "Description for F1 here", "F"), "u1");
        var second = await store.CreateAsync(new CreateFeatureIdeaRequest("Idea F2", "Description for F2 here", "F"), "u2");

        var firstVote = await store.ToggleVoteAsync(first.Idea!.Id, "u3", maxVotesPerUser: 1);
        Assert.True(firstVote.Success);

        var blocked = await store.ToggleVoteAsync(second.Idea!.Id, "u3", maxVotesPerUser: 1);
        Assert.False(blocked.Success);
        Assert.Equal("vote-limit-reached", blocked.ErrorCode);
    }

    [Fact]
    public async Task Toggle_vote_can_block_withdraw()
    {
        var store = new InMemoryFeatureFeedbackStore();
        var created = await store.CreateAsync(new CreateFeatureIdeaRequest("Idea G1", "Description for G1 here", "G"), "u1");

        var blocked = await store.ToggleVoteAsync(created.Idea!.Id, "u1", allowWithdraw: false);
        Assert.False(blocked.Success);
        Assert.Equal("withdraw-disabled", blocked.ErrorCode);
    }
}
