# TheNerdCollective.Components.BlazorServerCircuitHandler

A production-ready Blazor Server circuit reconnection handler with automatic reconnection UI and graceful error handling.

## Overview

This package provides a drop-in Razor component that handles Blazor Server circuit reconnection scenarios with:
- Automatic reconnection with exponential backoff
- Silent reconnection for brief interruptions
- Professional reconnection overlay with countdown timer
- Graceful handling of server restarts and deployments

## Quick Start

### Installation

```bash
dotnet add package TheNerdCollective.Components.BlazorServerCircuitHandler
```

### Setup

1. **Add to your root component** (`App.razor`):
```razor
<Routes @rendermode="InteractiveServer" />
<CircuitReconnectionHandler @rendermode="InteractiveServer" />
```

2. **Import in `_Imports.razor`**:
```csharp
@using TheNerdCollective.Components.BlazorServerCircuitHandler
```

3. **Done!** The component handles everything automatically.

## Features

✅ **Automatic Reconnection** - Infinite reconnection with exponential backoff (1s → 5s)  
✅ **Silent First Attempts** - No UI shown for quick reconnects (first 5 attempts)  
✅ **Professional UI** - Beautiful reconnection overlay with countdown timer  
✅ **Error Suppression** - Filters out MudBlazor and expected disconnection errors  
✅ **Graceful Fallback** - Auto-reload when circuits expire  
✅ **Zero Configuration** - Works out of the box  

## How It Works

### Scenario 1: Brief Network Interruption
```
Connection lost → 5 silent reconnection attempts → Connected
Result: User sees nothing
```

### Scenario 2: Extended Disconnection
```
Connection lost → 5 failed attempts → UI overlay shown
User waits or clicks "Reload Now" → Connection restored
Result: Overlay disappears, app continues normally
```

### Scenario 3: Server Restart/Deployment
```
Circuit expires → Auto-reload triggered
Result: Fresh session with new app instance
```

## Configuration

### Basic Usage

The handler uses sensible defaults and requires zero configuration:

```razor
<CircuitReconnectionHandler @rendermode="InteractiveServer" />
```

### Customizing Colors

```razor
<CircuitReconnectionHandler 
    @rendermode="InteractiveServer"
    PrimaryColor="#FF5722"
    SuccessColor="#00C853" />
```

### Custom Spinner Image

```razor
<CircuitReconnectionHandler 
    @rendermode="InteractiveServer"
    SpinnerUrl="/images/custom-spinner.gif" />
```

### Custom CSS

```razor
<CircuitReconnectionHandler 
    @rendermode="InteractiveServer"
    CustomCss="@customCss" />

@code {
    private string customCss = @"
        @keyframes spin { to { transform: rotate(360deg); } }
        #blazor-reconnect-modal { font-family: 'Roboto', sans-serif; }
        #manual-reload-btn:hover { opacity: 0.9; }
    ";
}
```

### Fully Custom HTML Dialog

For complete control over the dialog appearance, provide custom HTML:

```razor
<CircuitReconnectionHandler 
    @rendermode="InteractiveServer"
    ReconnectingHtml="@reconnectingHtml"
    ServerRestartHtml="@serverRestartHtml"
    CustomCss="@customCss" />

@code {
    private string reconnectingHtml = @"
        <div style='position: fixed; inset: 0; background: rgba(0,0,0,0.8); z-index: 9999; 
                    display: flex; align-items: center; justify-content: center;'>
            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
                        padding: 3rem; border-radius: 16px; text-align: center; color: white;'>
                <img src='/logo.svg' style='width: 64px; margin-bottom: 1rem;' />
                <h2>We'll be right back!</h2>
                <p id='reconnect-status'>Reconnecting to server...</p>
                <p id='reconnect-countdown'>Next try in <span id='countdown-seconds'>1</span>s</p>
                <button id='manual-reload-btn' style='background: white; color: #667eea; 
                        border: none; padding: 0.75rem 2rem; border-radius: 8px; cursor: pointer;'>
                    Refresh Page
                </button>
            </div>
        </div>
    ";

    private string serverRestartHtml = @"
        <div style='position: fixed; inset: 0; background: rgba(0,0,0,0.8); z-index: 9999; 
                    display: flex; align-items: center; justify-content: center;'>
            <div style='background: white; padding: 3rem; border-radius: 16px; text-align: center;'>
                <h2>Updating...</h2>
                <p>Please wait while we reload the application.</p>
            </div>
        </div>
    ";

    private string customCss = @"
        @keyframes spin { to { transform: rotate(360deg); } }
        h2 { margin: 0 0 1rem; }
    ";
}
```

**Important placeholder IDs for custom HTML:**
- `reconnect-status` - Updated with status messages
- `countdown-seconds` - Displays countdown timer
- `manual-reload-btn` - Must have this ID for reload functionality

### Circuit Options

Customize circuit behavior in `Program.cs`:

```csharp
builder.Services.Configure<CircuitOptions>(options =>
{
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(30);
    options.DetailedErrors = builder.Environment.IsDevelopment();
});
```

## Dependencies

- **MudBlazor** 8.15+ (optional, but recommended)
- **.NET** 10.0+
- **Blazor Server** enabled

## Browser Compatibility

- ✅ Chrome 60+
- ✅ Firefox 55+
- ✅ Safari 12+
- ✅ Edge 79+

## License

Apache License 2.0 - See LICENSE file for details

## Repository

[TheNerdCollective.Components on GitHub](https://github.com/janhjordie/TheNerdCollective.Components)

---

Built with ❤️ by [The Nerd Collective Aps](https://www.thenerdcollective.dk/)

By [Jan Hjørdie](https://github.com/janhjordie/) - [LinkedIn](https://www.linkedin.com/in/janhjordie/) | [Dev.to](https://dev.to/janhjordie)

MIT License - See LICENSE file for details

## Support

For issues, questions, or contributions, visit:
- GitHub: https://github.com/janhjordie/TheNerdCollective.Components

## Version History

- **1.0.0** (2025-12-19) - Initial release with infinite reconnection handler, professional UI, and error suppression

---

**Built with ❤️ for the Blazor Server community**
