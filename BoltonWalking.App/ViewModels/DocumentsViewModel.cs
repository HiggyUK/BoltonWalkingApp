using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BoltonWalking.App.Models;
using BoltonWalking.App.Services;

namespace BoltonWalking.App.ViewModels;

/// <summary>
/// Reads policy/document PDFs from the club's Firestore database (collection
/// "documents"), uploaded via the admin page's Documents tab, and opens or
/// saves them on tap via IFileDownloadService - same as route GPX/PDF files.
/// </summary>
public partial class DocumentsViewModel : ObservableObject
{
    private readonly FirestoreClient firestoreClient;
    private readonly IFileDownloadService fileDownloadService;

    public ObservableCollection<ClubDocument> Documents { get; } = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;

    [ObservableProperty]
    private string? statusMessage;

    public DocumentsViewModel(FirestoreClient firestoreClient, IFileDownloadService fileDownloadService)
    {
        this.firestoreClient = firestoreClient;
        this.fileDownloadService = fileDownloadService;
    }

    [RelayCommand]
    private async Task LoadDocumentsAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var documents = await firestoreClient.GetCollectionAsync("documents");
            var items = documents.Select(d => new ClubDocument
            {
                Id = d.Id,
                Title = FirestoreClient.GetString(d.Fields, "title"),
                FileName = FirestoreClient.GetString(d.Fields, "fileName", "document.pdf"),
                Url = FirestoreClient.GetString(d.Fields, "url"),
                // Stored as a plain "yyyy-MM-dd" date by the admin page.
                LastUpdated = DateTime.TryParseExact(FirestoreClient.GetString(d.Fields, "lastUpdated"), "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null
            })
            .Where(d => !string.IsNullOrWhiteSpace(d.Url))
            .OrderBy(d => d.Title, StringComparer.OrdinalIgnoreCase);

            Documents.Clear();
            foreach (var item in items)
                Documents.Add(item);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Couldn't reach the server - showing the last documents loaded. ({ex.Message})";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenDocumentAsync(ClubDocument document)
    {
        try
        {
            StatusMessage = $"Opening {document.Title}...";
            await fileDownloadService.OpenOrSaveAsync(document.Url, document.FileName);
            StatusMessage = null;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Couldn't open {document.Title}: {ex.Message}";
        }
    }
}
