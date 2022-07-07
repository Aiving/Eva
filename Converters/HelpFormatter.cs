using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace Eva.Converters
{
    public class HelpFormatter : BaseHelpFormatter
    {
#nullable enable
        public DiscordEmbedBuilder EmbedBuilder { get; }
        private Command? Command { get; set; }

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {
            this.EmbedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Помощь")
                .WithColor(0x007FFF);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            Console.WriteLine(command);
            this.Command = command;

            this.EmbedBuilder.WithDescription($"{Formatter.InlineCode(command.Name)}: {command.Description ?? "Описание не предоставлено."}");

            if (command is CommandGroup cgroup && cgroup.IsExecutableWithoutSubcommands)
                this.EmbedBuilder.WithDescription($"{this.EmbedBuilder.Description}\n\nДанная категория может быть также использована как отдельная команда.");

            if (command.Aliases.Count > 0)
                this.EmbedBuilder.AddField("Синонимы", string.Join(", ", command.Aliases.Select(Formatter.InlineCode)), false);

            if (command.Overloads.Count > 0)
            {
                var sb = new StringBuilder();

                foreach (var ovl in command.Overloads.OrderByDescending(x => x.Priority))
                {
                    sb.Append('`').Append(command.QualifiedName);

                    foreach (var arg in ovl.Arguments)
                        sb.Append(arg.IsOptional || arg.IsCatchAll ? " [" : " <").Append(arg.Name).Append(arg.IsCatchAll ? "..." : "").Append(arg.IsOptional || arg.IsCatchAll ? ']' : '>');

                    sb.Append("`\n");

                    foreach (var arg in ovl.Arguments)
                        sb.Append('`').Append(arg.Name).Append(" (").Append(this.CommandsNext.GetUserFriendlyTypeName(arg.Type)).Append(")`: ").Append(arg.Description ?? "Описание не предоставлено.").Append('\n');

                    sb.Append('\n');
                }

                this.EmbedBuilder.AddField("Аргументы", sb.ToString().Trim(), false);
            }

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            IEnumerable<Command> commands = subcommands.Where((command) => command.GetType().GetProperty("Children") == null);
            IEnumerable<Command> categories = subcommands.Where((command) => command.GetType().GetProperty("Children") != null);

            if (commands.Count() > 0) this.EmbedBuilder.AddField((this.Command is null ? "Команды" : "Подкоманды"), string.Join(", ", commands.Select(x => Formatter.InlineCode(x.Name))), false);
            if (categories.Count() > 0) this.EmbedBuilder.AddField((this.Command is null ? "Категории" : "Подкатегории"), string.Join(", ", categories.Select(x => Formatter.InlineCode(x.Name))), false);

            return this;
        }

        public override CommandHelpMessage Build()
        {
            if (this.Command is null)
                this.EmbedBuilder.WithDescription("Список всех команд и категорий верхнего уровня. Укажите команду или категорию, чтобы увидеть больше информации.");

            return new CommandHelpMessage(embed: this.EmbedBuilder.Build());
        }
    }
}