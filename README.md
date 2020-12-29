# How to setup Tomoe

If you wish to host Tomoe yourself, skip on down to prerequisites. If you just want Tomoe to join your guild: https://discord.com/api/oauth2/authorize?client_id=481314095723839508&permissions=8&scope=bot

## Prerequisites

Tomoe uses dotnet 5.0. You can get it from [Microsoft](https://dotnet.microsoft.com/download/dotnet/5.0). I am currently developing Tomoe Visual Studio Code and [Windows Terminal](https://www.microsoft.com/en-us/p/windows-terminal/9n0dx20hk701). Tomoe works on Windows 10 Home and Ubuntu 20.04/20.10. Hopefully I can test on mac soon. While everything should theoretically work, there are no promises. If anything isn't working correctly, please open an issue and let me know. Mono is not supported due to DSharpPlus choosing not to support it.

## Setup

* Run a PostgreSQL instance that is reachable by the bot

``` bash
git clone https://github.com/OoLunar/Tomoe.git
cd Tomoe
```

Be sure to edit your res/config.jsonc, then:

``` bash
dotnet run # First run might take a minute since it has to download dependencies and whatnot.
```

### Docker

1. Edit the config file `res/config.jsonc` to suit your needs
2. Run the actual bot (how you do this is up to you but make sure that you have a volume from `./res` to `/Tomoe/res` inside the container)

**The Docker image is `ghcr.io/oolunar/tomoe`**

# Tomoe, The Discord Moderation Bot

Tomoe is all about moderation for big time servers and aims to be Vortex, Zeppelin and Carl all in one.

## What is Tomoe written in?

C#. However once Tomoe is finished 100%, I plan on creating a separate branch and rewriting Tomoe in Rust for efficency. Please keep in mind that plans may change.

## What can Tomoe do?

As of this minute, Tomoe can do the following moderation commands:

| Command | Syntax | Definition |
| :-: | :- | :- |
| Ban | >>ban \<User> [Reason] | Bans a guild member identified by mention or id, optionally with a reason. |
| Kick | >>kick \<User> [Reason] | Kicks a guild member identified by mention or id, optionally with a reason. |
| Mute | >>mute \<User> [Reason] | Mutes a user provided `>>config mute` has been set. |
| Pardon | >>pardon \<CaseId> [Reason] | Removes a strike from a user, using the case ID to find the strike. |
| Strike | >>strike \<User> [Reason] | Adds a strike to the users record. |
| Unban | >>unban \<User> [Reason] | Unbans a guild member identified by mention or id, optionally with a reason. |
| Unmute | >>unmute \<User> [Reason] | Unmutes a guild member. |

While also being able to do the following public commands:

| Command | Syntax | Definition |
| :-: | :- | :- |
| Bot Info | >>bot_info | Sends information about the current bot instance. |
| Flip | >>flip | Sends heads or tails using `System.Random()` . Optionally allows you to choose between multiple choices. |
| Guild Icon | >>guild_icon |Sends the server icon in 1024x1024px png format. |
| Invite | >>invite | Sends the link to add Tomoe to a guild. |
| Profile Picture | >>profile_picture User | Sends a 1024x1024px PNG of the user asked for, identified by mention or id. |
| Ping | >>ping | Times how long to took to send a message, then edits the sent message with how long it took in milliseconds. |
| Raw | >>raw \<Message id> | Gets the raw version of a message id. |
| Reminders | >>remind \<TimeSpan> [Content] | Reminds the user in the specified time to do `xyz`. |
| Repo | >>repo | Sends the repo of Tomoe. |
| Role Info | >>role_info \<Role> | Sends all information about the role indentified by ping, name or id in an embed. Also shows who has the role, along with a member count. |
| Server Info | >>server_info | Sends general information about the guild. |
| Support | >>support | Sends the link to the support guild. |
| Tags | >>tag | A full tag system. See `>>help tag` for a more indepth list. |

Some might have noticed that all commands are snake_case and PascalCase. This is best for simplicity and consistancy.

## Anything notable about Tomoe?

Tomoe aims to be the best moderation and quality of life bot, and nothing more. It supports PostgresSQL for it's database, however drivers can be added. See https://github.com/OoLunar/Tomoe/pull/1 for more info about the drivers.

## What's to come?

As mentioned previously, Tomoe aims to be the #1 bot choice for Discord Moderation. This means that any and all moderation commands that Vortex, Zeppelin and Carl can do, Tomoe will try to implement. Here's the current road map:

| Command | Implemented |
|:-|:-:|
| ban | :heavy_check_mark: |
| kick | :heavy_check_mark: |
| mute | :heavy_check_mark: |
| meme_ban | :x: |
| vc_ban | :x: |
| strike | :heavy_check_mark: |
| strike history | :heavy_check_mark: |
| pardon| :heavy_check_mark: |
| temp_ban | :heavy_check_mark: |
| temp_mute | :heavy_check_mark: |
| temp_meme_ban | :x: |
| temp_vc_ban | :x: |
| unban | :heavy_check_mark: |
| unmute | :heavy_check_mark: |
| unmeme_ban | :x: |
| unvc_ban | :x: |
| report | :x: |
| config mute | :heavy_check_mark: |
| bot_info | :heavy_check_mark: |
| flip | :heavy_check_mark: |
| guild_icon | :heavy_check_mark: |
| invite | :heavy_check_mark: |
| pfp | :heavy_check_mark: |
| raw | :heavy_check_mark: |
| reminders | :heavy_check_mark: |
| reminders list | :heavy_check_mark: |
| reminders remove | :heavy_check_mark: |
| repo | :heavy_check_mark: |
| role_info | :heavy_check_mark: |
| server_info | :heavy_check_mark: |
| support | :heavy_check_mark: |
| tag | :heavy_check_mark: |
| tag alias | :heavy_check_mark: |
| tag all | :heavy_check_mark: |
| tag claim | :heavy_check_mark: |
| tag create | :heavy_check_mark: |
| tag delete | :heavy_check_mark: |
| tag delete_alias | :heavy_check_mark: |
| tag delete_all_aliases | :heavy_check_mark: |
| tag edit | :heavy_check_mark: |
| tag exist | :heavy_check_mark: |
| tag get_aliases | :heavy_check_mark: |
| tag get_author | :heavy_check_mark: |
| tag is_alias | :heavy_check_mark: |
| tag realname | :heavy_check_mark: |
| tag transfer | :heavy_check_mark: |
| tag user | :heavy_check_mark: |

*Naming may change for simplicity.

## Why did you chose "Tomoe" as the bots name?

Simple! I originally found the name from the anime [Kamisama Kiss](https://www.funimation.com/shows/kamisama-kiss/). Tomoe is a wild fox yokai that Mikage had as his previous familiar. While he is taking care of Nanami, he's very strict yet kind with her. I'm taking the strict part from him, and putting it into the bot.
