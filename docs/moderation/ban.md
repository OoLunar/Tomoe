# Moderation

## Ban

Banning is a relatively useful tool when dealing with trolls, raging people or for having to make the tough decision on a close friend. Tomoe tries his hardest to make banning all the more easier by not only logging all bans on the server, but gives you a tool to ban as well. Tomoe can ban in two ways:

### Banning by Mentioning the Victim

<hr>
Syntax:

```
>>ban <user> [prune days] [reason]
```

The user is a required argument, while the prune days and reason is optional. Prune days removes all the users messages over a set amount of days. Messages over a week old cannot be pruned. Simple enough usage, as such:

```
>>ban @Lunar#9860
```

Now, lets remove all my messages from the past 4 days

```
>>ban @Lunar#9860 4
```

Easy peasy lemon squeezy. However, you can also provide a reason:

```
>>ban @Lunar#9860 7 Quit causing a ruckus on the server.
```

Again, quick and simple. But... What if the user isn't present on the server? Another helpful tool that Discord Developer Mode provides is getting the user ID. With this, you can do...

### Banning by Victim's ID

<hr>
Syntax:

```
>>ban <user ID> [prune days] [reason]
```

The user ID is required, but the reason is optional.

```
>>ban 336733686529654798
```

And with a reason

```
>>ban 336733686529654798 Quit causing a ruckus on the server.
```

Done and done. Tomoe will log the ban into it's proper logging channel provided that logging is setup. Upon banning, Tomoe will send a private message, or DM, to the user that looks like such:
![image](https://user-images.githubusercontent.com/46751150/93417282-53d7d980-f86d-11ea-8f3f-66e7b4faaaab.png)

### For the visual learners
<hr>

**Banning by Mentioning the Victim**

![image](https://user-images.githubusercontent.com/46751150/93636680-f7360500-f9b9-11ea-84a2-e7921f7470c6.png)

Removing their messages from the past 4 days:

![image](https://user-images.githubusercontent.com/46751150/93636775-1df43b80-f9ba-11ea-82c3-ab4a7c038744.png)

Providing a reason:

![image](https://user-images.githubusercontent.com/46751150/93636880-467c3580-f9ba-11ea-9a4d-bb10da158cd1.png)

Removing their messages from the past 2 days while providing a reason:

![image](https://user-images.githubusercontent.com/46751150/93637023-8216ff80-f9ba-11ea-9874-823c40ccc286.png)

**Banning by Victim's ID**

![image](https://user-images.githubusercontent.com/46751150/93637209-d02c0300-f9ba-11ea-90f7-58f67ca44a7e.png)

Removing their messages from the past 7 days:

![image](https://user-images.githubusercontent.com/46751150/93656742-29635900-f9f2-11ea-9216-2b780975d7da.png)

Providing a reason:

![image](https://user-images.githubusercontent.com/46751150/93656815-c9b97d80-f9f2-11ea-89a1-d0e65f10e171.png)

Removing their messages from the past 3 while providing a reason:

![image](https://user-images.githubusercontent.com/46751150/93656764-6b8c9a80-f9f2-11ea-882b-7c365e294eb5.png)
