using AngleSharp.Html.Parser;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Eva.Commands.Entities;
using Eva.Entities;
using GenshinSharp;
using Legato;
using Legato.Interop.AimpRemote.Enum;
using ChannelType = Legato.Interop.AimpRemote.Enum.ChannelType;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using NHentaiSharp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Eva.Commands
{
    [Group("owner")]
    [Description("Группа команд, предназначенная для использования исключительно владельцем бота.")]
    [Hidden]
    public partial class OwnerCommands : BaseCommandModule
    {
        private readonly int StartIndex = 2;
        private static readonly Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));

        [Command("send")]
        [Description("Отправляет текст, удаляя исходное сообщение с командой.")]
        public async Task SendAsync(CommandContext context, [RemainingText] string data)
        {
            if (!config.Owners.Contains(context.Message.Author.Id)) return;
            await context.Message.DeleteAsync();
            await context.Channel.SendMessageAsync(data);
        }

        [Group("scripts")]
        public class ScriptsManagementCommands : BaseCommandModule
        {
            public class ScriptsManagement
            {
                [JsonProperty("startup")] public List<Script> StartUp;
                [JsonProperty("functions")] public List<Script> Functions;
            }

            public class Script
            {
                [JsonProperty("name")] public string Name;
                [JsonProperty("code")] public string Code;
            }

            private ScriptsManagement scripts = JsonConvert.DeserializeObject<ScriptsManagement>(File.ReadAllText("scripts.json"));
            private string selectedType = "startup";
            private string selectedScript = null;
            private bool alreadyAdded = false;

            private DiscordMessageBuilder Generate()
            {
                DiscordButtonComponent createScript = new DiscordButtonComponent(ButtonStyle.Success, "script_create", "Создать скрипт");
                DiscordButtonComponent editScript = new DiscordButtonComponent(ButtonStyle.Primary, "script_edit", "Отредактировать скрипт", this.selectedScript == null);
                DiscordButtonComponent deleteScript = new DiscordButtonComponent(ButtonStyle.Danger, "script_delete", "Удалить скрипт", this.selectedScript == null);
                DiscordSelectComponent scriptType = new DiscordSelectComponent("script_type", "Выберите одну из категорий.", new List<DiscordSelectComponentOption>
                {
                    new DiscordSelectComponentOption("При запуске", "startup", "Скрипты в этой категории вызываются при запуске/перезапуске бота.", true),
                    new DiscordSelectComponentOption("Функции", "functions", "Скрипты в этой категории вызываются лишь пользователем.")
                });
                IEnumerable<Script> scriptList = this.selectedType == "functions" ? this.scripts.Functions : this.scripts.StartUp;
                DiscordSelectComponent scripts = new DiscordSelectComponent("script_list", "Выберите существующий скрипт.", scriptList.Count() < 1 ? new List<DiscordSelectComponentOption> {
                    new DiscordSelectComponentOption("Nothing.", "nothing", null, true)
                } : scriptList.Select((script) => new DiscordSelectComponentOption(script.Name, script.Name)), scriptList.Count() < 1 ? true : false);

                DiscordActionRowComponent firstRow = new DiscordActionRowComponent(new List<DiscordSelectComponent>
                {
                    scriptType
                });
                DiscordActionRowComponent secondRow = new DiscordActionRowComponent(new List<DiscordSelectComponent>
                {
                    scripts
                });
                DiscordActionRowComponent thirdRow = new DiscordActionRowComponent(new List<DiscordButtonComponent>
                {
                    createScript, editScript, deleteScript
                });

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithTitle(this.selectedScript != null ? $"Менеджер скриптов: {this.selectedScript}." : "Менеджер скриптов.")
                    .WithDescription(this.selectedScript != null ? Formatter.BlockCode(scriptList.ToList().Find((script) => script.Name == this.selectedScript)?.Code, "csharp"): "Здесь нет ничего интересного. Выберите один из существующих скриптов, либо создайте новый.")
                    .WithTimestamp(DateTime.Now);

                return new DiscordMessageBuilder()
                    .WithEmbed(embed.Build())
                    .AddComponents(new List<DiscordActionRowComponent>
                    {
                        firstRow, secondRow, thirdRow
                    });

            }

            [GroupCommand()]
            public async Task SendScriptsMessage(CommandContext context)
            {
                if (!this.alreadyAdded)
                {
                    context.Client.ComponentInteractionCreated += Interacted;
                    context.Client.ModalSubmitted += ModalSubmitted;
                    this.alreadyAdded = true;
                };
                await context.RespondAsync(Generate());
            }

            private async Task ModalSubmitted(DiscordClient sender, ModalSubmitEventArgs args)
            {
                string? name = args.Values.GetValueOrDefault("script_name");
                string? code = args.Values.GetValueOrDefault("script_code");

                if (this.selectedType == "functions") this.scripts.Functions.Add(new Script
                {
                    Name = name,
                    Code = code
                });
                else this.scripts.StartUp.Add(new Script
                {
                    Name = name,
                    Code = code
                });

                await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(Generate()));
            }

            public async Task Interacted(DiscordClient sender, ComponentInteractionCreateEventArgs args)
            {
                if (args.Interaction.Data.ComponentType == ComponentType.Select)
                {
                    if (args.Interaction.Data.CustomId == "script_type")
                    {
                        this.selectedType = args.Interaction.Data.Values[0];
                        this.selectedScript = null;
                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(Generate()));
                    };

                    if (args.Interaction.Data.CustomId == "script_list")
                    {
                        this.selectedScript = args.Interaction.Data.Values[0];
                        await args.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(Generate()));
                    };
                };

                if (args.Interaction.Data.ComponentType == ComponentType.Button)
                {
                    if (args.Interaction.Data.CustomId == "script_create")
                    {
                        await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, new DiscordInteractionResponseBuilder()
                            .WithTitle("New script")
                            .WithCustomId("script_new")
                            .AddComponents(new List<DiscordActionRowComponent> {
                                new DiscordActionRowComponent (new List<TextInputComponent> {
                                    new TextInputComponent("Name:", "script_name", "Hello World!", null, true, TextInputStyle.Short)
                                }),
                                new DiscordActionRowComponent(new List<TextInputComponent> {
                                    new TextInputComponent("Code:", "script_code", "using System;\n\nnamespace App {\n\tclass Program {\n\t\tpublic void Main(string[] args) {\n\n\t\t}\n\t}\n}", null, true, TextInputStyle.Paragraph)
                                })
                            }));
                    };

                    if (args.Interaction.Data.CustomId == "script_edit")
                    {

                    };

                    if (args.Interaction.Data.CustomId == "script_delete")
                    {

                    };
                };

            }
        }

        [Command("test")]
        public async Task TestAnyFunctionAsync(CommandContext context)
        {
            if (!config.Owners.Contains(context.Message.Author.Id)) return;
            DiscordMessageBuilder message = new DiscordMessageBuilder();
            DiscordButtonComponent button = new DiscordButtonComponent(ButtonStyle.Primary, "eval", "");
            message.AddComponents(button);
            message.WithContent("testing components");
            await message.SendAsync(context.Channel);
        }

        [Command("aimp")]
        [Description("Получает информацию о процессе AIMP.")]
        public async Task SendAimpTrackInformationAsync(CommandContext context, string data = null)
        {
            if (!config.Owners.Contains(context.Message.Author.Id)) return;
            AimpProperties properties = new();
            AimpCommands commands = new();
            Regex r = new(@"^\d*\.?\d+$", RegexOptions.Multiline);

            if (!properties.IsRunning)
            {
                await context.RespondAsync("ПРОЦЕСС АИМПА НЕ НАЙДЕН, БЛЯЯЯЯЯЯЯЯЯЯЯТЬ.");
            }
            else
            {
                if (!(data == null) && data.StartsWith("--") && data.Contains("=") && data.Split("=")[1] != "")
                {
                    string mode = data.Split("=")[0][StartIndex..];
                    string value = data.Split("=")[1];
                    string setMode = data.Split(";")[0] ?? null;
                    string setValue = data.Split(";")[1] ?? null;
                    if (mode == "switch")
                    {
                        if (value == "repeat") properties.IsRepeat = !properties.IsRepeat;
                        else if (value == "mute") properties.IsMute = !properties.IsMute;
                        else if (value == "shuffle") properties.IsShuffle = !properties.IsShuffle;
                        else if (value == "play") commands.PlayPause();
                    }
                    else if (mode == "set" && setValue != "")
                    {
                        if (setMode == "play")
                        {
                            if (setValue == "true") commands.Play();
                            else if (setValue == "false") commands.Pause();
                        }
                        else if (setMode == "pause")
                        {
                            if (setValue == "true") commands.Pause();
                            else if (setValue == "false") commands.Play();
                        }
                        else if (setMode == "stop")
                        {
                            if (setValue == "true") commands.Stop();
                            else if (setValue == "false") commands.Play();
                        }

                    }
                    else if (mode == "set-volume" && r.IsMatch(value) && Convert.ToInt32(value) >= 0 && Convert.ToInt32(value) <= 100)
                    {
                        properties.Volume = Convert.ToInt32(value.Split(";")[1]);
                    }
                }
                // Player.
                PlayerState currentState = properties.State;
                int currentVolumePercent = properties.Volume;
                string muteEnabled = properties.IsMute ? "`Включён`" : "`Выключен`";
                string repeatEnabled = properties.IsRepeat ? "`Включён`" : "`Выключен`";
                string shuffleEnabled = properties.IsShuffle ? "`Включёно`" : "`Выключено`";
                string left = TimeSpan.FromMilliseconds(properties.CurrentTrack.Duration - properties.Position).ToString("hh\\:mm\\:ss");

                // Current track.
                ChannelType channelType = properties.CurrentTrack.ChannelType;
                uint bitrate = properties.CurrentTrack.BitRate;
                string fileSize = (properties.CurrentTrack.FileSize / (double)1024 / 1024).ToString("N2");
                string samplerate = (properties.CurrentTrack.SampleRate / (double)1000).ToString("N1");
                uint queuePosition = properties.CurrentTrack.TrackNumber + 1;
                string duration = TimeSpan.FromMilliseconds(properties.CurrentTrack.Duration).ToString("hh\\:mm\\:ss");
                string title = properties.CurrentTrack.Title != "" ? $"Название: `{properties.CurrentTrack.Title}`\n" : "";
                string artist = properties.CurrentTrack.Artist != "" ? $"Артист: `{properties.CurrentTrack.Artist}`\n" : "";
                string year = properties.CurrentTrack.Year != "" ? $"Год выпуска: `{properties.CurrentTrack.Year}`\n" : "";
                string album = properties.CurrentTrack.Album != "" ? $"Альбом: `{properties.CurrentTrack.Album}`\n" : "";
                string genre = properties.CurrentTrack.Genre != "" ? $"Жанр[ы]: `{properties.CurrentTrack.Genre}`\n" : "";

                // Card image
                SKBitmap bitmapNew = new(1500, 900);
                TagLib.IPicture coverPicture = TagLib.File.Create(properties.CurrentTrack.FilePath).Tag.Pictures.FirstOrDefault();
                MemoryStream cover = new(coverPicture.Data.Data);
                cover.Position = 0;
                SKCanvas canvas = new(bitmapNew);
                SKImage coverImage = SKImage.FromEncodedData(cover);
                SKPaint background = new SKPaint
                {
                    Color = new SKColor(0, 0, 0, 120)
                };
                SKPaint paint = new SKPaint { };
                SKPaint primary = new SKPaint
                {
                    TextSize = 24 * 3,
                    Color = new SKColor(255, 255, 255),
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                };
                SKPaint percentableText = new SKPaint
                {
                    TextSize = 14 * 3,
                    Color = new SKColor(255, 255, 255, 190),
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                };
                SKPaint timeText = new SKPaint
                {
                    TextSize = 12 * 3,
                    Color = new SKColor(255, 255, 255, 190),
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                };
                SKPaint secondary = new SKPaint
                {
                    TextSize = 18 * 3,
                    Color = new SKColor(120, 120, 120)
                };
                string leftText = TimeSpan.FromMilliseconds(properties.Position).ToString("mm\\:ss");
                string rightText = TimeSpan.FromMilliseconds(properties.CurrentTrack.Duration).ToString("mm\\:ss");
                float leftTextWidth = timeText.MeasureText(leftText);
                float rightTextWidth = timeText.MeasureText(rightText);
                double percents = Math.Floor(TimeSpan.FromMilliseconds(properties.Position) / (TimeSpan.FromMilliseconds(properties.CurrentTrack.Duration) / 100));
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
                canvas.DrawText(properties.CurrentTrack.Title, primaryInteger + part + 30, part + (24 * 3), primary);
                canvas.DrawText(properties.CurrentTrack.Artist, primaryInteger + part + 30, part + (24 * 3) + 5 + (18 * 3), secondary);
                canvas.DrawText(properties.CurrentTrack.Album, primaryInteger + part + 30, (part * 2 + (90 * 3)) - (18 * 3), secondary);
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

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .AddField("Плеер", $@"Состояние: `{currentState}`
Громкость: `{currentVolumePercent}%`
Звук: {muteEnabled}
Повтор: {repeatEnabled}
Перемешивание: {shuffleEnabled}
Прошло: `{left}`", true)

                    .WithColor(DiscordColor.Orange);

                DiscordMessageBuilder message = new();
                if (properties.CurrentTrack.FilePath != "")
                {
                    TagLib.IPicture firstPicture = TagLib.File.Create(properties.CurrentTrack.FilePath).Tag.Pictures.FirstOrDefault();
                    embed.AddField("Трек", $@"{title}{artist}{year}{album}{genre}{(properties.CurrentTrack.Album != "" ? $"Позиция в альбоме: `{queuePosition}`\n" : "")}Битрейт: `{bitrate}kbps`
Тип канала: `{channelType}`
Сэмплрейт: `{samplerate}kHz`
Длительность: `{duration}`
Размера файла: `{fileSize}MB`", true);
                    if (firstPicture != null)
                    {
                        MemoryStream stream = new(firstPicture.Data.Data);
                        stream.Position = 0;
                        string filename = $"albumart.{firstPicture.MimeType.Split("/").Last()}";
                        embed.WithImageUrl($"attachment://{filename}");
                        Dictionary<string, Stream> files = new Dictionary<string, Stream> {
                { filename, card }
            };
                        message.WithFiles(files);
                    };
                };

                message.WithEmbed(embed.Build());
                await context.RespondAsync(message);
            };
        }

        [Command("eval")]
        [Description("Выполняет указанный код.")]
        public async Task EvalAsync(CommandContext context, [RemainingText] string data)
        {
            if (!config.Owners.Contains(context.Message.Author.Id)) return;
            await context.TriggerTypingAsync();
            try
            {
                ScriptOptions options = ScriptOptions.Default
                    .AddReferences(typeof(Runtime).Assembly, typeof(HtmlParser).Assembly, typeof(System.Text.Json.JsonSerializer).Assembly)
                    .WithImports(
                        "System",
                        "System.IO",
                        "System.Linq",
                        "System.Net",
                        "System.Net.Http",
                        "System.Threading.Tasks",
                        "System.Collections.Generic",
                        "System.Text.RegularExpressions"
                    )
                    .WithImports("DSharpPlus", "DSharpPlus.Entities")
                    .WithImports("SkiaSharp")
                    .WithImports("Newtonsoft.Json", "Newtonsoft.Json.Linq")
                    .WithImports("MongoDB.Bson", "MongoDB.Driver");
                var result = await CSharpScript.EvaluateAsync(data, options, globals: new Globals
                {
                    context = context,
                    genhsinClient = new GenshinClient(),
                    nClient = new NClient(),
                    aimpProperties = new AimpProperties(),
                    aimpCommands = new AimpCommands(),
                    mongo = new MongoClient("mongodb://localhost:22022")
                });
                string inspected = ObjectInspector.Inspect(result, StartIndex, 1); ;
                if (inspected.Length > 1900)
                {
                    InteractivityExtension interactivity = context.Client.GetInteractivity();
                    IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(inspected, SplitType.Line);
                    pages = pages.Select((page) => new Page(Formatter.BlockCode(page.Embed.Description, "csharp")));
                    await context.Channel.SendPaginatedMessageAsync(context.Member, pages);
                }
                else
                {
                    await context.RespondAsync(Formatter.BlockCode(inspected, "csharp"));
                }
            }
            catch (Exception e)
            {
                await context.RespondAsync(Formatter.BlockCode(e.ToString(), "csharp"));
            }
        }
    }
}
