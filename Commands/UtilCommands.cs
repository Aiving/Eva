// importing installed packages
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Eva.Commands
{
    [Group("util")]
    [Description("Команды, предназначенные для упрощения тех или иных задач.")]
    public class UtilCommands : BaseCommandModule
    {
        /* [Command("colorify")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendColorAsync(CommandContext context, DiscordColor color)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle(":")
                .WithColor(color)
                .WithTimestamp(DateTime.Now)
                .WithFooter(
                  $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                   context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                 );
            await context.RespondAsync(embed: embed.Build());
        } */

        [Command("btoa")]
        [Description("Конвертирует текст в base64.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendBase64ToAscii(CommandContext context, [RemainingText] string data)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(data);
            string text = Encoding.UTF8.GetString(base64EncodedBytes);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Конвертировано из base64 кодировки:")
                .WithDescription(text)
                .WithColor(DiscordColor.Black)
                .WithTimestamp(DateTime.Now)
                .WithFooter(
                  $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                   context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                 );
            await context.RespondAsync(embed: embed.Build());
        }

        [Command("atob")]
        [Description("Конвертирует из base64 в текст.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendAsciiToBase64(CommandContext context, [RemainingText] string data)
        {

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(data);
            string text = Convert.ToBase64String(plainTextBytes);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Конвертировано в base64 кодировку:")
                .WithDescription(text)
                .WithColor(DiscordColor.Black)
                .WithTimestamp(DateTime.Now)
                .WithFooter(
                  $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                   context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                 );
            await context.RespondAsync(embed: embed.Build());
        }

        [Command("garbage")]
        [Description("Делает текст \"непереводимым\".")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendGarbagedAsync(CommandContext context, [RemainingText] string text)
        {
            List<char> letters = text.ToList();
            IEnumerable<string> garbagedLetters = letters.Select((letter) => $"{letter}\u17b5\u200b");
            string garbagedText = string.Join("", garbagedLetters);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("\"Непереводимый\" текст:")
                .WithDescription(Formatter.BlockCode(garbagedText))
                .WithColor(DiscordColor.Black)
                .WithTimestamp(DateTime.Now)
                .WithFooter(
                  $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                   context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                 );
            await context.RespondAsync(embed: embed.Build());
        }

        [Command("opengraph")]
        [Description("Парсит страницу на наличие `og:`-тегов.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendOpenGraphAsync(CommandContext context, Uri url)
        {
            HttpClient client = new();
            string body = await client.GetStringAsync(url);
            HtmlParser parser = new();
            IHtmlDocument document = parser.ParseDocument(body);
            List<IElement> metas = document.Head.QuerySelectorAll("meta").ToList();
            IEnumerable<string> openGraphTags = metas.Where((meta) => meta.Attributes.Any((attr) =>
            {
                string[] values = new string[] { "title", "description", "theme-color" };
                return attr.Value.StartsWith("og:") || values.Contains(attr.Value);
            }))
                .Select((meta) => meta.OuterHtml);
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Результат Open Graph парсинга:")
                .WithDescription(Formatter.BlockCode(string.Join("\n", openGraphTags), "html"))
                .WithColor(DiscordColor.Brown)
                .WithTimestamp(DateTime.Now)
                .WithFooter(
                  $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                   context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                 );
            await context.RespondAsync(embed: embed.Build());

        }
    }
}