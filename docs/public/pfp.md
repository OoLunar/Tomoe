# Pfp
## Summary
Gets the profile picture of a user or userid.

## Description
Gets the user's profile picture and returns it in the desired format. The user can be chosen by mention or by user id. Returns the user's pfp when it's just `>>pfp` standalone.

## Overloads
- `pfp`
- `pfp <user>`
- `pfp <user> [imageFormat = png]`
- `pfp <user> [imageSize = 1024] [imageFormat = png]`

## Examples
Gets the users pfp.

```
>>pfp 
```

![pfp_overload_1.png](/docs/images/pfp_overload_1.png)

<hr>

Gets the requested users pfp through id.
```
>>pfp 481314095723839508
```

![pfp_overload_2.png](/docs/images/pfp_overload_2.png)

<hr>

Gets the requested users pfp through mention.
```
>>pfp @Lunar's Dev Testing Account#1214
```

![pfp_overload_3.png](/docs/images/pfp_overload_3.png)

<hr>

Gets the requested users pfp through mention, sized to 64x64px.
```
>>pfp @Tomoe#1033 64
```

![pfp_overload_4.png](/docs/images/pfp_overload_4.png)

<hr>

Gets the requested users pfp through id, in jpeg format.
```
>>pfp 735620710948405248 jpeg
```

![pfp_overload_5.png](pfp_overload_5.png)

<hr>

Gets the requested users pfp through mention, sized to 128x128px, in webp format.
```
>>pfp @Lunar#9860 128 webp
```

![pfp_overload_6.png](/docs/images/pfp_overload_6.png)

## Aliases
- profile_picture
- avatar

## Remarks
Please avoid using the jpeg format. Png or Webp is a great alternative. 

Available [image formats](https://discord.com/developers/docs/reference#image-formatting-image-formats) are:
- png
- jpeg
- webp
- gif
- unknown
- auto

`imageSize` must be a power of 2.