# Role Info
## Summary
Gets the permissions of a role, the role color, who has the role and a count of who has the role.

## Description
Role Info gets the following information about a guild role:
  - Role id.
  - Role name.
  - Creation date [in HTTP header format (see `Syntax`)](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Date).
  - Role's position in hierarchy.
  - Role's hex color code.
  - Role's mentionability.
  - Role being hoisted.
  - Role being managed by an integration.
  - Role's guild permissions.
  - Count of who has the role.
  - Who has the role.

Note that if the embed is too large, the last field may get trimmed a bit so that the embed can be sent.

## Overloads
- `>>roleinfo <role ping>`
- `>>roleinfo <role id>`
- `>>roleinfo <role name>`

## Examples
By ping
```
>>roleinfo @Mod
```
![role_info_overload_1.png](/docs/images/role_info_overload_1.png)

<hr>

By id
```
>>roleinfo 777258375007305759
```
![role_info_overload_2.png](/docs/images/role_info_overload_2.png)

<hr>

By name. See remarks
```
>>roleinfo mod
```
![role_info_overload_3.png](role_info_overload_3.png)

## Aliases
- `roleinfo`
- `ri`

## Remarks
Getting the role by the name is still very much being tested. It's only recommended if it's the only role with said name. If multiple roles with the same name are present, the first role found with the request name will be sent. This will be fixed soon.

#### Can I get the @everyone role info?

Yes. Here's how

```
>>role_info <guild_id>
```
or
```
>>role_info everyone
```