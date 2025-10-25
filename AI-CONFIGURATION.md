# AI Workout Generation Configuration

The GymHero API now supports multiple AI providers for intelligent workout and plan generation with automatic fallback.

## Supported AI Providers

### 1. Google Gemini (Recommended for Free Tier)
- **Model**: Gemini 1.5 Flash
- **Cost**: FREE tier available with generous limits
- **Speed**: Very fast
- **Get API Key**: https://aistudio.google.com/app/apikey

### 2. OpenAI
- **Model**: GPT-4o-mini
- **Cost**: Paid (very affordable ~$0.15 per 1M tokens)
- **Speed**: Fast
- **Get API Key**: https://platform.openai.com/api-keys

### 3. Mock Generation (Fallback)
- Always available as fallback if both AI providers fail
- Uses intelligent algorithms with 79+ exercises
- Supports all fitness levels and periodization

## Fallback Chain

The system tries providers in this order:
```
Gemini → OpenAI → Mock Generation
```

If Gemini fails, it automatically tries OpenAI. If both fail, it uses the enhanced mock generation.

## Configuration

### appsettings.json

Add your API keys to `src/GymHero.Api/appsettings.json`:

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE"
  },
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY_HERE"
  }
}
```

### appsettings.Development.json

Or for development only, add to `src/GymHero.Api/appsettings.Development.json`:

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE"
  },
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY_HERE"
  }
}
```

### Environment Variables

You can also configure via environment variables:

```bash
# Windows
set Gemini__ApiKey=YOUR_GEMINI_API_KEY_HERE
set OpenAI__ApiKey=YOUR_OPENAI_API_KEY_HERE

# Linux/Mac
export Gemini__ApiKey=YOUR_GEMINI_API_KEY_HERE
export OpenAI__ApiKey=YOUR_OPENAI_API_KEY_HERE
```

## Getting a Gemini API Key (Recommended)

1. Go to https://aistudio.google.com/app/apikey
2. Sign in with your Google account
3. Click "Get API Key"
4. Click "Create API Key"
5. Copy the key and add it to your configuration

**Gemini Free Tier Limits**:
- 15 requests per minute
- 1 million requests per day
- 1,500 requests per day (free tier)

This is more than enough for personal use and development!

## Getting an OpenAI API Key

1. Go to https://platform.openai.com/signup
2. Create an account
3. Add billing information (required even for low usage)
4. Go to https://platform.openai.com/api-keys
5. Click "Create new secret key"
6. Copy the key and add it to your configuration

**OpenAI Pricing**:
- GPT-4o-mini: ~$0.15 per 1M input tokens, ~$0.60 per 1M output tokens
- Very affordable for personal use

## Configuration Options

### Option 1: Gemini Only (FREE)
```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  }
}
```
Uses Gemini for AI generation, falls back to mock if it fails.

### Option 2: OpenAI Only (Paid)
```json
{
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY"
  }
}
```
Uses OpenAI for AI generation, falls back to mock if it fails.

### Option 3: Both (Maximum Reliability)
```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  },
  "OpenAI": {
    "ApiKey": "YOUR_OPENAI_API_KEY"
  }
}
```
Tries Gemini first, then OpenAI if Gemini fails, then mock if both fail.

### Option 4: No AI (Mock Only)
```json
{
  "Gemini": {
    "ApiKey": ""
  },
  "OpenAI": {
    "ApiKey": ""
  }
}
```
Always uses the enhanced mock generation (no API calls).

## Testing

After configuring your API key, test it by:

1. Start the backend: `cd src/GymHero.Api && dotnet run`
2. Go to the AI Workout page in the frontend
3. Try generating a workout or plan
4. Check the backend console logs to see which provider was used:
   - `"Calling Gemini API..."` - Gemini is being used
   - `"Gemini API call failed. Trying OpenAI..."` - Falling back to OpenAI
   - `"All AI APIs failed. Using enhanced mock generation."` - Using mock

## Troubleshooting

### "Gemini API error: 429"
- You've hit the rate limit
- Wait a minute and try again
- Consider adding OpenAI as backup

### "OpenAI API error: 401"
- Your API key is invalid
- Check that you copied the full key
- Make sure billing is set up (OpenAI requires it)

### "All AI APIs failed"
- Both Gemini and OpenAI failed
- The system automatically used mock generation
- Your workout/plan was still generated successfully!

## Recommendations

For **development**: Use Gemini free tier
For **production with low traffic**: Use Gemini free tier
For **production with high traffic**: Use both Gemini + OpenAI for redundancy
For **offline/no API**: Use mock generation only (works great!)

## Security

⚠️ **Important**: Never commit API keys to git!

Add to `.gitignore`:
```
appsettings.Development.json
appsettings.*.json
```

Keep your API keys in:
- Environment variables (production)
- appsettings.Development.json (development, gitignored)
- Secret manager (optional, for ASP.NET Core)
