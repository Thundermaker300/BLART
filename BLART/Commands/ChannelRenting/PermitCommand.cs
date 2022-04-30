namespace BLART.Commands.ChannelRenting;

using System.Threading.Tasks;
using BLART.Modules;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Services;

using Group = Discord.Interactions.GroupAttribute;
using Summary = Discord.Interactions.SummaryAttribute;

public partial class RentCommands : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("allow", "Permit user(s) to join your rented channel.")]
    public async Task Permit([Summary("The user(s) to permit")] [Remainder] string users)
    {
        if (!ChannelRenting.IsRenting(Context.User))
        {
            await RespondAsync(embed: await ErrorHandlingService.GetErrorEmbed(ErrorCodes.PermissionDenied,
                "You can only use this command while renting a channel."));
            return;
        }

        List<IGuildUser> guildUsers = new();

        foreach (string s in users.Split(' '))
        {
            if (ulong.TryParse(s.Replace("<", string.Empty).Replace("@", string.Empty).Replace(">", string.Empty), out ulong userId))
            {
                IGuildUser user = Context.Guild.GetUser(userId);
                if (user is not null)
                    guildUsers.Add(user);
            }
        }

        IVoiceChannel channel = Bot.Instance.Guild.GetVoiceChannel(ChannelRenting.RentedChannels[Context.User]);
            
        foreach (IGuildUser user in guildUsers)
            await channel.AddPermissionOverwriteAsync(user, new OverwritePermissions(connect: PermValue.Allow));

        await RespondAsync("The users mentioned have been permitted to connect to your channel.");
    }
}