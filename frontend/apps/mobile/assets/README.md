# GymHero Mobile Assets

This directory contains all image assets for the mobile app.

## Required Assets

### App Icon
- **icon.png** - 1024x1024px PNG
  - Used as the base app icon
  - Should be a simple, recognizable design
  - No transparency
  - Recommended: Dumbb ell or trophy icon with "GH" text

### Adaptive Icon (Android)
- **adaptive-icon.png** - 1024x1024px PNG
  - Foreground layer for Android adaptive icons
  - Should work on any background color
  - Center-weighted design (safe area: 66% of canvas)

### Splash Screen
- **splash.png** - 2048x2048px PNG
  - Displayed while app is loading
  - Should match app's branding
  - Keep important content in center 1280x1280px

### Favicon (Web)
- **favicon.png** - 48x48px PNG
  - Small icon for browser tabs
  - Simplified version of main icon

## Design Guidelines

### Color Palette
- Primary: #3b82f6 (Blue)
- Background: #0f172a (Dark Blue)
- Accent: #8b5cf6 (Purple)

### Style
- Modern, clean design
- Fitness-focused imagery
- Dark mode optimized
- High contrast for visibility

## Creating Assets

### Option 1: Design Tools
Use Figma, Adobe Illustrator, or Sketch to create:
1. Start with 1024x1024px canvas
2. Design your icon
3. Export at required sizes

### Option 2: Online Generators
- [App Icon Generator](https://www.appicon.co/)
- [Icon Kitchen](https://icon.kitchen/)
- [MakeAppIcon](https://makeappicon.com/)

### Option 3: Template
We recommend starting with this simple design:

```
Background: Gradient from #3b82f6 to #8b5cf6
Icon: White dumbbell or trophy
Text: "GH" in bold sans-serif font
Style: Rounded square with 20% radius
```

## Placeholder Assets

Currently, this directory contains placeholder assets. To replace them:

1. Create your custom assets following the guidelines above
2. Name them exactly as specified
3. Replace the files in this directory
4. Run `expo prebuild` to regenerate native projects

## Platform-Specific Icons

### iOS
Expo automatically generates all required iOS icon sizes from `icon.png`:
- App Icon (multiple sizes for different devices)
- Spotlight Icon
- Settings Icon
- Notification Icon

### Android
Expo generates adaptive icons and legacy icons from:
- `adaptive-icon.png` - Foreground layer
- App background color (set in app.json)

## Testing Icons

1. **iOS Simulator**: Icons appear in home screen
2. **Android Emulator**: Check both light and dark themes
3. **Physical Device**: Test on actual devices for best results

## Useful Resources

- [Expo Icon Requirements](https://docs.expo.dev/develop/user-interface/app-icons/)
- [Human Interface Guidelines (iOS)](https://developer.apple.com/design/human-interface-guidelines/app-icons)
- [Material Design Icons (Android)](https://m3.material.io/styles/icons)

## Need Help?

For custom icon design, consider:
- Hiring a designer on Fiverr or Upwork
- Using AI tools like Midjourney or DALL-E
- Adapting free icons from The Noun Project or Flaticon

---

**Note**: The current placeholder assets are for development only. Replace them before publishing to app stores.
