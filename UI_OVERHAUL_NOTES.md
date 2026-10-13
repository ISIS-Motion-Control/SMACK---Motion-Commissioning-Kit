# SMACK ModernWpf UI overhaul

This is a source-only copy of the supplied project. The existing TwinCAT/ADS,
measurement-device, and test backend classes have been retained except for the
explicit changes described below. Open `TwinCat Motion ADS.sln` in Visual Studio
on Windows to build and test the application.

## Changes

- `MainWindow.xaml`: compact sidebar, header and connection badge; resizable,
  scrollable view host; reduced console footprint. Existing navigation preserved.
- `APPLICATION WINDOWS/NcAxisView.xaml`: compact axis controls and test
  parameters; less common controls moved into expandable sections. All previous
  named controls and event hookups preserved.
- `APPLICATION WINDOWS/AirAxisView.xaml`: two-card layout, status chips,
  parameter labels, test execution and pause/cancel controls.
- `APPLICATION WINDOWS/SettingsView.xaml`: connection and measurement-tool cards.
- `APPLICATION WINDOWS/CsvHelperView.xaml`: responsive data and chart workspace,
  labelled toolbars in place of cryptic icon-only actions.
- `APPLICATION WINDOWS/TestSuite.xaml`: settings and queued tests side by side;
  bottom run-status panel.
- `APPLICATION WINDOWS/HelpWindow.xaml` and `measurementDeviceWindow.xaml`:
  visual consistency; existing help topics and device handlers preserved.
- CSV helper pop-up XAMLs: lighter backgrounds and shared input/button styles.
- Shared settings and readback controls: cleaner standardised typography.
- `Theme/Styles.xaml`: added shared form/expander styles; console item no longer
  needs a custom ControlTemplate just to show a ContentPresenter.
- `ListBoxWriter.cs`: dispatch console additions to the UI thread.
- `NcAxisView.xaml.cs`: reject invalid axis numbers / disconnected PLC, and
  restore test controls on errors.
- `SettingsView.xaml.cs`: keep the main-window AMS Net ID/connection badge in
  sync after a reconnection.
- `AirAxisView.xaml.cs` + `PneumaticAxis.cs`: status bindings, initialisation
  checks, cooperative Pause and Cancel requests, and stop obsolete status-poll
  loops when an air axis is reinitialised.
- `MainWindow.xaml.cs`: no longer disposes the PLC before the user confirms
  closing while an NC or air test is running. Reconnection is blocked while an
  NC or air test is active, and manual air controls are disabled during the test.

## Important motion/safety notes

- The air-axis **Cancel** action is cooperative: it is checked between commands,
  during delays and between test reads. It does **not** interrupt an active
  cylinder movement and is **not** an emergency stop. Retain hardware safety
  systems and validate on a safe test rig before any live use.
- The code does not automatically reinitialize NC axis or air-axis variable
  handles after reconnecting to a different PLC. Do this manually through Init.
- The Test suite remains a separate window (same behavior as the original app).
- NC axis limit-reversal commands and specialised test settings remain available
  under expandable sections.
- The application targets `net5.0-windows`, uses ModernWpfUI 0.9.6, and requires
  the original ADS environment/packages. The XAML was checked for XML validity;
  a full Windows/WPF build and hardware commissioning test were not available
  in this environment.

## Suggested Windows test sequence

1. Restore NuGet packages and build `TwinCat Motion ADS` in Visual Studio.
2. Launch without a PLC; verify navigation, console scrolling, Settings and
   correct disconnected state. Enter an invalid axis number and ensure it
   reports an error instead of crashing.
3. Confirm PLC connection and status badge, then initialise an NC axis. Check
   position/status changes and axis command button wiring with a safe setup.
4. Confirm the test selector, expanded parameter editing, save directory,
   Pause/Cancel, progress and estimate displays for supported NC tests.
5. Initialise an air axis. Verify input statuses, manual extend/retract,
   directory validation and cooperative Pause/Cancel between cycles.
6. Import/export CSV, edit the axes, chart series, and use all pop-up dialogs.
7. Add and reorder tests in Test suite; check files and run results.
8. Disconnect/reconnect to a different PLC. Reinitialise axes afterwards.
9. Test Windows scaling (100%/125%/150%), small windows and scrolling.
