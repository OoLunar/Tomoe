# Ban
## Summary
Bans a member from the guild.

## Description
When banning, Tomoe will attempt to send the victim a private message before banning, as such:
![ban_overload_1.png](docs/images/ban_overload_1.png)

## Overloads
- `ban <user id>`
- `ban <user mention>`
- `ban <user> [banReason]`
- `ban <user> [pruneDays]`
- `ban <user> [pruneDays = 7] [banReason]`
- `ban <multiple user ids>`
- `ban <multiple user mentions>`
- `ban [banReason] <multiple users>`
- `ban [pruneDays] <multiple users>`
- `ban [pruneDays] [banReason] <multiple user mentions>`

See Remarks on why mass banning requires the users to be last.

## Examples
Banning a user for no reason.
```
ban @Lunar's Dev Testing Account#1214
```
![ban_overload_2.png](/docs/images/ban_overload_2.png)

<hr>

Banning a user for shitposting.
```
ban 735620710948405248 Shitposting
```
![ban_overload_3.png](/docs/images/ban_overload_3.png)
<hr>

Banning a user for no reason, removing all their messages in the past 7 days.
```
ban @Lunar's Dev Testing Account#1214 7
```
![ban_overload_4.png](docs/images/ban_overload_4.png)

<hr>

Banning a user for shitposting and removing all their messages in the past 7 days.
```
>>ban @Lunar's Dev Testing Account#1214 7 Shitposting
```
![ban_overload_5_png](docs/images/ban_overload_5.png)

## Aliases
None.

## Remarks
Due to how DSharpPlus (The Discord library that Tomoe is using) is designed, it wishes to take arrays or lists using the `params` attribute. The `params` attribute requires for the argument to be the last in the function. This is why you're forced to mention all the users last.

#### When mass banning, how do I provide a reason longer than one word?
Quote it, as such:
```
>>ban "Continuing to shitpost when told to stop" @Lunar#9860 @Lunar's Dev Testing Account#1214
```

#### What happens when Tomoe can't DM the victim?
This:
![ban_overload_6.png](docs/images/ban_overload_6.png)