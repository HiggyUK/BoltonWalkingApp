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
            foreach (var (_, fields) in documents)
            {
                Members.Add(new CommitteeMember
                {
                    Name = FirestoreClient.GetString(fields, "name"),
                    Role = FirestoreClient.GetString(fields, "role"),
                    Quote = FirestoreClient.GetNullableString(fields, "quote"),
                    Bio = FirestoreClient.GetString(fields, "bio"),
                    PhotoUrl = FirestoreClient.GetNullableString(fields, "photoUrl")
                });
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
