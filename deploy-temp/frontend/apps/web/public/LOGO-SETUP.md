# TaktIQ Logo Setup

## Required Logo Files

To complete the branding, please add the following logo files to this directory:

### Favicon Files
- `favicon.ico` - 32x32 or 16x16 ICO format for browser tab icon
- `apple-icon.png` - 180x180 PNG format for iOS home screen icon

### Logo Files for Header/Menu (Theme-Aware - PNG Format)

**IMPORTANT**: You need TWO PNG logo files - one for light theme and one for dark theme.

#### Light Theme Logo (shown on light backgrounds)
- `taktiq-logo-light.png` - Full logo for light theme (dark/colored design with "TaktIQ" text)

#### Dark Theme Logo (shown on dark backgrounds)
- `taktiq-logo-dark.png` - Full logo for dark theme (light/white design with "TaktIQ" text)

**Recommended Specifications**:
- **Dimensions**: Width ~500-600px, Height ~140-170px (or maintain your logo's aspect ratio ~3.5:1)
- **Format**: PNG with transparent background
- **DPI**: 144 DPI or higher for crisp display on retina screens
- **Colors**:
  - Light theme version: Use your primary brand colors or darker tones (black, dark blue, etc.)
  - Dark theme version: White (#FFFFFF) or very light colors (#F5F5F5)
- **Include Text**: Both logos should include the "TaktIQ" text/wordmark as part of the logo

## Current Implementation

### Favicon
The favicon is referenced in `src/app/layout.tsx`:
```typescript
icons: {
  icon: '/favicon.ico',
  apple: '/apple-icon.png',
}
```

### Header/Menu Logo (Theme-Aware)
A theme-aware logo component has been created at `src/components/taktiq-logo.tsx`.

The layout files (`src/app/(app)/layout.tsx`) have been updated to use the `TaktIQLogo` component, which automatically switches between light and dark logo variants based on the current theme.

**How it works**:
- When theme is light → shows `taktiq-logo-light.png` (dark logo with text)
- When theme is dark → shows `taktiq-logo-dark.png` (white logo with text)
- When theme is system → automatically uses light or dark based on OS preference
- Logo includes the "TaktIQ" wordmark, so no additional text is shown in the menu

**Display sizes**:
- Desktop sidebar: 140x40px
- Mobile sidebar: 120x34px
- Mobile top header: 100x28px

**No additional code changes needed** - just add the 2 PNG logo files to this directory!

## File Placement
All files should be placed in this `frontend/apps/web/public/` directory.
Next.js will automatically serve them from the root URL path.

Example:
- `frontend/apps/web/public/favicon.ico` → accessible at `/favicon.ico`
- `frontend/apps/web/public/logo.svg` → accessible at `/logo.svg`
