# Average's Appointer
Support me & this plugin's (along with several others) development on Ko.Fi: [Here!](https://ko-fi.com/averageterraria)

This is a complete rewrite of a rewrite! My (old? not really) plugin known as RankSystem has received yet ANOTHER rewrite. What's different this time? Well, a FUCK-ton. This is a still a simple, automatic rank progression system based on user playtime but a variety of things are different. It now depends on CSF.Net.TShock along with Auxiliary. Why? For two reasons, CSF.Net makes command management much easier, and Auxiliary allows us to utilize a Mongo database, which is *much* faster and flat out better. Code has been re-written from scratch and works at least 100x better than RankSystem. 

### Features
- Time based rank progression system (unlimited ranks!)
- Built-in AFK system

## Config Explained
(tshock/RankSystem.json)

```json
{
  "StartGroup": "default",
  "DoesCurrencyAffectRankTime": false,
  "CurrencyMultiplier": 1,
  "UseAFKSystem": true,
  "Groups": [
    {
      "Name": "vip",
      "NextRank": "member",
      "Cost": 1000
    },
    {
      "Name": "member",
      "NextRank": "admin",
      "Cost": 5000
    }
  ]
}
```
Very easy and self-explanatory, but here is an explanation of each field regardless.

`StartGroup` is the initial rank your users get put into when they register. Ideally, leave this at default.
`doesCurrencyAffectRankTime` and `currencyAffect` are complementary. If set to true, all of the rank times are also affected by the user's economy level. the `currencyAffect` is a percentage value. For example, if it is set to 5, then 5% of the user's balance adds to the user's playtime (in seconds). If set to 100, then the user balance will add the entire balance (in seconds) to the user's playtime.
`Groups` has three main values. Each group has a `Name` such as `"member"`, and `NextRank` is the group that will succeed that group. The `Cost` is how much playtime the group will take to rank up to.
 
## Commands List 

| Command        |Description           |Usage  |Permission    |
| ------------- |:-------------:| :-----:| :-----------:|
| /rank    |Shows the user's playtime and next rank info | /rank (optional: `playerName`) (Alias: /check, /rankup | tbc.user |
