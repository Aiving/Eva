// importing installed packages
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Eva.Commands.Entities;
// importing custom namespaces
using Eva.Util;
using GenshinSharp;
using GenshinSharp.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eva.Commands.GenshinImpact
{
    [Group("genshin")]
    [Description("Группа команд, связанных с игрой Genshin Impact.")]
    public class GenshinImpactCommands : BaseCommandModule
    {
        private readonly GenshinClient client = new();

        [Command("card")]
        [Description("Выводит информацию об игровом аккаунте. Туда включается такая информация, как: статистика, полученные персонажи и их уровень, уровни исследования локаций, а также дома в чайнике.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendCardAsync(CommandContext context, string uid, string mode = null)
        {
            try
            {
                Card card = await client.GetUserCardAsync(uid);

                List<string> modes = new() { "statistic", "world", "homes" };
                List<string> charactersMode = new() { "characters" };

                List<string> fields = new()
                {
                    string.Join("\n", ProperityGet.GetProperites(card.Statistic).Select(x => $"**{GenshinSharp.Utilities.Util.Translate(string.Join(" ", Regex.Split(x.Name, @"(?<!^)(?=[A-Z])")))}**: `{x.Value}`")),
                    string.Join("\n", card.Homes.Select((x, index) => $"**`Дом {index + 1}`**\n**Уровень доверия**: `{x.Level}`\n**Гостей за всё время**: `{x.VisitedByPlayers}`\n**Высочайшая сила Адептов**: `{x.ComfortCount}`\n**Получено украшений**: `{x.ItemCount}`")),
                    string.Join("\n", card.Characters.OrderBy(s => s.Rarity).ThenBy(s => s.Level).Select((x, index) => $"**{index + 1}.** {GenshinSharp.Utilities.Util.GetEmoji(x.Element.ToLower())} `{x.Name}` `LVL{x.Level}` `{new String('★', x.Rarity > 20 ? 6 : x.Rarity)}`")),
                    string.Join("\n", card.WorldExplorations.Select((x, index) => $"**{index + 1}.** {x.Name} с уровнем {(x.Type.ToLower() == "reputation" ? "репутации" : "исследования")}: `{x.Level}({(double)x.ExplorationPercentage / 10}%)`"))
                };

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Black)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(
                        $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                        context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                    );
                if (charactersMode.Contains(mode))
                {
                    List<string> lines = fields[2].Split("\n").ToList();
                    int counter = 10;
                    List<GroupString> chars = lines.GroupBy(_ => counter++ / 10).Select((x, i) =>
                    {
                        GroupString gr = new() { Index = i, Strings = string.Join("\n", x) };
                        return gr;
                    }).ToList();
                    chars.ForEach(x => embed.AddField($"Персонажи | Страница {x.Index + 1}", x.Strings, false));
                }
                else if (modes.Contains(mode))
                {
                    embed.WithTitle(GenshinSharp.Utilities.Util.FindTitle(mode));
                    embed.WithDescription(fields[GenshinSharp.Utilities.Util.FindDescription(mode)]);
                }
                else if (!modes.Contains(mode))
                {
                    if (fields[0].Length != 0) embed.AddField("Обзор данных", fields[0].Length > 1024 ? $"Слишком длинный текст! Используйте команду: `$:genshin card {uid} statistic`" : fields[0], true);
                    if (fields[1].Length != 0) embed.AddField("Чайник безмятежности", fields[1].Length > 1024 ? $"Слишком длинный текст! Используйте команду: `$:genshin card {uid} homes`" : fields[1], true);
                    if (fields[2].Length != 0) embed.AddField("Исследование мира", fields[3].Length > 1024 ? $"Слишком длинный текст! Используйте команду: `$:genshin card {uid} world`" : fields[3], true);
                    if (fields[3].Length != 0) embed.AddField("Персонажи", fields[2].Length > 1024 ? $"Слишком длинный текст! Используйте команду: `$:genshin card {uid} characters`" : fields[2], false);
                };
                await context.RespondAsync(embed: embed.Build());
            }
            catch (Exception err)
            {
                await context.RespondAsync($"```cs\n{err}\n```");
            };
        }


        [Group("profile")]
        [Description("Выводит информацию об аккаунте пользователя в сообществе.")]
        public class ProfileGroup : BaseCommandModule
        {
            private readonly GenshinClient client = new();

            [GroupCommand]
            [Cooldown(1, 5.0, CooldownBucketType.Guild)]
            public async Task SendProfileAsync(CommandContext context, string cid)
            {
                try
                {
                    BaseData<GenshinAccountsList> card = await client.GetUserAccountsAsync(cid);
                    BaseData<UserInfo> info = await client.GetUserInformationAsync(cid);

                    string profiles = string.Join("\n", card.Data.Accounts.Select((x, index) => $"**{index + 1}.** `{x.Name}` | `{x.Level}LVL` | `{x.Region}` | `UID: {x.UserId}`"));

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                        .WithTitle($"Профиль сообщества `{info.Data.User.Name}`")
                        .WithDescription($"**Уровень**: `{info.Data.User.CurrentLevel.Level}` **`({info.Data.User.CurrentLevel.Experience} опыта)`**")
                        .AddField("Игровой[-ые] аккаунт[-ы]", (profiles.Trim() == "" ? "Нет" : profiles), false)
                        .WithColor(DiscordColor.Black)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter(
                            $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                            context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                        );
                    if (info.Data.User.Avatar != null) embed.WithThumbnail(info.Data.User.Avatar);
                    await context.RespondAsync(embed: embed.Build());
                }
                catch (Exception err)
                {
                    await context.RespondAsync($"```cs\n{err}\n```");
                };
            }

            [Command("search")]
            [Description("Ищет аккаунт в сообществе, содержащий указанный никнейм.")]
            [Cooldown(1, 5.0, CooldownBucketType.Guild)]
            public async Task SendSearchedProfileAsync(CommandContext context, string nickname)
            {
                try
                {
                    BaseData<SearchBase<CommunitySearchUserBase>> data = await client.SearchUserAsync(nickname);
                    string users = string.Join("\n", data.Data.List.Select((x, index) => $"**{index + 1}.** `{Regex.Replace(x.User.UserNameMarked, @"<hoyolab>|<\/hoyolab>|`", string.Empty, RegexOptions.Multiline)}` | `CommunityID: {x.User.CommunityId}`"));

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                        .WithTitle($"Поиск пользователя `{nickname}`")
                        .WithDescription(users)
                        .WithColor(DiscordColor.Black)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter(
                            $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                            context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                        );
                    if (data.Data.List.ToArray()[0].User.Avatar != null) embed.WithThumbnail(data.Data.List.ToArray()[0].User.Avatar);
                    await context.RespondAsync(embed: embed.Build());
                }
                catch (Exception err)
                {
                    await context.RespondAsync($"```cs\n{err}\n```");
                };

            }
        }
    }
}