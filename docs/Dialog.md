# Documentation for dialog.json

## Syntax

All variables are bash-like snake case. I. E `$timestamp` or `$issuer_mention` . The variable names are pretty straight forward, however we'll always keep a list of all variables names and purpose.

## Variables available

### Global:

| Name | Purpose |
| - | - |
| $issuer               | Mentions the person who issued the command.                                               |
| $issuer_id            | The id of $issuer                                                                         |
| $issuer_nick          | Gets the nickname of $issuer. If there is no nickname set, it falls back to the username. |
| $issuer_username      | The username of $issuer                                                                   |
| $issuer_discriminator | Gets the discriminator of $issuer                                                         | 
| $victim               | Mentions the victim of $issuer's command.                                                 |
| $victim_id            | The id of $victim                                                                         |
| $victim_nick          | Gets the nickname of $victim. If there is no nickname set, it falls back to the username. |
| $victim_username      | The username of $victim                                                                   |
| $victim_discriminator | Gets the discriminator of $victim                                                         |
| $reason               | Gets $issuer's reason for executing the command. May not always be present.               |
| $timestamp            | Gets the current or related timestamp of when the action occured.                         |
| $null                 | Replies with nothing.                                                                     |
| \n                    | Ascii new line.                                                                           |

### Guild:

| Name | Purpose |
| - | - |
| $guild_name   | The guild's name          |
| $guild_id     | The guild's id            |
| $channel      | Mentions the channel      |
| $channel_id   | Gets the $channel id.     |
| $channel_name | Gets the $channel name.   |

### Per case:

| Name | Action/Event Required | Purpose |
| - | - | - |
| $old_message          | Message Updated, Message Deleted, Message Bulk Deleted                    | Gets the old message's content.
| $old_message_id       | Message Updated, Message Deleted, Message Bulk Deleted                    | Gets the old message's id. |
| $new_message          | Message Recieved, Message Updated, Message Deleted, Message Bulk Deleted  | Gets the new message's content. |
| $new_message_id       | Message Recieved, Message Updated, Message Deleted, Message Bulk Deleted  | Gets the new message's id. |
| $action               | Ban, TempBan, Kick, TempMute, Mute, Temp No Meme, No Meme, Strike         | Gets the action done. |
| $past_action          | Ban, TempBan, Kick, TempMute, Mute, Temp No Meme, No Meme, Strike         | Gets the action done in past tense.
| $required_guild_permission | Any Moderation Command | The required permission set for either the bot or the user to successfully use the command. |
| $old_role             | Setup Mute, Setup No Meme, Setup No VC                                    | Gets the role name. **Does NOT mention the role.** Can only be used when a new role is assigned to said commands. |
| $old_role_id          | Setup Mute, Setup No Meme, Setup No VC                                    | Gets the role id. Can only be used when a new role is assigned to said commands. |
| $old_role_mention     | Setup Mute, Setup No Meme, Setup No VC                                    | Mentions the old role. Can only be used when a new role is assigned to said commands. |
| $new_role             | Setup Mute, Setup No Meme, Setup No VC                                    | Gets the role name. **Does NOT mention the role.** |
| $new_role_id          | Setup Mute, Setup No Meme, Setup No VC                                    | Gets the role id. |
| $new_role_mention     | Setup Mute, Setup No Meme, Setup No VC                                    | Mentions the new role. |
