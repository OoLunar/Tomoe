# Role Info
## Summary
Gets the permissions of a role, the role color, who has the role and a count of who has the role.

## Description
Role Info gets the following information about a guild role:
  - Role id.
  - Role name.
  - Creation date [in header format (see `Syntax`)](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Date).
  - Role's position in hierarchy.
  - Role's hex color code.
  - Role's mentionability.
  - Role being hoisted.
  - Role being managed by an integration.
  - Role's guild permissions.
  - Count of who has the role.
  - Who has the role.

Note that if the embed is too large, the last field may get trimmed a bit so that the embed can be sent.

![image](https://user-images.githubusercontent.com/46751150/93503456-b5d02780-f8dd-11ea-9db9-ad31945032b1.png)

## Syntax
The command can be executed in three ways.

Through mention (not recommended)
```
>>role_info @Developer
```

Through ID (recommended)
```
>>role_info 744427304880701470
```

Through name (beta)
```
>>role_info Developer
```

Getting the role by the name is still very much being tested. It's only recommended if it's the only role with said name. If multiple roles with the same name are present, the first role found with the request name will be sent. This will be fixed soon.

### Can I get the @everyone role info?

Yes. Here's how

```
>>role_info <guild_id>
```
or
```
>>role_info everyone
```

Through ID:

![image](https://user-images.githubusercontent.com/46751150/93504111-a3a2b900-f8de-11ea-89d6-30dbc928e5a7.png)

Through name:

![image](https://user-images.githubusercontent.com/46751150/93504193-c503a500-f8de-11ea-981b-85c05e39485b.png)
