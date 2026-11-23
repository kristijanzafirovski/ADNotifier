# GimNotifier

GimNotifier is a Windows desktop tool for IT administrators. It sends pop-up notification messages to computers in an Active Directory domain, either immediately or on a schedule, using reusable message templates.

## Features

- **Directory browser:** navigate the organisational unit (OU) tree from Active Directory and pick computers from any OU.
- **Search:** find computers by name across the domain.
- **Send:** deliver a message to one or more selected computers, with the result reported for each target.
- **Schedule:** queue messages to be sent at a later time.
- **Templates:** create, edit and reuse message templates.
- **Settings:** configure the domain, the message timeout and related options.
- **Logging:** send results are written to a log file.

Messages are delivered with the built-in `msg.exe` utility, so no agent needs to be installed on the target computers.

## Requirements

- Windows, joined to the Active Directory domain you want to manage
- .NET 10 SDK to build the project
- Network access from the administrator's computer to the target computers, with firewall rules that allow `msg.exe` messages to reach them

## Getting started

```bash
dotnet build GimNotifier.csproj
dotnet run --project GimNotifier.csproj
```

On first launch, open **Settings** and confirm the domain name and distinguished name used for directory lookups.

## Project structure

| Folder | Contents |
|---|---|
| `Helpers/` | Directory lookups (`AdHelper`), message delivery (`MsgSender`), scheduling, templates and settings persistence |
| `Models/` | Data models for settings, computers, templates and scheduled messages |
| `Pages/` | UI pages: Search, Send, Schedule, Templates and Settings |

## Tech stack

C# · .NET 10 · Windows Forms · System.DirectoryServices (LDAP)

## License

Released under the GNU General Public License v2. See [LICENSE.txt](LICENSE.txt).

## Author

**Kristijan Zafirovski**
