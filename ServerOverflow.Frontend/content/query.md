ServerOverflow queries support operators, which work like this:
- Want to find a server with a specific MC version? You can use `version:1.19.2`
- Want to include servers with non-zero player count? You can use `-online:0`
- Want to be even more specific about it? You can use `version:"Paper 1.20.1"`
- Just want to search for keywords in MOTDs? Type it out `like so`
- Want to include only online servers? You can use `isOnline:true`
- Want to match multiple words? Put it in quotes `"like so"`

## Operators
### Servers
- `botJoined (boolean)` - got joined by the snooper bot
- `allowsReporting (boolean)` - chat reporting is enabled
- `hasForge (boolean)` - forge is installed
- `onlineMode (boolean)` - online mode status
- `whitelist (boolean)` - whitelist status
- `online (-integer)` - online players
- `max (-integer)` - maximum players
- `protocol (-integer)` - protocol version
- `ip (-string)` - target IP address
- `port (-integer)` - target port
- `version (string)` - minecraft version
- `hasPlayer (string)` - has player sighting
- `hasMod (-string)` - has forge mod installed

### Audit logs
- `action (-string/integer)` - name of the action performed or it's ID
- `timestamp (>integer)` - unix timestamp of the performed action
- `!default (string)` - matches any arbitrary action attribute

### Exclusions
- `ip (-string)` - IP address or range included in the exclusion

## Denominators
- `>` - comparison operators (`>`, `<` and `=`) are allowed
- `-` - reversal of the operation via `-` is allowed

## Types
- `boolean` - Binary `true` or `false`
- `integer` - An unsigned number
- `string` - String of text