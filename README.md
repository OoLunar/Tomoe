# How to setup Tomoe

If you wish to host Tomoe yourself, skip on down to prerequisites. If you just want Tomoe to join your guild: https://discord.com/api/oauth2/authorize?client_id=481314095723839508&permissions=8&scope=bot

## Prerequisites

Tomoe uses dotnet 5.0. You can get it from [Microsoft](https://dotnet.microsoft.com/download/dotnet/5.0). On Windows, I develop Tomoe using Visual Studio Code and [Windows Terminal](https://www.microsoft.com/en-us/p/windows-terminal/9n0dx20hk701). On Ubuntu, I use Visual Studio Code and Gnome Terminal. Tomoe works on Windows 10 Home and Ubuntu 20.04/20.10. Hopefully I can test on mac soon. While everything should theoretically work, there are no promises. If anything isn't working correctly, please open an issue and let me know. Mono is not supported due to DSharpPlus choosing not to support it.

## Setup

* Run a PostgreSQL instance that is reachable by the bot

``` bash
git clone https://github.com/OoLunar/Tomoe.git
cd Tomoe
dotnet restore
cp res/config.jsonc res/config.jsonc.prod
printf "\n\nPlease edit res/config.jsonc.prod\n\n\n"
dotnet run # First run might take a minute since it has to download dependencies and whatnot.
```

### Docker

1. Edit the config file `res/config.jsonc` to suit your needs
2. Run the actual bot (how you do this is up to you but make sure that you have a volume from `./res` to `/Tomoe/res` inside the container)

**The Docker image is `ghcr.io/oolunar/tomoe`**

# Tomoe, The Discord Moderation Bot

## What is Tomoe written in?

C#. However once Tomoe is finished 100%, I plan on creating a separate branch and rewriting Tomoe in Rust for efficency. Please keep in mind that plans may change.

## Anything notable about Tomoe?
Tomoe aims to be the best moderation and quality of life bot, and nothing more. It supports PostgresSQL for it's database, however drivers can be added. See https://github.com/OoLunar/Tomoe/pull/1 for more info about the drivers.

## What features does Tomoe have?

As mentioned previously, Tomoe aims to be the #1 bot choice for Discord Moderation. This means that any and all moderation commands that Vortex, Zeppelin and Carl can do, Tomoe will try to implement. Here's the current road map of commands:

| Command | Implemented |
|:-|:-:|
| ban | ✅ |
| kick | ✅ |
| mute | ✅ |
| antimeme |✅ |
| vc_ban | ❌ |
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
| reminders | ✅ |
| reminders list | ✅ |
| reminders remove | ✅ |
| repo | ✅ |
| role_info | ✅ |
| server_info | ✅ |
| support | ✅ |
| tag | ✅ |
| tag alias | ✅ |
| tag all | ✅ |
| tag claim | ✅ |
| tag create | ✅ |
| tag delete | ✅ |
| tag delete_alias | ✅ |
| tag delete_all_aliases | ✅ |
| tag edit | ✅ |
| tag exist | ✅ |
| tag get_aliases | ✅ |
| tag get_author | ✅ |
| tag is_alias | ✅ |
| tag realname | ✅ |
| tag transfer | ✅ |
| tag user | ✅ |

*Naming may change for simplicity. All commands are snake_case and PascalCase.

If you host Tomoe, it can also offer features such as:

| Feature | Implemented |
| :-: | :-: |
| Cache | ❌ |
| Extensive Logging | ✅ |
| Update System | ❌ |
| Custom prefix | ✅ |
| Custom repository link | ✅ |
| Custom invite link | ✅ |
| Database Support | PostgreSQL ✅, SQLite ❌ |
| Custom Database Drivers | ✅ |

## Why did you chose "Tomoe" as the bots name?

Simple! I originally found the name from the anime [Kamisama Kiss](https://www.funimation.com/shows/kamisama-kiss/). Tomoe is a wild fox yokai that Mikage had as his previous familiar. While he is taking care of Nanami, he's very strict yet kind with her. I'm taking the strict part from him, and putting it into the bot.
