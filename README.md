# How to setup Tomoe

If you wish to host Tomoe yourself, skip on down to prerequisites. If you just want Tomoe to join your guild: https://discord.com/api/oauth2/authorize?client_id=481314095723839508&permissions=8&scope=bot

## Prerequisites

Having Dotnet Core SDK installed (Version 5.0 Preview as of this moment). You can get it from [Microsoft](https://dotnet.microsoft.com/download/dotnet/5.0) or the [Mono Project](https://www.mono-project.com/download/stable/). I am currently developing Tomoe with the Microsoft link, Visual Studio Code, and [Windows Terminal](https://www.microsoft.com/en-us/p/windows-terminal/9n0dx20hk701). I have yet to test Tomoe on any Linux Distro, and I'll never be able to test Tomoe on a Mac. While everything should theoretically work, there are no promises. If anything isn't working correctly, please open an issue and let me know.

## Setup
- Run a PostgreSQL instance that is reachable by the bot

``` bash
git clone https://github.com/OoLunar/Tomoe.git
cd Tomoe
```
Be sure to edit your res/tokens.xml, then:
```bash
dotnet run # First run might take a minute since it has to download dependencies and whatnot.
```

### Docker
1. Edit the config file `./res/config.jsonc` to suit your needs
2. Run the actual bot (how you do this is up to you but make sure that you have a volume from `./res` to `/Tomoe/res` inside the container)

**The Docker image is `ghcr.io/oolunar/tomoe`**
# Tomoe, The Discord Moderation Bot

Tomoe is all about moderation for big time servers and aims to be Vortex, Zeppelin and Carl all in one.

## What is Tomoe written in?

C#. However once Tomoe is finished 100%, I plan on creating a seperate branch and rewriting Tomoe in Rust for efficency. Please keep in mind that plans may change.

## What can Tomoe do?

As of this minute, Tomoe can do the following moderation commands:

| Command | Syntax | Definition |
| :-: | :- | :- |
| Ban | >>ban User [Reason] | Bans a guild member identified by mention or ID, optionally with a reason. |
| Kick | >>kick User [Reason] | Kicks a guild member identified by mention or ID, optionally with a reason. |
| Hard Mute | >>hard_mute User [Reason] | Mutes a guild member permanently, identified by mention or ID, optionally with a reason. |
| No Meme | >>no_meme User [Reason] | Takes away all Discord functionality other than send/read message from a user identified by mention or ID, optionally with a reason. |
| Silent Ban | >>silent_ban User [Reason] | Bans the user identified by mention or ID silently, optionally with a reason. Ban is still logged, however the ban message is deleted from chat. |
| Setup Guild | >>setup_guild | Registers the guild into the database. |
| Setup Mute | >>setup_mute Role | Sets up the mute role for HardMute and SoftMute. |

While also being able to do the following public commands:

| Command | Syntax | Definition |
| :-: | :- | :- |
| Ping | >>ping | Times how long to took to send a message, then edits the sent message with how long it took in milliseconds. |
| Profile Picture | >>profile_picture User | Sends a 512x512px PNG of the user asked for, identified by mention or ID. |
| Profile Picture | >>pfp User | Alias for `>>profile_picture`. |
| Reminder | >>remind TimeSpan [Reason] | Reminds the user of an optionally provided reason in a C# Timespan format. |
| Reminders | >>reminders | Shows all the listed reminders that are currently set in the form a paged embed. |
| Repo | >>repo | Sends the repo of Tomoe. |
| Repo | >>repository | Alias to `>>repo`. |
| Repo | >>github | Alias to `>>repo`. |
| Repo | >>gitlab | Alias to `>>repo`. |
| Role Info | >>role_info Role | Sends all information about the role indentified by ping, name or ID in an embed. Also shows who has the role, along with a member count. |

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
| setup_logging | Halfway Done | Static or Interactive |
| setup_ignored_channels | :x: | Static or Interactive |
| setup_admins | :x: | Static or Interactive |
| setup_anti_invite | :x: | Static or Interactive |
| setup_anti_everyone | :x: | Static or Interactive |
| setup_anti_duplicate | :x: | Static or Interactive |
| setup_max_lines | :x: | Static or Interactive |
| setup_max_mentions | :x: | Static or Interactive |
| setup_auto_dehoist | :x: | Static or Interactive |
| setup_auto_raidmode | Halfway Done | Static or Interactive |
| allow_invite | :x: | Static |
| strike | :x: | Static |
| nomeme | :x: | Static |
| temp_mute | :x: | Static |
| perm_mute | :heavy_check_mark: | Static |
| kick | :heavy_check_mark: | Static |
| temp_ban | :heavy_check_mark: | Static |
| perm_ban | :heavy_check_mark: | Static |
| report | :x: | Static |
| tag | :x: | Static |
| remind | :heavy_check_mark: | Static |
| reminders | :heavy_check_mark: | Static |
| pfp | :heavy_check_mark: | Static |
| role_info | :heavy_check_mark: | Static |
| repo | :heavy_check_mark: | Static |
| help | :heavy_check_mark: | Static |

*Naming may change for simplicity.

## Why did you chose "Tomoe" as the bots name?

Simple! I originally found the name from the anime [Kamisama Kiss](https://www.funimation.com/shows/kamisama-kiss/). Tomoe is a wild fox yokai that Mikage had as his previous familiar. While he is taking care of Nanami, he's very strict yet kind with her. I'm taking the strict part from him, and putting it into the bot.
