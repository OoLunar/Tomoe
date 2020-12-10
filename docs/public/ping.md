# Ping
## Summary
Checks the latency between the bot and the Discord API Websocket. Best used to see if the bot is lagging.

## Description
Sometimes the bot isn't responding as fast as it should. Pinging can give an idea of whether it's the bot or Discord's API that's acting up. Tomoe uses `CommandContext.Client.Ping` to get the latency

## Overloads
- `ping`

## Examples

```
>>ping
```

![ping.png](/docs/images/ping.png)

## Aliases
None.

## Remarks
May not always be accurate.