# Enhancement backlog

Ideas raised but not yet scheduled for implementation.

## Routes page

### 1. Match Android pin style to iOS (name label under the pin)
On iOS, route pins show the route name in a label underneath the marker.
On Android, pins are bare (colour-coded teardrop only, no label) - you have
to tap a pin to see which route it is. Bring Android in line with iOS so
both platforms show the name under the pin.

- Android: `Microsoft.Maui.Maps` pins don't support a persistent visible
  label out of the box (only the tap-triggered info window, which
  `RoutesPage.xaml.cs` already suppresses via `HideInfoWindow = true`).
  Getting a permanent name label likely means either a custom
  `MapPinHandler` mapping that swaps in a `BitmapDescriptorFactory.FromView`
  (render a small XAML/View to a bitmap with the name text baked in,
  similar to the existing difficulty-colour mapping in `MauiProgram.cs`),
  or dropping to a custom marker renderer.
- Where: `MauiProgram.cs` (`MapPinHandler.Mapper` mapping), `RoutesPage.xaml.cs` (`BuildPins()`).

### 2. Map/List view toggle next to the search bar - Done
Implemented: an icon button beside the search bar toggles between the map
and a scrollable `CollectionView` list (difficulty dot, name, distance/stats),
sharing the same search/difficulty filter and tapping through to the same
route details page. See `RoutesPage.xaml`, `RoutesViewModel.cs` (`IsListView`).

## Theming

### 3. Real dark-mode colour palette
The app currently forces `UserAppTheme = AppTheme.Light` app-wide
(`App.xaml.cs`) as a stopgap after a bug report: several controls hardcode
literal White/Black colours (Routes popup + search bar backgrounds,
About page title text) that don't move with the system theme the way
`Label`/`SearchBar` default text colour does, so a phone in system Dark
Mode got invisible white-on-white or black-on-black text. Worth doing this
properly instead: a real dark variant via `AppThemeBinding` throughout
`Colors.xaml`/`Styles.xaml`, then removing the light-mode pin.

Suggested dark palette, extending `Resources/Styles/Colors.xaml`:

| Key | Light (current) | Dark (proposed) | Use |
|---|---|---|---|
| `Primary` | `#E0232B` | `#FF4550` | Brand red - lightened slightly for contrast against dark backgrounds |
| `Secondary` | `#000000` | `#F2F2F2` | Primary text (About title, etc.) |
| `Charcoal` | `#404042` | `#D0D0D2` | Secondary headings |
| `SurfaceMuted` | `#F5F5F5` | `#1E1E20` | Card/section backgrounds |
| `TextMuted` | `#5B5B5B` | `#A0A0A3` | Captions, muted body text |
| *(new)* `PageBackground` | `#FFFFFF` | `#121212` | ContentPage background |
| *(new)* `Surface` | `#FFFFFF` | `#1E1E1E` | Popups, cards, search bar - replaces literal `BackgroundColor="White"` |
| *(new)* `Divider` | `#E0E0E0` | `#333335` | BoxView separators, borders |
| *(new)* `TabBarUnselected` | `#6B6B6B` | `#8E8E93` | Shell tab bar unselected icons/text |

Notes:
- `Primary` red at `#E0232B` already has decent contrast on black (~5:1),
  but `#FF4550` is a touch lighter/warmer so button text and the colour
  itself both stay comfortable on dark surfaces.
- Every hardcoded `BackgroundColor="White"` / `TextColor="Black"` in the
  XAML (Routes popup + search bar, About page, difficulty filter chips,
  etc.) needs auditing and swapping for `{AppThemeBinding Light=..., Dark=...}`
  or the new `Surface`/`PageBackground` resources - the bug wasn't one bad
  control, it was that dark mode was never designed for anywhere in the app.
- Once done, remove the `UserAppTheme = AppTheme.Light` pin in `App.xaml.cs`.
