using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BoltonWalking.App.Models;
using BoltonWalking.App.Services;

namespace BoltonWalking.App.ViewModels;

/// <summary>
/// Reads committee members from the club's Firestore database (collection
/// "committeeMembers"). The club can add/edit members directly in the
/// Firebase console and the app picks them up on next load.
/// </summary>
public partial class CommitteeViewModel : ObservableObject
{
    private readonly FirestoreClient firestoreClient;

    public ObservableCollection<CommitteeMember> Members { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    public CommitteeViewModel(FirestoreClient firestoreClient)
    {
        this.firestoreClient = firestoreClient;
    }

    [RelayCommand]
    private async Task LoadMembersAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            Members.Clear();

            var documents = await firestoreClient.GetCollectionAsync("committeeMembers");
            var members = documents.Select(d => new CommitteeMember
            {
                Id = int.TryParse(d.Id, out var id) ? id : 0,
                Name = FirestoreClient.GetString(d.Fields, "name"),
                Role = FirestoreClient.GetString(d.Fields, "role"),
                Quote = FirestoreClient.GetNullableString(d.Fields, "quote"),
                Bio = FirestoreClient.GetString(d.Fields, "bio"),
                PhotoUrl = FirestoreClient.GetNullableString(d.Fields, "photoUrl")
            }).OrderBy(m => m.Id);

            foreach (var member in members)
                Members.Add(member);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
