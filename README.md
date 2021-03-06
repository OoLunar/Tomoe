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
Be sure to edit your config file.
```bash
dotnet run
```

### Docker
**THIS WILL NOT WORK UNTIL A RELEASE HAPPENS. USE DOCKER-COMPOSE INSTEAD!**
```bash
docker run ghcr.io/oolunar/tomoe --mount ./configs,/src/res
```

### Docker-Compose
Make sure to review the `volumes` section of the `docker-compose.yml` file.
```bash
docker-compose up -d
```

# Tomoe, The Discord Moderation Bot

## What is Tomoe written in?

99.8% C#, 0.02% Docker. Just like the sidebar on your right says.

## What features does Tomoe have?

As mentioned previously, Tomoe aims to be the #1 bot choice for Discord Moderation. This means that any and all moderation commands that Vortex, Mee6 and Carl can do, Tomoe will try to implement. Here's the current road map of commands:

| Command | Implemented |
|:-|:-:|
| ban | ✅ |
| kick | ✅ |
| mute | ✅ |
| antimeme | ✅ |
| vc_ban | ✅ |
| strike | ✅ |
| strike history | ✅ |
| pardon| ✅ |
| temp_ban | ✅ |
| temp_mute | ✅ |
| temp_antimeme | ✅ |
| temp_vc_ban | ❌ |
| unban | ✅ |
| unmute | ✅ |
| promeme | ✅ |
| unvc_ban | ❌ |
| report | ❌ |
| config mute | ✅ |
| config antimeme | ✅ |
| config logging | ❌ |
| lock channel | ❌ |
| lock server | ❌ |
| lock bots | ❌ |
| bot_info | ✅ |
| flip | ✅ |
| guild_icon | ✅ |
| invite | ✅ |
| pfp | ✅ |
| raw | ✅ |
| reminders | ✅ (kinda broken at the moment) |
| reminders list | ✅ (kinda broken at the moment) |
| reminders remove | ✅ (kinda broken at the moment) |
| repo | ✅ |
| role_info | ✅ |
| server_info | ✅ |
| support | ✅ |
| tag | ❌ (Tags have been temporarily removed) |

*Naming may change for simplicity. All commands are snake_case by default and have PascalCase aliases.

## Why did you chose "Tomoe" as the bots name?

Simple! I originally found the name from the anime [Kamisama Kiss](https://www.funimation.com/shows/kamisama-kiss/). Tomoe is a wild fox yokai that Mikage had as his previous familiar. While he is taking care of Nanami, he's very strict yet kind with her. I'm taking the strict part from him, and putting it into the bot.
