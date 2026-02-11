# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Flash Monitor is a .NET MAUI app (net10.0) for monitoring and controlling FlashForge 3D printers over the local network via the FlashForge HTTP API (port 8898). It targets Android, iOS, MacCatalyst, and Windows. App ID: `com.tylercaselli.flashmonitor`.

## Build Commands

```bash
# Build
dotnet build "Flash Monitor.sln"

# Build for specific platform
dotnet build -f net10.0-android
dotnet build -f net10.0-ios
dotnet build -f net10.0-maccatalyst
dotnet build -f net10.0-windows10.0.19041.0

# Restore packages
dotnet restore
```

No test project exists yet.

## Architecture

**Pattern**: Code-behind with direct event handling (no MVVM framework). XAML for UI, C# code-behind for logic.

**Navigation flow** (`App.xaml.cs`): Checks `PrinterIp` in MAUI Preferences — if set, navigates to MainPage; otherwise, navigates to SetupPage.

**Pages**:
- `SetupPage` — First-run config: collects printer IP, serial number, check code, and feature flags (light, camera). Stores everything in MAUI Preferences.
- `MainPage` — Primary interface: printer status display, print job controls (pause/resume/cancel), light toggle. Contains the full HTTP API client implementation inline.

**API client** (embedded in `MainPage.xaml.cs`): Implements all FlashForge HTTP API endpoints (`/detail`, `/product`, `/control`, `/gcodeList`, `/gcodeThumb`, `/printGcode`, `/uploadGcode`). Response models (`ApiResponse`, `DetailResponse`, `PrinterDetail`, `ProductInfo`, etc.) are defined in the same file.

**Data storage**: MAUI Preferences with keys: `PrinterIp`, `PrinterName`, `PrinterSn`, `PrinterCheckCode`, `PrinterHasLight`, `PrinterHasCam`.

## API Reference

`printer-api-docs.md` contains the full FlashForge HTTP API documentation (endpoints, auth patterns, request/response schemas, error codes, machine states). Refer to this when implementing new printer interactions.