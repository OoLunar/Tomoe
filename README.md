# How to setup Tomoe

If you wish to host Tomoe yourself, skip on down to prerequisites. If you just want Tomoe to join your guild: https://discord.com/api/oauth2/authorize?client_id=481314095723839508&permissions=8&scope=bot

## Prerequisites

Tomoe uses dotnet 5.0. You can get it from [Microsoft](https://dotnet.microsoft.com/download/dotnet/5.0). On Windows, I develop Tomoe using Visual Studio Code and [Windows Terminal](https://www.microsoft.com/en-us/p/windows-terminal/9n0dx20hk701). On Ubuntu, I use Visual Studio Code and Gnome Terminal. Tomoe works on Windows 10 Home and Ubuntu 20.04/20.10. Mac support seems just fine. Mono is not supported due to DSharpPlus intentionally choosing not to support it.

## Setup

* Run a PostgreSQL instance that is reachable by the bot
* Edit your `res/config.jsonc` file. If you're planning on contributing to the bot, copy the `res/config.jsonc` file to `config.jsonc.prod` and edit that instead.

``` bash
git clone https://github.com/OoLunar/Tomoe.git
cd Tomoe
dotnet restore --configfile /src/Nuget.Config
```

Be sure to edit your config file `res/config.jsonc` .

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

## What is Tomoe written in?

99.70% C#, 0.30% Docker.

## What features does Tomoe have?

As mentioned previously, Tomoe aims to be the #1 bot choice for Discord Moderation. This means that any and all moderation commands that Vortex, Mee6 and Carl can do, Tomoe will try to implement. Here's the current road map of commands:

| Command | Implemented |
|:-|:-:|
| **Bot Owner Commands** | - |
| Reboot | ✅ |
| Stop | ✅ |
| Update | ❌ |
| **Moderation Commands** | - |
| Antimeme | ✅ |
| Autoreact | ✅ |
| Ban | ✅ |
| Config | ✅ |
| Doctor | ❌ |
| Kick | ✅ |
| Logging | ✅ |
| Lock | ✅ |
| Modlog | ✅ |
| Mute | ✅ |
| Reaction Roles | ✅ |
| Report | ❌ |
| Strike | ✅ |
| Tempantimeme | ❌ |
| Tempban | ❌ |
| Tempmute | ❌ |
| Templock | ❌ |
| Tempvoiceban | ❌ |
| Unantimeme | ✅ |
| Unban | ✅ |
| Unmute | ✅ |
| Unvoiceban | ✅ |
| Voiceban | ✅ |
| **Public commands** | - |
| Bot Info | ✅ |
| Flip | ✅ |
| GuildIcon | ✅ |
| Help | ✅ |
| Invite | ✅ |
| Member Count | ✅ |
| Ping | ✅ |
| Profile Picture | ✅ |
| Raw | ✅ |
| Reminders | ❌ |
| Repeat | ✅ |
| Repository | ✅ |
| Role Info | ✅ |
| Server Info | ✅ |
| Support | ✅ |
| Tag | ✅ |
| Time Of | ✅ |

*Naming may change for simplicity. All commands are snake_case.

## Why did you chose "Tomoe" as the bots name?

Simple! I originally found the name from the anime [Kamisama Kiss](https://www.funimation.com/shows/kamisama-kiss/). Tomoe is a wild fox yokai that Mikage had as his previous familiar. While he is taking care of Nanami, he's very strict yet kind with her. I'm taking the strict part from him, and putting it into the bot.
