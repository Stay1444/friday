﻿using System.Data.Common;
using System.Globalization;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Friday.Common.Entities;
using Friday.Common.Models;
using Friday.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Friday.Common;

public static class Extensions
{
    private static Random _random = new Random();
    public static Task<UserConfiguration> GetUserConfiguration(this CommandContext ctx)
    {
        var userConfigurationProvider = ctx.Services.GetService<UserConfigurationProvider>();
        if (userConfigurationProvider is null) throw new InvalidOperationException("UserConfigurationProvider is null");
        return userConfigurationProvider.GetConfiguration(ctx.User.Id);
    }
    
    public static Task<GuildConfiguration> GetGuildConfiguration(this CommandContext ctx)
    {
        var guildConfigurationProvider = ctx.Services.GetService<GuildConfigurationProvider>();
        if (guildConfigurationProvider is null) throw new InvalidOperationException("GuildConfigurationProvider is null");
        return guildConfigurationProvider.GetConfiguration(ctx.Guild.Id);
    }
    
    public static async Task<DiscordGuild?> GetGuildAsync(this DiscordShardedClient client, ulong id)
    {
        foreach (var dClient in client.ShardClients.Values)
        {
            try
            {
                var g = await dClient.GetGuildAsync(id);
                if (g != null) return g;
                
            }catch(Exception)
            {
                Log.Error("Failed to get guild {0}", id);
            }
        }
        
        return null;
    }

    public static bool IsCallerOwner(this CommandContext ctx)
    {
        return ctx.Guild.OwnerId == ctx.User.Id;
    }

    public static DiscordInteractionResponseBuilder ToInteractionResponseBuilder(this DiscordMessageBuilder builder)
    {
        var responseBuilder = new DiscordInteractionResponseBuilder();
        if (builder.Embed is not null)
        {
            //responseBuilder.AddEmbed(builder.Embed);
        }

        foreach (var builderEmbed in builder.Embeds)
        {
            responseBuilder.AddEmbed(builderEmbed);
        }

        foreach (var builderComponent in builder.Components)
        {
            var components = new List<DiscordComponent>();
            foreach (var component in builderComponent.Components)
            {
                components.Add(component);
            }

            responseBuilder.AddComponents(components);
        }
        
        return responseBuilder;
    }

    public static List<T> ToList<T>(this IEnumerator<T> enumerator)
    {
        var list = new List<T>();
        while (enumerator.MoveNext())
        {
            list.Add(enumerator.Current);
        }

        return list;
    }

    public static DiscordClient GetClient(this DiscordShardedClient client, DiscordGuild guild)
    {
        var c =  client.ShardClients.FirstOrDefault(x => x.Value.Guilds.ContainsKey(guild.Id)).Value;
        return c;
    }

    public static DiscordClient GetClient(this DiscordShardedClient client, ulong guildId)
    {
        var c =  client.ShardClients.FirstOrDefault(x => x.Value.Guilds.ContainsKey(guildId)).Value;
        return c;
    }
    
    public static string GetName(this DiscordMember member)
    {
        return member.Nickname ?? member.Username + "#" + member.Discriminator;
    }

    public static string GetName(this DiscordUser user)
    {
        return user.Username + "#" + user.Discriminator;
    }

    public static async Task<DiscordMember> GetCurrentMemberAsync(this DiscordClient client, DiscordGuild guild)
    {
        return await guild.GetMemberAsync(client.CurrentUser.Id);
    }
    
    public static async Task<string> GetString(this CommandContext ctx, string key, params object[] format)
    {
        var languageProvider = ctx.Services.GetService<LanguageProvider>();
        if (languageProvider is null) throw new InvalidOperationException("LanguageProvider is null");
        
        var userConfigTask = ctx.GetUserConfiguration();
        if (ctx.Member is null) // check if we are in a guild
        {
            //We are not in a guild, so we can't get the guild config
            var userConfig = await userConfigTask;

            var language = "en";
            if (userConfig.LanguageOverride is not null)
            {
                language = userConfig.LanguageOverride;
            }
            
            return languageProvider.GetString(language, key, format);
        }
        else
        {
            // We are in a guild, so we can get the guild config
            var guildConfig = await ctx.GetGuildConfiguration();
            var userConfig = await userConfigTask;

            var language = guildConfig.Language;
            if (userConfig.LanguageOverride is not null)
            {
                language = userConfig.LanguageOverride;
            }
            
            return languageProvider.GetString(language, key, format);
        }
    }

    public static Task Ack(this InteractivityResult<ComponentInteractionCreateEventArgs> ievent)
    {
        return ievent.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }
    
    public static Task Ack(this ComponentInteractionCreateEventArgs ievent)
    {
        return ievent.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }
    
    public static Task Ack(this ModalSubmitEventArgs ievent)
    {
        return ievent.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }
    
    public static Task Ack(this InteractivityResult<ModalSubmitEventArgs> ievent)
    {
        return ievent.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }

    public static Task<DiscordMember> GetCurrentMember(this CommandContext ctx)
    {
        return ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
    }

    public static DiscordEmbedBuilder Transparent(this DiscordEmbedBuilder builder)
    {
        return builder.WithColor(new DiscordColor("#2F3136"));
    }
    
    public static HumanTimeSpan ToHumanTimeSpan(this TimeSpan span)
    {
        return new HumanTimeSpan(span);
    }

    public static IEnumerable<DiscordGuild> GetGuilds(this DiscordShardedClient client)
    {
        var guilds = new List<DiscordGuild>();
        foreach (var shardClient in client.ShardClients)
        {
            guilds.AddRange(shardClient.Value.Guilds.Values);
        }
        
        return guilds;
    }

    public static string RandomAlphanumeric(this char[] array)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = chars[_random.Next(chars.Length)];
        }
        return new string(array) ?? throw new InvalidOperationException();
    }
    
    public static string MaxLength(this string str, int maxLength, bool addDots = false)
    {
        if (str.Length > maxLength)
        {
            if (addDots) {
                return str.Substring(0, maxLength - 3) + "...";
            } else {
                return str.Substring(0, maxLength);
            }
        }
        return str;
    }

    public static async Task<Stream> DownloadAsync(this DiscordEmoji emoji)
    {
        var client = new HttpClient();
        
        var response = await client.GetAsync(emoji.Url);
        
        return await response.Content.ReadAsStreamAsync();
    }
    
    public static async Task<Stream> DownloadAsync(this DiscordAttachment attachment)
    {
        var client = new HttpClient();
        
        var response = await client.GetAsync(attachment.Url);
        
        return await response.Content.ReadAsStreamAsync();
    }

    public static string ToHex(this DiscordColor color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public static int GetUserCount(this DiscordShardedClient client)
    {
        var count = 0;
        
        foreach (var shardClient in client.ShardClients)
        {
            foreach (var guild in shardClient.Value.Guilds.Values)
            {
                count += guild.MemberCount;
            }
        }
        
        return count;
    }

    public static int GetGuildCount(this DiscordShardedClient client)
    {
        return client.ShardClients.Sum(shardClient => shardClient.Value.Guilds.Values.Count());
    }

    public static string RemoveDiacritics(this string v)
    {
        String normalizedString = v.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            switch (CharUnicodeInfo.GetUnicodeCategory(c))
            {
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.DecimalDigitNumber:
                    stringBuilder.Append(c);
                    break;
                case UnicodeCategory.SpaceSeparator:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.DashPunctuation:
                    stringBuilder.Append('_');
                    break;
            }
        }
        string result = stringBuilder.ToString();
        return String.Join("_", result.Split(new char[] { '_' }
            , StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
    {
        return !enumerable.Any();
    }

    public static string? ReadNullableString(this DbDataReader r, int i)
    {
        if (r.IsDBNull(i))
        {
            return null;
        }
        
        return r.GetString(i);
    }
}