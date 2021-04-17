// **********************************************************************************
//
//           Title: jp-lite
//     Description: This is the class responsible for recieving and handling all
//                  command methods used by the jp-lite Discord bot
//          Author: Josh Pion (jP)
//   Date Modified: 4/16/2021
//
// **********************************************************************************

using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JP_Lite
{
    class Commands
    {
        public static Task CommandHandler(SocketMessage message)
        {
            //variables
            string command = "";
            int lengthOfCommand = -1;
            string guildStr = "";
            ulong guildID = 0;
            ChannelGuide guide = null;

            //load the guide for this server
            for (int indexedChar = message.GetJumpUrl().IndexOf("els/") + 4; indexedChar < message.GetJumpUrl().Length; indexedChar++)
            {
                if (message.GetJumpUrl()[indexedChar] != '/')
                {
                    guildStr += message.GetJumpUrl()[indexedChar];
                }
                else
                {
                    break;
                }
            }

            if (ulong.TryParse(guildStr, out guildID))
            {
                guide = new ChannelGuide(guildID);
            }
            //post link for move command
            //this looks for the bot to post the message in the receiving channel then uses the info of THAT message to post a second message in the
            //original channel using the values saved before
            if (message.Author.IsBot && message.Content.Contains("Please follow the link to the original thread"))
            {                        //post the notification message in the original channel
                originalChannel.SendMessageAsync($@"{originalAuthor.Mention} has indicated this conversation " +
                    $"would be better suited for the channel #{message.Channel.Name} " +
                    $"Please follow the link to continue the thread\n\n" +
                    $"https://discord.com/channels/" + $"{originalGuildID}/{message.Channel.Id}/{message.Id}", false, null, null, null, originalReference);
            }

            if (message.MentionedUsers.Count > 0)
            {
                //check if the bot was mentioned
                IEnumerator<SocketUser> usersMentioned = message.MentionedUsers.GetEnumerator();

                for (int index = 0; index < message.MentionedUsers.Count; index++)
                {
                    usersMentioned.MoveNext();
                    if (usersMentioned.Current.IsBot && usersMentioned.Current.Username == Program._client.CurrentUser.Username)
                    {
                        message.Channel.SendMessageAsync(File.ReadAllText("Data\\help.txt"));
                        message.DeleteAsync();
                    }
                }
            }

            //filtering messages begin here
            if (!message.Content.StartsWith('!')) //This is your prefix
                return Task.CompletedTask;

            if (message.Author.IsBot) //This ignores all commands from bots
                return Task.CompletedTask;

            //if (message.Content.Contains(' '))
            //    lengthOfCommand = message.Content.IndexOf(' ');
            //else

            lengthOfCommand = message.Content.Length;

            command = message.Content.Substring(1, lengthOfCommand - 1).ToLower();

            //Commands begin here
            if (command.Contains("move"))
            {
                //check if the user entered a channel
                if (message.MentionedChannels.Count > 0 && message.MentionedChannels.Count < 2 && message.Reference != null)
                {
                    IEnumerator<SocketGuildChannel> mentionedChannels = message.MentionedChannels.GetEnumerator();
                    mentionedChannels.MoveNext();

                    if (mentionedChannels.Current.Id != message.Channel.Id)
                    {
                        //post the message in the new channel
                        ISocketMessageChannel newChannel = (ISocketMessageChannel)mentionedChannels.Current;
                        newChannel.SendMessageAsync($@"{message.Author.Mention} has indicated that a conversation started in #{message.Channel.Name} " +
                            $"would be better suited for this channel " +
                            $"Please follow the link to the original thread. \n\n" +
                            $"https://discord.com/channels/"+$"{message.Reference.GuildId.Value}/{message.Reference.ChannelId}/{message.Reference.MessageId.Value}");

                        //hold on to the original channel
                        originalChannel = message.Channel;
                        originalGuildID = message.Reference.GuildId.Value;
                        originalReference = message.Reference;
                        originalAuthor = message.Author;

                        ////post the notification message in the original channel
                        //message.Channel.SendMessageAsync($@"{message.Author.Mention} has indicated this conversation " +
                        //    $"would be better suited for the channel #{mentionedChannels.Current.Name} " +
                        //    $"Please follow the link to continue the thread", false, null, null, null, message.Reference);
                    }
                    else
                    {
                        //post an error message for the user
                        message.Channel.SendMessageAsync($@"{message.Author.Mention} please select a channel, other than the current one to move this conversation");
                    }
                }
            }
            else if (command.Contains("addguide"))
            {
                //determines if the user is an admin who can edit the channel guide
                if (ChannelGuide.admins.Contains(message.Author.Id))
                {
                    int indexedChar = 8;
                    while (indexedChar < message.Content.Length)
                    {
                        if (message.Content[indexedChar] == ' ' || message.Content[indexedChar] == '\n')
                        {
                            break;
                        }

                        indexedChar++;
                    }

                    command = message.Content.Substring(indexedChar);

                    guide.AddGuide(message.Channel.Name, command);
                }
                else
                {
                    message.Channel.SendMessageAsync("Access Denied! You are not able to edit channel guides");
                }
            }
            else if (command.Contains("guide"))
            {
                //this command will call a method in the channel guide class that will then display a 
                //message with the intended usage of the channel and any rules associated with it.

                ChannelInfo info = guide.RetrieveGuide(message.Channel.Name);
                message.Channel.SendMessageAsync(info.info);
            }
            else if (command.Contains("getuserinfo"))
            {
                message.Author.SendMessageAsync($"Name: {message.Author.Username}\n" +
                                                $"ID: {message.Author.Id}");
            }
            else if (command.Contains("help"))
            {
                if (command.Contains("pm"))
                {
                    message.Author.SendMessageAsync(File.ReadAllText("Data\\help.txt"));
                }
                else
                {
                    message.Channel.SendMessageAsync(File.ReadAllText("Data\\help.txt"));
                }
            }

            message.DeleteAsync();
            return Task.CompletedTask;
        }

        private static ulong originalGuildID;
        private static ISocketMessageChannel originalChannel;
        private static MessageReference originalReference;
        private static SocketUser originalAuthor;
    }
}
