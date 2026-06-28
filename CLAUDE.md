# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

SeptaRail shows the next three SEPTA Regional Rail trains between two stations. It has two independently-deployed .NET 10 components (the repo root `global.json` pins the SDK):

- **`src/ClientApp`** — a .NET MAUI cross-platform app (Android, iOS, MacCatalyst, Windows). The mobile/desktop UI.
- **`src/GetTrainsFunction`** — an isolated-worker Azure Function (HTTP, v4) that the ClientApp calls. It proxies the public SEPTA "NextToArrive" hackathon API.

The ClientApp does **not** call SEPTA directly — it always calls the Azure Function, which in turn calls SEPTA. Request/response shapes are duplicated on both sides (`NextTrain`/`NextTrainRequest` in ClientApp mirror `ApiResponse`/`ApiRequest` in the Function); keep them in sync when changing the contract.

## Data flow

```
HomePageViewModel.SearchTrain()
  -> INextTrainFunction (NextTrainFunction)
  -> IRestService (RestService)        POSTs {from,to} JSON
  -> Azure Function NextThreeTrainFunction.Run()
  -> GET www3.septa.org/hackathon/NextToArrive/{from}/{to}/3
```

`RestService` always POSTs to `Constants.ProdRestUrl` (the deployed Azure Function, function key embedded in the URL). `Constants.RestUrl` (localhost:7143) exists for local testing but is not wired into `RefreshDataAsync` — switch the `uri` there if you want to hit a locally-running Function. Android can't reach `localhost`, so `Constants.LocalhostUrl` maps it to `10.0.2.2` (emulator host loopback).

The Function's SEPTA base address is configured in `Program.cs` via a named `HttpClient` ("httpClient"); the function builds the path as `{from}/{to}/3`.

## Architecture notes

- **ClientApp** is MVVM-ish but hand-rolled: `HomePageViewModel` implements `INotifyPropertyChanged` manually (no MVVM Toolkit source generators). Services and pages are registered as singletons in `MauiProgram.cs` and resolved via DI. Navigation/shell is `AppShell`.
- The station list is a hardcoded `List<string>` in `HomePageViewModel.LoadPicker()` — both pickers share it.
- `HttpsClientHandlerService` provides per-platform `HttpMessageHandler`s that trust the `CN=localhost` dev certificate; it is only used in `#if DEBUG` builds of `RestService`. Release builds use a plain `HttpClient`.
- **Function** is the isolated worker model (`Microsoft.Azure.Functions.Worker`), not the in-process model. Auth level is `Function` (requires the key). App Insights is wired in `Program.cs`.
- **MAUI/CommunityToolkit versions**: MAUI is pinned via `<MauiVersion>` in `ClientApp.csproj` (newer than the installed workload default — required by `CommunityToolkit.Maui`). The Toolkit is on the v14 Popup API: popups are `ContentView`s shown with `Page.ShowPopup(view, new PopupOptions { ... })` and dismissed with `await popup.CloseAsync()` (see `SpinnerPopup` + `HomePageViewModel.SearchTrain()`). When bumping the Toolkit, check its required minimum `Microsoft.Maui.Controls` version and raise `<MauiVersion>` to match, or restore fails with `NU1605`.

## Commands

Solutions: root `SeptaRail.sln` (everything). The MAUI app no longer ships its own solution — build the project directly at `src/ClientApp/ClientApp.csproj`.

```bash
# Build the Azure Function
dotnet build src/GetTrainsFunction/GetTrainsFunction.csproj

# Run the Function locally (needs Azure Functions Core Tools)
cd src/GetTrainsFunction && func start

# Run tests
dotnet test test/GetTrainsFunction.Tests/GetTrainsFunction.Tests.csproj

# Run a single test
dotnet test test/GetTrainsFunction.Tests/GetTrainsFunction.Tests.csproj --filter "FullyQualifiedName~NextThreeTrainFunction_InvalidBody"

# Tests with coverage (matches CI)
dotnet test test/GetTrainsFunction.Tests/GetTrainsFunction.Tests.csproj --collect:"XPlat Code Coverage"

# Build the MAUI app for a single platform (must target one TFM)
dotnet build src/ClientApp/ClientApp.csproj -f net10.0-windows10.0.19041.0
dotnet build src/ClientApp/ClientApp.csproj -f net10.0-android

# Unpackaged Windows build (skips MSIX packaging — useful for a quick compile check)
dotnet build src/ClientApp/ClientApp.csproj -f net10.0-windows10.0.19041.0 -p:WindowsPackageType=None
```

Notes:
- The MAUI app multi-targets (`net10.0-android;net10.0-ios;net10.0-maccatalyst`, plus `net10.0-windows...` only on Windows). Always pass `-f <tfm>` when building/running the app. Requires the .NET 10 MAUI workload (`dotnet workload install maui`); iOS/MacCatalyst builds require a Mac. iOS/MacCatalyst minimum is `SupportedOSPlatformVersion` 15.0.
- Only the Function has tests (xUnit + Moq). There are no ClientApp tests.

### Build environment gotchas
- **Azure Function build**: the final `WorkerExtensions` metadata-generation step can fail in restricted/sandboxed environments with `Failed to read environment variable [DOTNET_STARTUP_HOOKS], HRESULT: 0x800700CB`. This is environmental, not a code problem (the C# compiles). To compile/run tests past it locally, gate it off with `-p:_FunctionsBuildEnabled=false` (verification only — never commit this). It builds normally in the `windows-latest` pipeline.
- **MAUI Windows MSIX packaging** can fail loading a native module (`APPX0002` / `Win32Exception 126`) in restricted environments; use `-p:WindowsPackageType=None` for a compile check.

## Testing the Function

`MockHttpRequestData`/`MockHttpResponseData` in `test/.../Mocks/` stand in for the isolated-worker `HttpRequestData`/`HttpResponseData` types, which are otherwise hard to construct. `CreateNextThreeTrainFunction()` injects a mocked `HttpMessageHandler` so SEPTA is never actually called. The happy-path test `TestNextThreeTrainFunctionSuccess` is `[Skip]`ped — `WriteAsJsonAsync` on the mocked response isn't implemented yet (`TODO Mock WriteAsJsonAsync`).

## CI/CD

Azure Pipelines (`pipeline/Yaml_Files/`), one per component. Both trigger only on `release/*` branches and PRs into them, path-filtered to their own `src/` folder:
- **GetTrainsFunction** (windows-latest): GitVersion-stamps the version, builds, runs tests with coverage, then deploys to the `GetTrainFunction` Azure Function App.
- **ClientApp** (macos-15): installs the MAUI workload + Apple signing cert/profile and publishes the iOS `.ipa` (AdHoc). iOS signing identity/profile are also set in `ClientApp.csproj` under the iOS-Release `PropertyGroup`.
