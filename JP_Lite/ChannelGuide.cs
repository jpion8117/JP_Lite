// **********************************************************************************
//
//           Title: jp-lite
//     Description: This class houses all features related to the jp-lite project's 
//                  channel guide. Including loading, saving, modifing and displaying
//                  channel guides by the bot
//          Author: Josh Pion (jP)
//   Date Modified: 4/16/2021
//
// **********************************************************************************


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JP_Lite
{
    /// <summary>
    /// Stores info about channels
    /// </summary>
    struct ChannelInfo
    {
        public ChannelInfo(string nameSet = "", string infoSet = "")
        {
            name = nameSet;
            info = infoSet;
        }

        public string name;
        public string info;
    }

    class ChannelGuide
    {
        public ChannelGuide(ulong guildIDSet)
        {
            //
            // Variables
            //
            string[] fileData = null;

            //set fields
            channels = new List<ChannelInfo>();
            admins = new List<ulong>();
            guildID = guildIDSet;

            //load the channel guide
            try
            {
                fileData = File.ReadAllLines(FILEPATH + guildID.ToString() + FILENAME);
            }
            catch (DirectoryNotFoundException)
            {
                //if the directory doesn't exist, it will be created
                Directory.CreateDirectory(FILEPATH);
            }
            catch (FileNotFoundException)
            {
                //if the directory doesn't exist, it will be created
                Directory.CreateDirectory(FILEPATH);

                //creates the file if it doesn't exist already
                if (!File.Exists(FILEPATH + guildID.ToString() + FILENAME))
                {
                    File.Create(FILEPATH + guildID.ToString() + FILENAME);
                }
            }

            //begin parsing and loading the channel info
            if(fileData != null)
            {
                //variables used to extract the channel info from file
                ChannelInfo info = new ChannelInfo("","");
                (int start, int end) channelInfoTags;

                for (int line = 0; line < fileData.Length; line++)
                {
                    if (info.name == "")
                    {
                        if (fileData[line].Contains('<') && fileData[line].Contains('>') && !fileData[line].Contains('/'))
                        {
                            for (int charNum = fileData[line].IndexOf('<') + 1; charNum < fileData[line].Length; charNum++)
                            {
                                if (fileData[line][charNum] != '>')
                                {
                                    info.name += fileData[line][charNum];
                                }
                                else
                                {
                                    line++;
                                    break;
                                }
                            }
                        }
                    }

                    channelInfoTags = FindChannelTag(fileData, info.name);

                    if (line != channelInfoTags.end)
                    {
                        info.info += fileData[line] + "\n";
                    }
                    else
                    {
                        //add the data to the List
                        channels.Add(info);
                        
                        //reset to get the next channel's info
                        info = new ChannelInfo("","");
                        channelInfoTags.end = 0;
                        channelInfoTags.start = 0;
                    }
                }
            }

            //load admin Ids
            try
            {
                fileData = File.ReadAllLines(ADMIN_FILEPATH);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("Failed to load admin file...");
            }

            foreach (string id in fileData)
            {
                ulong temp = 0;
                if (ulong.TryParse(id, out temp))
                {
                    admins.Add(temp);
                }
            }
        }
        ~ChannelGuide()
        {
            SaveGuide();
        }

        /// <summary>
        /// retrieves the start and end lines of a channel info tag
        /// </summary>
        /// <param name="data">data to be searched</param>
        /// <param name="tag">name of the channel being searched</param>
        /// <returns></returns>
        (int, int) FindChannelTag(string[] data, string tag)
        {
            (int start, int end) returns;
            returns.start = -1;
            returns.end = -1;

            for (int line = 0; line < data.Length; line++)
            {
                if (data[line].Contains($"<{tag}>"))
                {
                    returns.start = line;
                }
                else if (data[line].Contains($"</{tag}>"))
                {
                    returns.end = line;
                    break;
                }
            }

            return returns;
        }
        private void SaveGuide()
        {
            //
            // Variables
            // 
            List<string> fileOut = new List<string>();

            //
            // Save the channel guide
            //

            //Format the file output
            foreach (ChannelInfo channel in channels)
            {
                fileOut.Add($"<{channel.name}>");
                fileOut.Add(channel.info);
                fileOut.Add($"</{channel.name}>");
            }

            //save the file
            File.WriteAllLines(FILEPATH + guildID.ToString() + FILENAME, fileOut);
        }
        public void AddGuide(string name, string info)
        {
            //check if there is already a guide
            for (int index = 0; index < channels.Count; index++)
            {
                if (channels[index].name == name)
                {
                    channels[index] = new ChannelInfo(name, info);
                    SaveGuide();
                    return;
                }
            }

            //if channel doesn't have a guide, create one for it
            channels.Add(new ChannelInfo(name, info));
            SaveGuide();
        }
        public ChannelInfo RetrieveGuide(string channelName)
        {
            ChannelInfo channel = new ChannelInfo(channelName, "Sorry, but the current channel does not have a guide associated with it...");

            foreach (ChannelInfo guide in channels)
            {
                if (guide.name == channelName)
                {
                    channel.info = guide.info;
                }
            }

            return channel;
        }

        public static List<ulong> admins { get; private set; }
        List<ChannelInfo> channels;
        ulong guildID;
        const string ADMIN_FILEPATH = "Data\\approvedAdmins.txt";
        const string FILENAME = "_ChannelGuide.chg";
        const string FILEPATH = "info\\";
    }
}
