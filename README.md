# ASCOM Driver for IP Clamshell Dome

ASCOM Dome driver (shutter only) for a clamshell dome driven by an **IP relay board** that is
controlled with plain HTTP GET requests. Works with NINA and any other ASCOM client.

The board's limit switches handle the end of travel, so each command is **one pulse per relay**:
the dome then opens or closes by itself. Because the board gives no feedback and a pulse cannot be
stopped, the driver estimates the shutter state from a travel time you configure.

## Highlights

- **Nothing is hard-coded.** Host, credentials and the exact GET requests for *Open* and *Close*
  are configured in the setup wizard. A fresh install has no commands and cannot move the dome.
- **Password protected.** Stored encrypted with Windows DPAPI (current user), never written to
  logs or URLs.
- **Safe state handling.** Real states reported to the client: `Closed`, `Opening`, `Open`,
  `Closing`, `Error`. A failed request puts the driver in `Error` instead of pretending the dome moved.
- **No accidental double pulses.** Repeating a command that is already running is ignored; the
  opposite command is refused until the travel time has elapsed.
- **Simulation mode** to try everything without touching the hardware.
- **Import / export** of the configuration as JSON (password excluded unless you choose otherwise).

## Setup wizard

1. **Connection** – board address, HTTP/HTTPS, authentication, user and password. *Test connection*
   only checks that the address answers on its TCP port; it **never sends a command**.
2. **Commands** – a path template containing `{relay}` plus the list of relays to pulse for *Open*
   and for *Close*, sent in parallel or one after another with an optional delay. A live preview
   shows exactly which URLs will be called. An advanced grid allows fully custom steps.
3. **Timing & status** – travel time for opening and closing, request timeout, optional text the
   board must answer, and the state assumed at first start.
4. **Options** – simulation mode, trace log, import/export.

If you upgrade from the first version, host, username and password are migrated automatically
(the password is re-saved encrypted). Commands must be entered again.
The values used by the previous version are written down in [docs/LEGACY_COMMANDS.md](docs/LEGACY_COMMANDS.md).

## Extra ASCOM actions

| Action | Parameter | Purpose |
|---|---|---|
| `SetAssumedState` | `open` or `closed` | Re-sync the driver after moving the dome by hand |
| `GetStateInfo` | – | One-line description of the current state |

## Build

Visual Studio 2019+ with the .NET Framework 4.7.2 targeting pack and the ASCOM Platform 6.x
(plus its Developer Components) installed. Open `Clamshell/Clamshell.sln`, build, then run the produced
`ASCOM.Clamshell.exe /register` once (it asks for administrator rights) to register the driver;
`/unregister` removes it.

## Tests

`Tests/ClamshellCore.Tests/build_and_run.sh` builds the platform-independent core with Mono and runs
the tests against a mock HTTP server on `127.0.0.1`. The tests set `CLAMSHELL_TEST_MODE=1`, which
makes the driver refuse any address that is not loopback, so a test can never reach a real board.

## License

No license has been chosen yet.

Author: Robert Panai
