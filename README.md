![Code Size](https://img.shields.io/github/languages/code-size/OoLunar/Tomoe?style=for-the-badge&logo=appveyor&color=blueviolet&logo=none)![Lines of Code](https://img.shields.io/tokei/lines/github/OoLunar/Tomoe?style=for-the-badge&logo=appveyor&color=blueviolet&label=Total%20Lines%20of%20Code&logo=none)[![Discord](https://img.shields.io/discord/832354798153236510?style=for-the-badge&color=blueviolet&label=Chat%20On%20Discord&logo=discord)](https://discord.gg/5gm3pUt8Fg)

## Prerequisites

Tomoe uses dotnet 7.0. You can get it from [Microsoft](https://dotnet.microsoft.com/download/dotnet/7.0). Tomoe should work on all OS' that .NET is supported on. Mono is not supported due to DSharpPlus intentionally choosing not to support it.

## Setup

* Edit your `res/config.json` file. If you're planning on contributing to the bot, copy the `res/config.json` file to `config.json.prod` and edit that instead.

``` bash
git clone https://github.com/OoLunar/Tomoe.git
cd Tomoe
dotnet restore
```

Be sure to edit your config file `res/config.json` .

``` bash
dotnet run
```

### Docker

``` bash
docker run ghcr.io/oolunar/tomoe --mount ./configs,/src/res
```

### Docker-Compose

Make sure to review the `volumes` section of the `docker-compose.yml` file.

``` bash
docker-compose up -d
```

# Tomoe, The Discord Moderation Bot

## What features does Tomoe have?

As mentioned previously, Tomoe aims to be the #1 bot choice for Discord Moderation. This means that any and all moderation commands that Vortex, Mee6 and Carl can do, Tomoe will try to implement. Here's the current road map of commands:

| Command                 | Implemented  |
|-------------------------|--------------|
| **Moderation Commands** | -            |
| Antimeme                | ❌           |
| Autoreact               | ❌           |
| Ban                     | ❌           |
| Config                  | ❌           |
| Doctor                  | ❌           |
| Kick                    | ❌           |
| Logging                 | ❌           |
| Lock                    | ❌           |
| Modlog                  | ❌           |
| Mute                    | ❌           |
| Reaction Roles          | ❌           |
| Report                  | ❌           |
| Strike                  | ❌           |
| Tempantimeme            | ❌           |
| Tempban                 | ❌           |
| Tempmute                | ❌           |
| Templock                | ❌           |
| Tempvoiceban            | ❌           |
| Unantimeme              | ❌           |
| Unban                   | ❌           |
| Unmute                  | ❌           |
| Unvoiceban              | ❌           |
| Voiceban                | ❌           |
| **Public commands**     | -            |
| Bot Info                | ✅           |
| Flip                    | ❌           |
| GuildIcon               | ❌           |
| Invite                  | ❌           |
| Member Count            | ❌           |
| Ping                    | ✅           |
| Profile Picture         | ❌           |
| Raw                     | ❌           |
| Reminders               | ❌           |
| Repository              | ❌           |
| Role Info               | ❌           |
| Server Info             | ❌           |
| Support                 | ❌           |
| Tag                     | ❌           |
| Time Of                 | ❌           |


## Why did you chose "Tomoe" as the bots name?

[Kamisama Kiss](https://www.funimation.com/shows/kamisama-kiss/).
