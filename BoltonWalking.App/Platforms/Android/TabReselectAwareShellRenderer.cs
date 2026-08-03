using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace BoltonWalking.App.Platforms.Android;

// Shell's default ShellItemRenderer leaves OnTabReselected() empty, so
// tapping an already-selected TabBar item does nothing even when a page is
// pushed on top of that tab's root (e.g. Committee/SafetyGuide pushed from
// the More tab) - unlike native Android/iOS tab bar convention, where
// reselecting a tab pops back to its root. Overriding it here restores that.
public class TabReselectAwareShellRenderer : ShellRenderer
{
    protected override IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem)
    {
        return new TabReselectAwareShellItemRenderer(this);
    }
}

public class TabReselectAwareShellItemRenderer : ShellItemRenderer
{
    public TabReselectAwareShellItemRenderer(IShellContext shellContext) : base(shellContext)
    {
    }

    protected override void OnTabReselected(ShellSection shellSection)
    {
        base.OnTabReselected(shellSection);
        _ = shellSection.Navigation.PopToRootAsync();
    }
}
