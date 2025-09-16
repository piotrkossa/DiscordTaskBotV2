# Discord Task Bot 📝

A Discord bot for managing tasks on a server – supports statuses and interactive buttons. It reads from `.json`, and allows editing tasks directly via embeds.

## ✨ Features

- Add tasks with deadlines and descriptions
- Change task status (⚪ Todo → 🟡 In Progress → ✅ Complete → 📦 Archived)
- Automatic embed coloring
- Interactive buttons for status control (Start, Complete, Archive, Delete)
- Deadline countdowns with `<t:...:R>` formatting
- Archiving tasks to specified channel

## </> Commands
```/createtask {description} {user} {days to deadline}```

## 🛠️ Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- A `.json` file with:

```json
{
  "DiscordBot": {
    "Token": "YOUR_TOKEN",
    "GuildId": YOUR_GUILD,
    "ArchiveChannelId": ARCHIVE_CHANNEL,
    "RegisterCommandsGlobally": true or false,
    "LogLevel": int,
    "TaskManagamentRoleId": ROLE
  }
}
```

## Build Command
```dotnet publish -c Release -o path```

## 📝 License
Open source – feel free to use, modify, and contribute!
License: MIT
