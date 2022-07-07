using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.VoiceNext;
using Eva.Entities;
using Eva.Converters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Eva
{
    public class Bot
    {
        private readonly EventId Client = new(777, "Client");
        private readonly EventId MessageLogger = new(228, "Messages");
        private readonly Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
        private ErrorVariants errors = JsonConvert.DeserializeObject<ErrorVariants>(File.ReadAllText("Errors.json"));

        public async Task StartAsync()
        {
            DiscordClient _client = new(new DiscordConfiguration()
            {
                MinimumLogLevel = LogLevel.Information,
                Token = config.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });
            _client.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });
            _client.UseVoiceNext();
            CommandsNextExtension commands = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "d$:" } // dev prefix.
            });
            commands.CommandErrored += CommandErroredHandler;
            commands.SetHelpFormatter<HelpFormatter>();
            commands.RegisterCommands(Assembly.GetExecutingAssembly());
            _client.Ready += HandleReady;
            _client.MessageCreated += HandleMessage;
            _client.MessageCreated += CommandHandler;
            // _client.ComponentInteractionCreated += Interacted;
            // _client.ModalSubmitted += ModalSubmitted;
            await _client.ConnectAsync();
            await Task.Delay(-1);
        }

        /*private async Task ModalSubmitted(DiscordClient sender, ModalSubmitEventArgs args)
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You lying, because that's was you."));
        }

        private async Task Interacted(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            if (args.Interaction.Data.ComponentType == ComponentType.Button)
            {
                if (args.Interaction.Data.CustomId == "eval")
                {
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.Modal, new DiscordInteractionResponseBuilder()
                      .WithTitle("Evaluating C# code.")
                      .WithCustomId("eval")
                      .AddComponents(new TextInputComponent("Code:", "eval_code", "using System;\n\nnamespace App {\n\tclass Program {\n\t\tpublic void Main(string[] args) {\n\n\t\t}\n\t}\n}", null, true, TextInputStyle.Paragraph))
                    );
                    // Console.WriteLine(ObjectInspector.Inspect(args.Interaction.Data, 2, 1));
                };
            };
        }*/

        private Task CommandErroredHandler(CommandsNextExtension _, CommandErrorEventArgs e)
        {
            CommandContext context = e.Context;
            Command command = e.Command;
            Random rnd = new Random();
            string title = errors.Errors[rnd.Next(0, errors.Errors.Length)];
            string description = $"```cs\n{e.Exception.GetType()}: {e.Exception.Message}\n```";
            DiscordColor color = new DiscordColor("#ED4245");

            if (e.Exception is InvalidOperationException)
            {
                title = "�������, �� ������� ��������!";
                description = "������� ��������� ����, � ��������� ����� �� ����� ���� ������������ � �������� �������.";
            }

            if (e.Exception is ArgumentException || e.Exception is ArgumentNullException)
            {
                title = "��, ������� �� �������� �����-�� ��������!";
                description = $"����� ���� `$:help {command.QualifiedName}` �� ��������?";
            };

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = color,
                Title = title,
                Description = description
            };

            if (e.Exception is CommandNotFoundException) return Task.CompletedTask;

            context.Client.Logger.LogError(Client, ObjectInspector.Inspect(e, 2, 2));
            context.RespondAsync(embed: embed.Build());
            return Task.CompletedTask;
        }

        private Task HandleMessage(DiscordClient client, MessageCreateEventArgs e)
        {
            if (e.Message.Content.StartsWith(client.CurrentUser.Mention))
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithTitle($"������ {e.Author.Username}, � {client.CurrentUser.Username}!")
                    .WithDescription($"��� �������: `$:`.\n\n����� �������� ������ [���-]������ �/��� [���-]��������� ���������� ��������� **`$:help [...���������] [�������]`**.\n\n���� �� ����� �����-�� ���/������ ��� ������ ����� ���� ��������� ����� �������, ���������� �������� �� **`ar15@aiving.tk`**. ���� ������ �� ����� � ������� ��� ����, ������ � DM ������������: {client.CurrentApplication.Owners.FirstOrDefault().Mention}\n\n�.�: [ ] - �������� ��������������.")
                    .WithColor(DiscordColor.Black)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(
                        $"��������� {e.Author.Username}#{e.Author.Discriminator}.",
                        e.Author.GetAvatarUrl(ImageFormat.Png, 512)
                    );
                e.Message.RespondAsync(embed: embed.Build());
            };
            client.Logger.LogInformation(MessageLogger, $"{e.Guild?.Name}, {e.Channel.Name}, {e.Author.Username}#{e.Author.Discriminator}: {e.Message.Content}");
            return Task.CompletedTask;
        }

        private Task CommandHandler(DiscordClient client, MessageCreateEventArgs e)
        {
            CommandsNextExtension cnext = client.GetCommandsNext();
            DiscordMessage msg = e.Message;

            int cmdStart = msg.GetStringPrefixLength("$:");
            if (cmdStart == -1) return Task.CompletedTask;

            string prefix = msg.Content.Substring(0, cmdStart);
            string cmdString = msg.Content[cmdStart..];

            Command command = cnext.FindCommand(cmdString, out string args);
            if (command == null) return Task.CompletedTask;

            CommandContext ctx = cnext.CreateContext(msg, prefix, command, args);
            Task.Run(async () => await ctx.TriggerTypingAsync());

            return Task.CompletedTask;
        }

        private Task HandleReady(DiscordClient client, ReadyEventArgs e)
        {
            client.Logger.LogInformation(Client, "���-��� � ����!");
            return Task.CompletedTask;
        }
    }
}
