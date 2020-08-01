# How to setup Tomoe

If you wish to host Tomoe yourself, skip on down to prerequisites. If you just want Tomoe to join your guild: https://discord.com/api/oauth2/authorize?client_id=481314095723839508&permissions=8&scope=bot

## Prerequisites

Having Dotnet Core SDK installed (Version 5.0 Preview as of this moment). You can get it from [Microsoft](https://dotnet.microsoft.com/download/dotnet/5.0) or the [Mono Project](https://www.mono-project.com/download/stable/). I am currently developing Tomoe with the Microsoft link, Visual Studio Code, and [Windows Terminal](https://www.microsoft.com/en-us/p/windows-terminal/9n0dx20hk701). I have yet to test Tomoe on any Linux Distro, and I'll never be able to test Tomoe on a Mac. While everything should theoretically work, there are no promises. If anything isn't working correctly, please open an issue and let me know.

## Setup

``` bash
git clone https://github.com/OoLunar/Tomoe.git
cd Tomoe
printf "\n\nSTOP HERE! Editing any and all values in res/tokens.xml (https://github.com/OoLunar/Tomoe/blob/master/res/tokens.xml) so everything works correctly.\n\n\n"
dotnet run # First run might take a minute since it has to download dependencies and whatnot.
```

# Tomoe, The Discord Moderation Bot

Tomoe is all about moderation for big time servers and aims to be Vortex, Zeppelin and Carl all in one.

## What is Tomoe written in?

C#. However once Tomoe is finished 100%, I plan on creating a seperate branch rewriting Tomoe in Rust for efficency. Please keep in mind that plans may change.

## What can Tomoe do?

As of this minute, Tomoe can do the following commands:

| Command | Syntax | Params | Definition |
|:-:|:-:|:-:|:-:|
| setup_guild | >>setup_guild | N/A | Registers the guild into the database. |
| setup_mute | >>setup_mute [Role] | [Role] = Mentioned Role \| Role ID | Sets up the mute role for HardMute and SoftMute |
| hard_mute | >>hard_mute [User] | [User] = Mentioned User\| User ID | Mutes the user permanently until staff unmute them. |

Some might have noticed that all commands are snake case. This is best for simplicity and consistancy.

## Anything notable about Tomoe?

Tomoe has customizable dialogs over in [res/dialogs.xml](https://github.com/OoLunar/Tomoe/blob/master/res/dialog.xml). Templates will soon be available. Additionally, Tomoe uses Postgres as the preferred database, however changes will be made to support other storage formats such as JSON or SQLite. By default, all configs are XML. Again, other storage formats such as JSON or TOML will be supported.

## What's to come?

As mentioned previously, Tomoe aims to be the #1 bot choice for Discord Moderation. This means that any and all moderation commands that Vortex, Zeppelin and Carl can do, Tomoe will try to implement. Here's the current road map:
| Command | Implemented | Type |
|:-|:-:|:-|
| setup_guild | Halfway Done | Interactive |
| setup_no_meme | :x: | Static or Interactive |
| setup_mute | :heavy_check_mark: | Static or Interactive |
| setup_logging | :x: | Static or Interactive |
| setup_ignored_channels | :x: | Static or Interactive |
| setup_timezone | :x: | Static or Interactive |
| setup_admins | :x: | Static or Interactive |
| setup_anti_invite | :x: | Static or Interactive |
| setup_anti_everyone | :x: | Static or Interactive |
| setup_anti_duplicate | :x: | Static or Interactive |
| setup_max_lines | :x: | Static or Interactive |
| setup_max_mentions | :x: | Static or Interactive |
| setup_auto_dehoist | :x: | Static or Interactive |
| setup_auto_raidmode | :x: | Static or Interactive |
| allow_invite | :x: | Static |
| strike | :x: | Static |
| nomeme | :x: | Static |
| temp_mute | :x: | Static |
| perm_mute | Halfway Done | Static |
| kick | :x: | Static |
| temp_ban | :x: | Static |
| perm_ban | :x: | Static |
| report | :x: | Static |
| tag | :x: | Static |
| help | :x: | Static |

*Naming may change for simplicity.

## Why did you chose "Tomoe" as the bots name?

Simple! I originally found the name from the anime [Kamisama Kiss](https://www.funimation.com/shows/kamisama-kiss/). Tomoe is a wild fox yokai that Mikage had as his previous familiar. While he is taking care of Nanami, he's very strict yet kind with her. I'm taking the strict part from him, and putting it into the bot.
