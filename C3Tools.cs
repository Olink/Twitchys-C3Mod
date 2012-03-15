using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using TShockAPI;
using Terraria;
using C3Mod.GameTypes;

namespace C3Mod
{
    public class C3Tools
    {
        internal static string C3ConfigPath { get { return Path.Combine(TShock.SavePath, "c3modconfig.json"); } }

        internal static void SetupConfig()
        {
            try
            {
                if (File.Exists(C3ConfigPath))
                {
                    C3Mod.C3Config = C3ConfigFile.Read(C3ConfigPath);
                    // Add all the missing config properties in the json file
                }
                C3Mod.C3Config.Write(C3ConfigPath);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error in config file");
                Console.ForegroundColor = ConsoleColor.Gray;
                Log.Error("Config Exception");
                Log.Error(ex.ToString());
            }
        }
        //Converted v2.2
        internal static string AssignTeam(C3Player who, string gametype)
        {
            switch (gametype)
            {
                //Converted v2.2
                #region CTF
                case "ctf":
                    {
                        if (who.Team != 1 || who.Team != 2)
                        {
                            int playerteam1 = 0;
                            int playerteam2 = 0;

                            foreach (C3Player player in C3Mod.C3Players)
                            {
                                if (player.Team == 1)
                                    playerteam1++;
                                else if (player.Team == 2)
                                    playerteam2++;
                            }

                            if (playerteam1 > playerteam2)
                            {
                                who.GameType = "ctf";
                                who.Team = 2;
                                switch(C3Mod.C3Config.TeamColor2)
                                {
                                    case 1:
                                        {
                                            return "Blue";
                                        }
                                    case 2:
                                        {
                                            return "Green";
                                        }
                                    case 3:
                                        {
                                            return "Blue";
                                        }
                                    case 4:
                                        {
                                            return "Yellow";
                                        }
                                }
                            }
                            else if (playerteam2 > playerteam1)
                            {
                                who.Team = 1;
                                who.GameType = "ctf";

                                switch (C3Mod.C3Config.TeamColor2)
                                {
                                    case 1:
                                        {
                                            return "Blue";
                                        }
                                    case 2:
                                        {
                                            return "Green";
                                        }
                                    case 3:
                                        {
                                            return "Blue";
                                        }
                                    case 4:
                                        {
                                            return "Yellow";
                                        }
                                }
                            }

                            else
                            {
                                Random r = new Random();

                                switch (r.Next(2) + 1)
                                {
                                    case 1:
                                        {
                                            who.Team = 1;
                                            who.GameType = "ctf";
                                            switch (C3Mod.C3Config.TeamColor1)
                                            {
                                                case 1:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 2:
                                                    {
                                                        return "Green";
                                                    }
                                                case 3:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 4:
                                                    {
                                                        return "Yellow";
                                                    }
                                            }
                                            break;
                                        }
                                    case 2:
                                        {
                                            who.Team = 2;
                                            who.GameType = "ctf";
                                            switch (C3Mod.C3Config.TeamColor2)
                                            {
                                                case 1:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 2:
                                                    {
                                                        return "Green";
                                                    }
                                                case 3:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 4:
                                                    {
                                                        return "Yellow";
                                                    }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    }
#endregion
                //Converted v2.2
                #region OneFlag

                case "oneflag":
                    {
                        if (who.Team != 5 || who.Team != 6)
                        {
                            int redteamplayers = 0;
                            int blueteamplayers = 0;

                            foreach (C3Player player in C3Mod.C3Players)
                            {
                                if (player.Team == 5)
                                    redteamplayers++;
                                else if (player.Team == 6)
                                    blueteamplayers++;
                            }

                            if (redteamplayers > blueteamplayers)
                            {
                                who.Team = 6;
                                who.GameType = "oneflag";
                                switch (C3Mod.C3Config.TeamColor2)
                                {
                                    case 1:
                                        {
                                            return "Blue";
                                        }
                                    case 2:
                                        {
                                            return "Green";
                                        }
                                    case 3:
                                        {
                                            return "Blue";
                                        }
                                    case 4:
                                        {
                                            return "Yellow";
                                        }
                                }
                            }
                            else if (blueteamplayers > redteamplayers)
                            {
                                who.Team = 5;
                                who.GameType = "oneflag";
                                switch (C3Mod.C3Config.TeamColor2)
                                {
                                    case 1:
                                        {
                                            return "Blue";
                                        }
                                    case 2:
                                        {
                                            return "Green";
                                        }
                                    case 3:
                                        {
                                            return "Blue";
                                        }
                                    case 4:
                                        {
                                            return "Yellow";
                                        }
                                }
                            }

                            else
                            {
                                Random r = new Random();

                                switch (r.Next(2) + 1)
                                {
                                    case 1:
                                        {
                                            who.Team = 5;
                                            who.GameType = "oneflag";
                                            switch (C3Mod.C3Config.TeamColor1)
                                            {
                                                case 1:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 2:
                                                    {
                                                        return "Green";
                                                    }
                                                case 3:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 4:
                                                    {
                                                        return "Yellow";
                                                    }
                                            }
                                            break;
                                        }
                                    case 2:
                                        {
                                            who.Team = 6;
                                            who.GameType = "oneflag";
                                            switch (C3Mod.C3Config.TeamColor2)
                                            {
                                                case 1:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 2:
                                                    {
                                                        return "Green";
                                                    }
                                                case 3:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 4:
                                                    {
                                                        return "Yellow";
                                                    }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    }
#endregion
                //Converted v2.2
                #region TDM
                case "tdm":
                    {
                        if (who.Team != 7 || who.Team != 8)
                        {
                            int redteamplayers = 0;
                            int blueteamplayers = 0;

                            foreach (C3Player player in C3Mod.C3Players)
                            {
                                if (player.Team == 7)
                                    redteamplayers++;
                                else if (player.Team == 8)
                                    blueteamplayers++;
                            }

                            if (redteamplayers > blueteamplayers)
                            {
                                who.Team = 8;
                                who.GameType = "tdm";
                                switch (C3Mod.C3Config.TeamColor2)
                                {
                                    case 1:
                                        {
                                            return "Blue";
                                        }
                                    case 2:
                                        {
                                            return "Green";
                                        }
                                    case 3:
                                        {
                                            return "Blue";
                                        }
                                    case 4:
                                        {
                                            return "Yellow";
                                        }
                                }
                            }
                            else if (blueteamplayers > redteamplayers)
                            {
                                who.Team = 7;
                                who.GameType = "tdm";
                                switch (C3Mod.C3Config.TeamColor1)
                                {
                                    case 1:
                                        {
                                            return "Blue";
                                        }
                                    case 2:
                                        {
                                            return "Green";
                                        }
                                    case 3:
                                        {
                                            return "Blue";
                                        }
                                    case 4:
                                        {
                                            return "Yellow";
                                        }
                                }
                            }

                            else
                            {
                                Random r = new Random();

                                switch (r.Next(2) + 1)
                                {
                                    case 1:
                                        {
                                            who.Team = 7;
                                            who.GameType = "tdm";
                                            switch (C3Mod.C3Config.TeamColor1)
                                            {
                                                case 1:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 2:
                                                    {
                                                        return "Green";
                                                    }
                                                case 3:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 4:
                                                    {
                                                        return "Yellow";
                                                    }
                                            }
                                            break;
                                        }
                                    case 2:
                                        {
                                            who.Team = 8;
                                            who.GameType = "tdm";
                                            switch (C3Mod.C3Config.TeamColor2)
                                            {
                                                case 1:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 2:
                                                    {
                                                        return "Green";
                                                    }
                                                case 3:
                                                    {
                                                        return "Blue";
                                                    }
                                                case 4:
                                                    {
                                                        return "Yellow";
                                                    }
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                        break;
                    }
                #endregion
            }
            return "";
        }

        /// <summary>
        /// Broadcasts a message to all players in a running gametype
        /// </summary>
        /// <param name="gametype">"ctf","tdm","1v1","oneflag","ffa","apoc"</param>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public static void BroadcastMessageToGametype(string gametype, string message, Color color)
        {
            foreach (C3Player player in C3Mod.C3Players)
                if (player.GameType == gametype)
                    player.SendMessage(message, color);
        }

        public static C3Player GetC3PlayerByIndex(int index)
        {
            foreach (C3Player player in C3Mod.C3Players)
            {
                if (player.Index == index)
                    return player;
            }
            return new C3Player(-1);
        }

        public static C3Player GetC3PlayerByName(string name)
        {
            foreach (C3Player player in C3Mod.C3Players)
            {
                if (player.PlayerName.ToLower() == name)
                    return player;
            }
            return null;
        }

        internal static TSPlayer GetTSPlayerByIndex(int index)
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Index == index)
                    return player;
            }
            return null;
        }

        internal static NPC GetNPCByIndex(int index)
        {
            foreach (NPC npc in Main.npc)
            {
                if (npc.whoAmI == index)
                    return npc;
            }
            return new NPC();
        }

        internal static void ResetGameType(string gametype)
        {
            foreach (C3Player player in C3Mod.C3Players)
            {
                if (player.GameType == gametype)
                {
                    player.GameType = "";
                    player.Team = 0;
                }
            }
        }
    }
}
