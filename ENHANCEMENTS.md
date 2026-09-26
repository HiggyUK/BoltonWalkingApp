# Enhancement backlog

Ideas raised but not yet scheduled for implementation.

## Admin HTML page
### 1. Upload of images / GPX / Risk Assessment - Done
Implemented: photos and downloadable files (GPX/risk assessment PDF) can
now be uploaded directly from the admin page to Firebase Storage instead
of requiring a URL to be pasted in first - the paste-a-URL option stays as
a fallback. Existing Wix-hosted files were migrated to Storage via
`tools/Migrate-BwoasFilesToStorage.ps1`.

## Bookings page


### 2. Booking Feedback
When a booking has passed can we move to the bottom of the list and change the button to Feedback, this should open a form that asks for feedback on the work, perhaps a rating and optional comments with a number of questions, with optional recording of name and contact details

Question could be (or similar)
How did you find this walk
How did you find the organisation of the walk
How did you find booking the walk

It should then if possible record this data in the database and send an email in the background

## Routes page



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
