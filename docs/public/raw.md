# Raw
## Summary
Gets the raw version of the message.

## Description
Obtaining the raw version of the message allows for an easy copy and paste for duplicating messages.

## Overloads
- `>>raw <message id or jump link>`

## Examples

With a message jump link
```
>>raw https://discord.com/channels/779027360347324447/779027360347324450/786386634886742016
```
![raw_overload_1.png](/docs/images/raw_overload_1.png)![raw_overload_2.png](/docs/images/raw_overload_2.png)

<hr>
With a message id
```
>>raw 786400530570018826
```

![raw_overload_1.png](/docs/images/raw_overload_1.png)![raw_overload_3.png](/docs/images/raw_overload_3.png)

## Aliases
- source

## Remarks
Doesn't work with embeds at the moment. Note how mentions are filtered.