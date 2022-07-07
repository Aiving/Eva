using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Eva.Commands
{
    [Group("client")]
    [Description("Группа команд, связанная непосредственно с самим ботом.")]
    public class ClientCommands : BaseCommandModule
    {
        [Command("user")]
        [Description("Обычная информация про аккаунт бота.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task GetClientUserInformationAsync(CommandContext Context)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
              .WithTitle("Информация об аккаунте бота:")
              .WithUrl($"https://discord.com/users/{Context.Client.CurrentUser.Id}")
              .WithDescription(@$"**Айди:** `{Context.Client.CurrentUser.Id}`
**Юзернейм:** `{Context.Client.CurrentUser.Username}`
**Дискриминатор:** `{Context.Client.CurrentUser.Discriminator}`
**Ссылка на аватар:** [`{Context.Client.CurrentUser.AvatarHash}.png`]({Context.Client.CurrentUser.AvatarUrl})
**Создано:** `{string.Format("{0:hh}:{0:mm}, {0:dd}.{0:mm}.{0:yyyy}", Context.Client.CurrentUser.CreationTimestamp)}`
**Обо мне:** {Context.Client.CurrentApplication.Description ?? "Ничего нет."}")
              .WithThumbnail(Context.Client.CurrentUser.AvatarUrl)
              .WithColor(DiscordColor.Black)
              .WithTimestamp(DateTime.Now)
              .WithFooter(
                $"Запрошено {Context.Message.Author.Username}#{Context.Message.Author.Discriminator}.",
                Context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
              );
            await Context.RespondAsync(embed);
        }

        [Command("gateway")]
        [Description("Выводит немного информации про гейт-подключение бота.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task GetGatewayInformationAsync(CommandContext Context)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
              .WithTitle("Информация об гейт-подключении бота:")
              .WithDescription(@$"**Версия API:** `{Context.Client.GatewayVersion}`
**Пинг:** `{Context.Client.Ping}ms`
**Количество шардов:** `{Context.Client.ShardCount}`")
              .WithThumbnail(Context.Client.CurrentUser.AvatarUrl)
              .WithColor(DiscordColor.Black)
              .WithTimestamp(DateTime.Now)
              .WithFooter(
                $"Запрошено {Context.Message.Author.Username}#{Context.Message.Author.Discriminator}.",
                Context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
              );
            await Context.RespondAsync(embed);
        }

        [Command("application")]
        [Description("Выводит информацию об C# проекте. Туда входят: фреймворк и установленные пакеты.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task GetApplicationInformationAsync(CommandContext Context)
        {
            XDocument csproj = XDocument.Load("Eva.csproj");
            string frameworkUsed = csproj.Descendants("TargetFramework").ToList()[0].Value;
            // List<string> ignoredWarnings = csproj.Descendants("NoWarn").ToList()[0].Value.Split("; ").Select((warning) => $"`{warning}`").ToList();
            List<string> packageList = csproj.Descendants("PackageReference").ToList().OrderBy((XElement package) => package.Attribute("Include").Value).Select((Package, Index)
              => $"{Index + 1}. `{Package.Attribute("Include").Value}` [**{Package.Attribute("Version").Value}**]").ToList();
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
              .WithTitle("Информация об C# проекте:")
              .WithDescription($"**Использованный фреймворк:** `{frameworkUsed}`") /* \n**Ignored warning codes:** {string.Join(", ", ignoredWarnings)} */
              .AddField("Установленные пакеты", string.Join('\n', packageList))
              .WithThumbnail(Context.Client.CurrentUser.AvatarUrl)
              .WithColor(DiscordColor.Black)
              .WithTimestamp(DateTime.Now)
              .WithFooter(
                $"Запрошено {Context.Message.Author.Username}#{Context.Message.Author.Discriminator}.",
                Context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
              );
            await Context.RespondAsync(embed: embed.Build());
        }
    }
}
