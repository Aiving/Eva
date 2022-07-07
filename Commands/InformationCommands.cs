using Eva.Util;
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SkiaSharp;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Net.Http;

namespace Eva.Commands
{
    [Group("info")]
    [Description("Группа команд, чьей задачей является получение различного рода информации и её выведение в удобном и читабельном виде.")]
    public class InformationCommands : BaseCommandModule
    {
        [Command("card")]
        [Description("Показывает некоторую информацию о пользователе в стиле дискорд popup'a.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendCardAsync(CommandContext context)
        {
            DiscordUser author = context.User;
            DiscordMember member = context.Member;
            HttpClient client = new HttpClient();
            HttpResponseMessage responseMessage = await client.GetAsync(author.AvatarUrl);
            HttpContent httpContent = responseMessage.Content;
            Stream avatar = await httpContent.ReadAsStreamAsync();

            SKTypeface typeface1 = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
            SKTypeface typeface2 = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

            int modifier = 5;
            int width = 300 * modifier;
            int height = 182 * modifier;
            int radius = 8 * modifier;
            int padding = 16 * modifier;
            int borderSize = 92 * modifier;
            int avatarSize = 80 * modifier;
            int borderWidth = 6 * modifier;
            int primaryText = 24 * modifier;
            int secondaryText = 18 * modifier;
            int spacing = 5 * modifier;
            int circle = 50 * modifier;
            int banner = padding + ((borderSize - (borderWidth * 2)) / 2);
            string tag = $"{author.Username}#{author.Discriminator}";

            SKColor color = SKColor.Parse(member.Color.ToString());

            SKPaint background = new SKPaint { Color = new SKColor(24, 25, 28) };
            SKPaint bannerBackground = new SKPaint { Color = color };
            SKPaint primary = new SKPaint { TextSize = primaryText, Color = new SKColor(255, 255, 255), Typeface = typeface1 };
            SKPaint secondary = new SKPaint { TextSize = secondaryText, Color = new SKColor(185, 187, 190), Typeface = typeface2 };

            int nicknameWidth = 0;
            if (!(member.Nickname is null)) nicknameWidth = ((int)primary.MeasureText(member.Nickname)) + (padding * 2);
            int usernameWidth = ((int)secondary.MeasureText(tag)) + (padding * 2);

            if (nicknameWidth > width) width = nicknameWidth;
            if (usernameWidth > width && usernameWidth > nicknameWidth) width = usernameWidth;

            SKBitmap bitmapNew = new(width, height);
            SKCanvas canvas = new(bitmapNew);

            SKRect backgroundRect = new(0, 0, width, height);
            SKRect bannerRect1 = new(0, padding, width, banner);
            SKRect bannerRect2 = new(0, 0, width, banner);
            SKRect borderRect = new(padding, padding, borderSize - borderWidth, borderSize - borderWidth);
            SKRect avatarRect = new(padding + borderWidth, padding + borderWidth, avatarSize, avatarSize);
            SKRoundRect circleRect = new(avatarRect, circle);

            SKImage authorImage = SKImage.FromEncodedData(avatar);

            canvas.DrawRoundRect(backgroundRect, radius, radius, background);
            canvas.DrawRect(bannerRect1, bannerBackground);
            canvas.DrawRoundRect(bannerRect2, radius, radius, bannerBackground);
            canvas.DrawRoundRect(borderRect, circle, circle, background);
            if (!(member.Nickname is null)) canvas.DrawText(member.Nickname, padding, height - padding - secondaryText - spacing, primary);
            if (!(member.Nickname is null)) canvas.DrawText(tag, padding, height - padding, secondary);
            if (member.Nickname is null) canvas.DrawText(tag, padding, height - padding - secondaryText - spacing, primary);
            canvas.ClipRoundRect(circleRect);
            canvas.DrawImage(authorImage, avatarRect, background);

            SKImage image = SKImage.FromBitmap(bitmapNew);
            SKData encoded = image.Encode();
            Stream userData = encoded.AsStream();
            userData.Position = 0;
            DiscordMessageBuilder message = new();
            message.WithFile("userData.png", userData);
            await context.RespondAsync(message);
        }

        [Command("spotify")]
        [Description("Ищет и показывает информацию о Spotify определённого участника.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendSpotifyStatus(CommandContext context, DiscordMember member = null)
        {
            member ??= context.Member;
            DiscordActivity activity = member.Presence.Activities.ToList().Find((activity) => activity.Name == "Spotify");
            if (member.Presence.Activities.Count == 0 || activity == null)
            {
                await context.RespondAsync("Чёрт, а где спотифай?");
            }
            else
            {
                DiscordRichPresence Spotify = activity?.RichPresence;
                if (Spotify is null)
                {
                    await context.RespondAsync("ААААА SPOTIFY IS NULL. Я БЕЗ ПОНЯТИЯ ПОЧЕМУ");
                }
                else
                {
                    string Author = Spotify.State;
                    string Title = Spotify.Details;
                    string Album = Spotify.LargeImageText;
                    string AlbumImage = Spotify.LargeImage.Url.OriginalString;
                    TimeSpan Duration = (TimeSpan)(Spotify.EndTimestamp - Spotify.StartTimestamp);
                    TimeSpan Left = ((TimeSpan)(Spotify.EndTimestamp - Spotify.StartTimestamp)) - ((TimeSpan)(DateTime.Now - Spotify.StartTimestamp));

                    HttpClient client = new HttpClient();
                    HttpResponseMessage responseMessage = await client.GetAsync(AlbumImage);
                    HttpContent httpContent = responseMessage.Content;
                    Stream cover = await httpContent.ReadAsStreamAsync();
                    SKBitmap bitmapNew = new(1500, 900);
                    SKCanvas canvas = new(bitmapNew);
                    SKImage coverImage = SKImage.FromEncodedData(cover);
                    SKPaint background = new SKPaint
                    {
                        Color = new SKColor(25, 20, 20)
                    };
                    SKPaint paint = new SKPaint { };
                    SKPaint primary = new SKPaint
                    {
                        TextSize = 24 * 3,
                        Color = new SKColor(30, 215, 96),
                        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                    };
                    SKPaint percentableText = new SKPaint
                    {
                        TextSize = 14 * 3,
                        Color = new SKColor(255, 255, 255),
                        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                    };
                    SKPaint timeText = new SKPaint
                    {
                        TextSize = 12 * 3,
                        Color = new SKColor(255, 255, 255),
                        Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                    };
                    SKPaint secondary = new SKPaint
                    {
                        TextSize = 18 * 3,
                        Color = new SKColor(20, 130, 59)
                    };
                    string leftText = ((TimeSpan)(DateTime.Now - Spotify.StartTimestamp)).ToString("mm\\:ss");
                    string rightText = ((TimeSpan)(Spotify.EndTimestamp - Spotify.StartTimestamp)).ToString("mm\\:ss");
                    float leftTextWidth = timeText.MeasureText(leftText);
                    float rightTextWidth = timeText.MeasureText(rightText);
                    double percents = Math.Floor((TimeSpan)(DateTime.Now - Spotify.StartTimestamp) / ((TimeSpan)(Spotify.EndTimestamp - Spotify.StartTimestamp) / 100));
                    float part = bitmapNew.Height / 8;
                    SKRect first = new(0, part, bitmapNew.Width, bitmapNew.Height - part);
                    float width = (float)percents * ((bitmapNew.Width - 30 - leftTextWidth) / 100);
                    float primaryInteger = 75 * 3;
                    SKRect time = new(30 + leftTextWidth, bitmapNew.Height - part - 45, (width < (30 + leftTextWidth) ? 30 + leftTextWidth + width : width), bitmapNew.Height - part - 15);
                    SKRect fulltime = new(30 + leftTextWidth, bitmapNew.Height - part - 45, bitmapNew.Width - 30 - rightTextWidth, bitmapNew.Height - part - 15);
                    SKRect second = new(15, part + 15, primaryInteger + part, part * 2 + primaryInteger);
                    canvas.Clear();
                    canvas.DrawRoundRect(first, 10, 10, background);
                    canvas.DrawRoundRect(fulltime, 10, 10, primary);
                    canvas.DrawRoundRect(time, 10, 10, secondary);
                    canvas.DrawText(leftText, 15, bitmapNew.Height - part - 20, timeText);
                    canvas.DrawText(rightText, bitmapNew.Width - 15 - rightTextWidth, bitmapNew.Height - part - 20, timeText);
                    canvas.DrawText(Title, primaryInteger + part + 30, part + (24 * 3), primary);
                    canvas.DrawText(Author, primaryInteger + part + 30, part + (24 * 3) + 5 + (18 * 3), secondary);
                    canvas.DrawText(Album, primaryInteger + part + 30, (part * 2 + (90 * 3)) - (18 * 3), secondary);
                    string text = $"{percents}% / 100%";
                    float textWidth = percentableText.MeasureText(text);
                    canvas.DrawText(text, (bitmapNew.Width / 2) - (textWidth / 2), bitmapNew.Height - part - 15 - (14 * 3), percentableText);
                    SKRoundRect circleRect = new(second, 5);
                    canvas.ClipRoundRect(circleRect);
                    canvas.DrawImage(coverImage, second, paint);
                    SKImage image = SKImage.FromBitmap(bitmapNew);
                    SKData encoded = image.Encode();
                    Stream card = encoded.AsStream();
                    card.Position = 0;
                    string hours = Eva.Util.Utilities.GetDeclension(new string[] { "час", "часа", "часов" }, Left.Hours);
                    string minutes = Eva.Util.Utilities.GetDeclension(new string[] { "минута", "минуты", "минут" }, Left.Minutes);
                    string seconds = Eva.Util.Utilities.GetDeclension(new string[] { "секунда", "секунды", "секунд" }, Left.Seconds);

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                        .WithAuthor(Author)
                        .WithTitle(Title)
                        .WithDescription(@$"Альбом: **`{Album}`**
Длительность: **`{Duration:hh\:mm\:ss}`**
Осталось: **`{Left.Hours} {hours}, {Left.Minutes} {minutes} и {Left.Seconds} {seconds}`**.
Процент завершённости: **`{percents}% / 100%`**
					    ")
                        .WithThumbnail(AlbumImage)
                        .WithColor(DiscordColor.Black)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter(
                          $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                          context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                        )
                       .WithImageUrl("attachment://card.png");
                    DiscordMessageBuilder message = new DiscordMessageBuilder()
                        .WithFile("card.png", card)
                        .WithEmbed(embed.Build());

                    await context.RespondAsync(message);

                };
            };
        }

        [Command("presence")]
        [Description("Выводит список активностей участника.")]
        [Cooldown(1, 5.0, CooldownBucketType.Guild)]
        public async Task SendPresenceListAsync(CommandContext context, DiscordMember member = null)
        {
            member ??= context.Member;
            List<DiscordActivity> activities = member.Presence.Activities.ToList().Where((activity) => activity.ActivityType != ActivityType.Custom).ToList();
            if (activities.Count == 0)
            {
                await context.RespondAsync("А где активности?");
            }
            else
            {
                List<DiscordAsset> largeImages = activities.Where((activity) => activity.RichPresence != null && activity.RichPresence.LargeImage != null).Select((activity) => activity.RichPresence?.LargeImage).ToList();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithTitle($"Список активностей `{member.Username}`")
                    .WithColor(DiscordColor.Black)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(
                        $"Запрошено {context.Message.Author.Username}#{context.Message.Author.Discriminator}.",
                        context.Message.Author.GetAvatarUrl(ImageFormat.Png, 512)
                    );

                if (largeImages.Count != 0) embed.WithThumbnail(largeImages[0].Url.AbsoluteUri);

                foreach (DiscordActivity activity in activities)
                {
                    DiscordRichPresence presence = activity.RichPresence;
                    string FirstLine = null;
                    string SecondLine = null;
                    string Time = null;
                    if (presence != null)
                    {
                        FirstLine = presence.Details;
                        string Party = presence.MaximumPartySize == null ? "" : $" ({presence.CurrentPartySize} из {presence.MaximumPartySize})";
                        SecondLine = $"{presence.State}{Party}";
                        if (!(presence.StartTimestamp is null))
                        {
                            Time = $"{(TimeSpan)(DateTime.Now - presence.StartTimestamp):hh\\:mm\\:ss} прошло";
                        }
                        else if (!(presence.EndTimestamp is null))
                        {
                            Time = $"{(TimeSpan)(presence.EndTimestamp - DateTime.Now):hh\\:mm\\:ss} осталось";
                        };
                    }

                    string t = activity.ActivityType.ToString();
                    Regex reg = new Regex("[A-z][a-z]*", RegexOptions.Multiline);
                    string type = string.Join(' ', reg.Matches(t).Select((word) => word.Value));

                    embed.AddField($"{type} {activity.Name}", $"\u200c{FirstLine}\n{SecondLine}\n{Time}", true);
                };

                await context.RespondAsync(embed: embed.Build());
            }
        }
    }
}