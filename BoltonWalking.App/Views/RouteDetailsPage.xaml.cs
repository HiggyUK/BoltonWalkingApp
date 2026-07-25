using BoltonWalking.App.ViewModels;

namespace BoltonWalking.App.Views;

public partial class RouteDetailsPage : ContentPage
{
    private IDispatcherTimer? carouselTimer;

    public RouteDetailsPage(RouteDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        carouselTimer = Dispatcher.CreateTimer();
        carouselTimer.Interval = TimeSpan.FromSeconds(3.5);
        carouselTimer.Tick += OnCarouselTick;
        carouselTimer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (carouselTimer is null) return;
        carouselTimer.Stop();
        carouselTimer.Tick -= OnCarouselTick;
        carouselTimer = null;
    }

    private void OnCarouselTick(object? sender, EventArgs e)
    {
        var count = PhotoCarousel.ItemsSource is ICollection<string> items ? items.Count : 0;
        if (count == 0) return;

        PhotoCarousel.Position = (PhotoCarousel.Position + 1) % count;
    }
}
