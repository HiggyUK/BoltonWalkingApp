using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace BoltonWalking.App.Platforms.iOS;

// Same gap as Android's TabReselectAwareShellRenderer: Shell's TabBar doesn't
// pop a tab's own navigation stack back to root when you tap the already-
// selected tab. On iOS, ShouldSelectViewController fires for reselection too
// (unlike Android, where it's suppressed by default), but the base
// ShellItemRenderer doesn't check for it - so we wrap the closure it installs
// in ViewDidLoad with our own check.
public class TabReselectAwareShellRenderer : ShellRenderer
{
    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem item)
    {
        return new TabReselectAwareShellItemRenderer(this)
        {
            ShellItem = item
        };
    }
}

public class TabReselectAwareShellItemRenderer : ShellItemRenderer
{
    public TabReselectAwareShellItemRenderer(IShellContext context) : base(context)
    {
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        var originalShouldSelect = ShouldSelectViewController;
        ShouldSelectViewController = (tabController, viewController) =>
        {
            if (viewController == tabController.SelectedViewController && ViewControllers is not null)
            {
                var index = Array.IndexOf(ViewControllers, viewController);
                var items = ((IShellItemController)ShellItem).GetItems();
                if (index >= 0 && index < items.Count)
                {
                    _ = items[index].Navigation.PopToRootAsync();
                }
            }

            return originalShouldSelect?.Invoke(tabController, viewController) ?? true;
        };
    }
}
