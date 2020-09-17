# Moderation

## Ban

Banning is a relatively useful tool when dealing with trolls, raging people or for having to make the tough decision on a close friend. Tomoe tries his hardest to make banning all the more easier by not only logging all bans on the server, but gives you a tool to ban as well. Tomoe can ban in two ways:

### Banning by Mentioning the Victim.

<hr>
Syntax:

    >>ban <user> [reason]

The user is a required argument, while the reason is optional. Simple enough usage, as such:
    
    >>ban @Lunar#9860

Easy peasy lemon squeezy. However, you can also provide a reason:

    >>ban @Lunar#9860 Quit causing a ruckus on the server.

Again, quick and simple. But... What if the user isn't present on the server? Another helpful tool that Discord Developer Mode provides is getting the user ID. With this, you can do...

### Banning by Victim's ID.

<hr>
Syntax:

    >>ban <user ID> [reason]

The user ID is required, but the reason is optional.

    >>ban 336733686529654798

And with a reason

    >>ban 336733686529654798 Quit causing a ruckus on the server.

Done and done. Tomoe will log the ban into it's proper logging channel provided that logging is setup. Upon banning, Tomoe will send a private message, or DM, to the user that looks like such:
![image](https://user-images.githubusercontent.com/46751150/93417282-53d7d980-f86d-11ea-8f3f-66e7b4faaaab.png)

### For the visual learners
<hr>
TO BE WRITTEN (Commands need to function all the way before screenshots can be taken)
