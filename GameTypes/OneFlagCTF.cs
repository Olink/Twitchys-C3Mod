using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace C3Mod.GameTypes
{
    internal class OneFlagCTF
    {
        public static bool OneFlagGameRunning = false;
        public static bool OneFlagGameCountdown = false;
        public static List<OneFlagArena> Arenas = new List<OneFlagArena>();
        public static Vector2 FlagPoint = new Vector2();
        public static Vector2[] SpawnPoint = new Vector2[2];
        public static int Team1Score = 0;
        public static int Team2Score = 0;
        public static bool[] playersDead = new bool[Main.maxNetPlayers];
        public static DateTime countDownTick = DateTime.UtcNow;
        public static DateTime voteCountDown = DateTime.UtcNow;
        public static C3Player FlagCarrier;
        public static int StartCount = 5;
        public static int VoteCount = 0;

        public static void OnUpdate()
        {
            lock (C3Mod.C3Players)
            {
                if (C3Mod.VoteRunning && C3Mod.VoteType == "oneflag")
                {
                    int VotedPlayers = 0;
                    int TotalPlayers = 0;

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "" || player.GameType == "oneflag")
                            TotalPlayers++;
                        if (player.GameType == "oneflag")
                            VotedPlayers++;
                    }

                    if (VotedPlayers == TotalPlayers)
                    {
                        C3Tools.BroadcastMessageToGametype("oneflag", "Vote to play One Flag CTF passed, Teleporting to start positions", Color.DarkCyan);
                        C3Mod.VoteRunning = false;
                        C3Mod.VoteType = "";
                        FlagCarrier = null;
                        Team1Score = 0;
                        Team2Score = 0;
                        bool[] playersDead = new bool[Main.maxNetPlayers];
                        TpToOneFlagSpawns();
                        countDownTick = DateTime.UtcNow;
                        OneFlagGameCountdown = true;
                        return;
                    }

                    double tick = (DateTime.UtcNow - voteCountDown).TotalMilliseconds;
                    if (tick > (C3Mod.C3Config.VoteNotifyInterval * 1000) && VoteCount > 0)
                    {
                        if (VoteCount != 1 && VoteCount < (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval))
                        {
                            C3Tools.BroadcastMessageToGametype("oneflag", "Vote still in progress, please be patient", Color.Cyan);
                            C3Tools.BroadcastMessageToGametype("", "Vote to play One Flag CTF in progress, type /join to join the lobby", Color.Cyan);
                        }

                        VoteCount--;
                        voteCountDown = DateTime.UtcNow;
                    }

                    else if (VoteCount == 0)
                    {
                        C3Mod.VoteRunning = false;

                        int redteamplayers = 0;
                        int blueteamplayers = 0;

                        foreach (C3Player player in C3Mod.C3Players)
                        {
                            if (player.Team == 5)
                                redteamplayers++;
                            else if (player.Team == 6)
                                blueteamplayers++;
                        }

                        if (redteamplayers >= C3Mod.C3Config.VoteMinimumPerTeam && blueteamplayers >= C3Mod.C3Config.VoteMinimumPerTeam)
                        {
                            C3Tools.BroadcastMessageToGametype("oneflag", "Vote to play One Flag CTF passed, Teleporting to start positions", Color.DarkCyan);
                            FlagCarrier = null;
                            Team1Score = 0;
                            Team2Score = 0;
                            bool[] playersDead = new bool[Main.maxNetPlayers];
                            TpToOneFlagSpawns();
                            countDownTick = DateTime.UtcNow;
                            OneFlagGameCountdown = true;
                        }
                        else
                            C3Tools.BroadcastMessageToGametype("oneflag", "Vote to play One Flag CTF failed, Not enough players", Color.DarkCyan);
                    }
                }
                if (OneFlagGameCountdown)
                {
                    double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
                    if (tick > 1000 && StartCount > -1)
                    {
                        if (TpToOneFlagSpawns() > 0)
                        {
                            if (StartCount == 0)
                            {
                                C3Tools.BroadcastMessageToGametype("oneflag", "Capture...The...Flag!!!", Color.Cyan);
                                StartCount = 5;
                                OneFlagGameCountdown = false;
                                OneFlagGameRunning = true;
                            }
                            else
                            {
                                C3Tools.BroadcastMessageToGametype("oneflag", "Game starting in " + StartCount.ToString() + "...", Color.Cyan);
                                countDownTick = DateTime.UtcNow;
                                StartCount--;
                            }
                        }
                        else
                        {
                            StartCount = 5;
                            C3Tools.ResetGameType("oneflag");
                            return;
                        }
                    }
                }

                if (OneFlagGameRunning)
                {
                    int team1players = 0;
                    int team2players = 0;

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.TSPlayer == null)
                        {
                            C3Mod.C3Players.Remove(player);
                            break;
                        }

                        if (player.GameType == "oneflag")
                        {
                            if (!player.TSPlayer.TpLock)
                                if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }

                            if (player.Team == 5)
                                team1players++;
                            else if (player.Team == 6)
                                team2players++;

                            if ((player.Team == 5 && Main.player[player.Index].team != C3Mod.C3Config.TeamColor1))
                                TShock.Players[player.Index].SetTeam(C3Mod.C3Config.TeamColor1);
                            else if (player.Team == 6 && Main.player[player.Index].team != C3Mod.C3Config.TeamColor2)
                                TShock.Players[player.Index].SetTeam(C3Mod.C3Config.TeamColor2);

                            if (!Main.player[player.Index].hostile)
                            {
                                Main.player[player.Index].hostile = true;
                                NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", player.Index, 0f, 0f, 0f);
                            }

                            //Respawn on flag
                            if (Main.player[player.Index].dead)
                                player.Dead = true;
                            else
                            {
                                if (player.Dead)
                                {
                                    player.Dead = false;
                                    player.TSPlayer.TpLock = false;

                                    if (player.Team == 5)
                                        TShock.Players[player.Index].Teleport((int)SpawnPoint[0].X, (int)SpawnPoint[0].Y);
                                    else if (player.Team == 6)
                                        TShock.Players[player.Index].Teleport((int)SpawnPoint[1].X, (int)SpawnPoint[1].Y);
                                    NetMessage.SendData(4, -1, player.Index, player.PlayerName, player.Index, 0f, 0f, 0f, 0);
                                    if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }
                                }
                            }

                            //Grab flag
                            if (!player.Dead)
                            {
                                if (FlagCarrier == null)
                                {
                                    if ((int)player.tileX <= (int)FlagPoint.X + 2 && (int)player.tileX >= (int)FlagPoint.X - 2 && (int)player.tileY == (int)FlagPoint.Y - 3)
                                    {
                                        FlagCarrier = player;

                                        if (player.Team == 5)
                                        {
                                            switch (C3Mod.C3Config.TeamColor1)
                                            {
                                                case 1:
                                                    {
                                                        C3Tools.BroadcastMessageToGametype("oneflag", Main.player[player.Index].name + " has the flag!", Color.OrangeRed);
                                                        break;
                                                    }
                                                case 2:
                                                    {
                                                        C3Tools.BroadcastMessageToGametype("oneflag", Main.player[player.Index].name + " has the flag!", Color.LightGreen);
                                                        break;
                                                    }
                                                case 3:
                                                    {
                                                        C3Tools.BroadcastMessageToGametype("oneflag", Main.player[player.Index].name + " has the flag!", Color.LightBlue);
                                                        break;
                                                    }
                                                case 4:
                                                    {
                                                        C3Tools.BroadcastMessageToGametype("oneflag", Main.player[player.Index].name + " has the flag!", Color.LightYellow);
                                                        break;
                                                    }
                                            }
                                        }
                                        else if (player.Team == 6)
                                            switch (C3Mod.C3Config.TeamColor2)
                                            {
                                                case 1:
                                                    {
                                                        C3Tools.BroadcastMessageToGametype("oneflag", Main.player[player.Index].name + " has the flag!", Color.OrangeRed);
                                                        break;
                                                    }
                                                case 2:
                                                    {
                                                        C3Tools.BroadcastMessageToGametype("oneflag", Main.player[player.Index].name + " has the flag!", Color.LightGreen);
                                                        break;
                                                    }
                                                case 3:
                                                    {
                                                        C3Tools.BroadcastMessageToGametype("oneflag", Main.player[player.Index].name + " has the flag!", Color.LightBlue);
                                                        break;
                                                    }
                                                case 4:
                                                    {
                                                        C3Tools.BroadcastMessageToGametype("oneflag", Main.player[player.Index].name + " has the flag!", Color.LightYellow);
                                                        break;
                                                    }
                                            }
                                        C3Events.FlagGrabbed(FlagCarrier, "oneflag");
                                    }
                                }
                            }
                        }
                    }

                    if (team1players == 0 || team2players == 0)
                    {
                        C3Tools.BroadcastMessageToGametype("oneflag", "One Flag CTF stopped, Not enough players to continue", Color.DarkCyan);
                        OneFlagGameRunning = false;
                        SendToSpawn(false);
                        C3Tools.ResetGameType("oneflag");
                        return;
                    }

                    //Check on flag carrier
                    if (FlagCarrier != null)
                    {
                        //Make them drop the flag
                        if (Main.player[FlagCarrier.Index].dead)
                        {
                            if (FlagCarrier.Team == 5)
                                switch (C3Mod.C3Config.TeamColor1)
                                {
                                    case 1:
                                        {
                                            C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.OrangeRed);
                                            break;
                                        }
                                    case 2:
                                        {
                                            C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.LightGreen);
                                            break;
                                        }
                                    case 3:
                                        {
                                            C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.LightBlue);
                                            break;
                                        }
                                    case 4:
                                        {
                                            C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.LightYellow);
                                            break;
                                        }
                                }
                            else if (FlagCarrier.Team == 6)
                                switch (C3Mod.C3Config.TeamColor2)
                                {
                                    case 1:
                                        {
                                            C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.OrangeRed);
                                            break;
                                        }
                                    case 2:
                                        {
                                            C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.LightGreen);
                                            break;
                                        }
                                    case 3:
                                        {
                                            C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.LightBlue);
                                            break;
                                        }
                                    case 4:
                                        {
                                            C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + " dropped the flag!", Color.LightYellow);
                                            break;
                                        }
                                }

                            FlagCarrier = null;
                        }
                        //Capture the flag
                        else
                        {
                            if (FlagCarrier.Team == 5)
                            {
                                if ((int)FlagCarrier.tileX <= (int)SpawnPoint[0].X + 2 && (int)FlagCarrier.tileX >= (int)SpawnPoint[0].X - 2 && (int)FlagCarrier.tileY == (int)SpawnPoint[0].Y - 3)
                                {
                                    Team1Score++;

                                    switch (C3Mod.C3Config.TeamColor1)
                                    {
                                        case 1:
                                            {
                                                if (C3Mod.C3Config.TeamColor2 == 2)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Red - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Green", Color.OrangeRed);
                                                else if (C3Mod.C3Config.TeamColor2 == 3)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Red - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Blue", Color.OrangeRed);
                                                else if (C3Mod.C3Config.TeamColor2 == 4)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Red - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Yellow", Color.OrangeRed);
                                                break;
                                            }
                                        case 2:
                                            {
                                                if (C3Mod.C3Config.TeamColor2 == 1)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Green - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Red", Color.LightGreen);
                                                else if (C3Mod.C3Config.TeamColor2 == 3)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Green - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Blue", Color.LightGreen);
                                                else if (C3Mod.C3Config.TeamColor2 == 4)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Green - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Yellow", Color.LightGreen);
                                                break;
                                            }
                                        case 3:
                                            {
                                                if (C3Mod.C3Config.TeamColor2 == 1)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Blue - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Red", Color.LightBlue);
                                                else if (C3Mod.C3Config.TeamColor2 == 2)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Blue - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Green", Color.LightBlue);
                                                else if (C3Mod.C3Config.TeamColor2 == 4)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Blue - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Yellow", Color.LightBlue);
                                                break;
                                            }
                                        case 4:
                                            {
                                                if (C3Mod.C3Config.TeamColor2 == 1)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Red", Color.LightYellow);
                                                else if (C3Mod.C3Config.TeamColor2 == 2)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Green", Color.LightYellow);
                                                else if (C3Mod.C3Config.TeamColor2 == 3)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Blue", Color.LightYellow);
                                                break;
                                            }
                                    }
                                    C3Events.FlagCapture(FlagCarrier, "oneflag", "Team1", Team1Score, Team2Score);
                                    FlagCarrier = null;

                                    if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && Team1Score != C3Mod.C3Config.CTFScoreLimit)
                                        TpToOneFlagSpawns();

                                    if (C3Mod.C3Config.ReCountdownOnFlagCapture && Team1Score != C3Mod.C3Config.CTFScoreLimit)
                                    {
                                        OneFlagGameRunning = false;
                                        OneFlagGameCountdown = true;
                                    }

                                    if (C3Mod.C3Config.HealPlayersOnFlagCapture)
                                    {
                                        Item heart = TShock.Utils.GetItemById(58);
                                        Item star = TShock.Utils.GetItemById(184);

                                        foreach (C3Player player in C3Mod.C3Players)
                                        {
                                            if (player.GameType == "ctf")
                                            {
                                                player.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                                                player.GiveItem(star.type, star.name, star.width, star.height, 20);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (FlagCarrier.Team == 6)
                            {
                                if ((int)FlagCarrier.tileX <= (int)SpawnPoint[1].X + 2 && (int)FlagCarrier.tileX >= (int)SpawnPoint[1].X - 2 && (int)FlagCarrier.tileY == (int)SpawnPoint[1].Y - 3)
                                {
                                    Team2Score++;
                                    switch (C3Mod.C3Config.TeamColor2)
                                    {
                                        case 1:
                                            {
                                                if (C3Mod.C3Config.TeamColor1 == 2)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Red - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Green", Color.OrangeRed);
                                                else if (C3Mod.C3Config.TeamColor1 == 3)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Red - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Blue", Color.OrangeRed);
                                                else if (C3Mod.C3Config.TeamColor1 == 4)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Red - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Yellow", Color.OrangeRed);
                                                break;
                                            }
                                        case 2:
                                            {
                                                if (C3Mod.C3Config.TeamColor1 == 1)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Green - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Red", Color.LightGreen);
                                                else if (C3Mod.C3Config.TeamColor1 == 3)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Green - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Blue", Color.LightGreen);
                                                else if (C3Mod.C3Config.TeamColor1 == 4)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Green - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Yellow", Color.LightGreen);
                                                break;
                                            }
                                        case 3:
                                            {
                                                if (C3Mod.C3Config.TeamColor1 == 1)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Blue - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Red", Color.LightBlue);
                                                else if (C3Mod.C3Config.TeamColor1 == 2)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Blue - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Green", Color.LightBlue);
                                                else if (C3Mod.C3Config.TeamColor1 == 4)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Blue - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Yellow", Color.LightBlue);
                                                break;
                                            }
                                        case 4:
                                            {
                                                if (C3Mod.C3Config.TeamColor1 == 1)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Red", Color.LightYellow);
                                                else if (C3Mod.C3Config.TeamColor1 == 2)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Green", Color.LightYellow);
                                                else if (C3Mod.C3Config.TeamColor1 == 3)
                                                    C3Tools.BroadcastMessageToGametype("oneflag", FlagCarrier.PlayerName + ": Scores!  Yellow - " + Team2Score.ToString() + " --- " + Team1Score.ToString() + " - Blue", Color.LightYellow);
                                                break;
                                            }
                                    }
                                    C3Events.FlagCapture(FlagCarrier, "oneflag", "Team2", Team2Score, Team1Score);
                                    FlagCarrier = null;

                                    if (C3Mod.C3Config.RespawnPlayersOnFlagCapture && Team2Score != C3Mod.C3Config.CTFScoreLimit)
                                        TpToOneFlagSpawns();

                                    if (C3Mod.C3Config.ReCountdownOnFlagCapture && Team2Score != C3Mod.C3Config.CTFScoreLimit)
                                    {
                                        OneFlagGameRunning = false;
                                        OneFlagGameCountdown = true;
                                    }

                                    if (C3Mod.C3Config.HealPlayersOnFlagCapture)
                                    {
                                        Item heart = TShock.Utils.GetItemById(58);
                                        Item star = TShock.Utils.GetItemById(184);

                                        foreach (C3Player player in C3Mod.C3Players)
                                        {
                                            if (player.GameType == "ctf")
                                            {
                                                player.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                                                player.GiveItem(star.type, star.name, star.width, star.height, 20);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Team1Score == C3Mod.C3Config.CTFScoreLimit)
                    {
                        OneFlagGameRunning = false;
                        if (C3Mod.C3Config.TeamColor2 == 1)
                            C3Tools.BroadcastMessageToGametype("ctf", "RED TEAM WINS!", Color.OrangeRed);
                        else if (C3Mod.C3Config.TeamColor2 == 2)
                            C3Tools.BroadcastMessageToGametype("ctf", "GREEN TEAM WINS!", Color.LightGreen);
                        else if (C3Mod.C3Config.TeamColor2 == 3)
                            C3Tools.BroadcastMessageToGametype("ctf", "BLUE TEAM WINS!", Color.LightBlue);
                        else if (C3Mod.C3Config.TeamColor2 == 4)
                            C3Tools.BroadcastMessageToGametype("ctf", "YELLOW TEAM WINS!", Color.LightYellow);

                        List<C3Player> LostPlayers = new List<C3Player>();
                        List<C3Player> WonPlayers = new List<C3Player>();

                        foreach (C3Player player1 in C3Mod.C3Players)
                        {
                            if (player1.GameType == "oneflag")
                            {
                                if (player1.Team == 6)
                                    WonPlayers.Add(player1);
                                if (player1.Team == 5)
                                    LostPlayers.Add(player1);
                            }
                        }

                        C3Events.GameEnd(WonPlayers, LostPlayers, "oneflag", Team2Score, Team1Score);

                        SendToSpawn(false);
                        C3Tools.ResetGameType("oneflag");
                        OneFlagCTF.FlagPoint = new Vector2();
                        OneFlagCTF.SpawnPoint = new Vector2[2];
                        return;
                    }
                    if (Team2Score == C3Mod.C3Config.CTFScoreLimit)
                    {
                        OneFlagGameRunning = false;
                        if (C3Mod.C3Config.TeamColor1 == 1)
                            C3Tools.BroadcastMessageToGametype("ctf", "RED TEAM WINS!", Color.OrangeRed);
                        else if (C3Mod.C3Config.TeamColor1 == 2)
                            C3Tools.BroadcastMessageToGametype("ctf", "GREEN TEAM WINS!", Color.LightGreen);
                        else if (C3Mod.C3Config.TeamColor1 == 3)
                            C3Tools.BroadcastMessageToGametype("ctf", "BLUE TEAM WINS!", Color.LightBlue);
                        else if (C3Mod.C3Config.TeamColor1 == 4)
                            C3Tools.BroadcastMessageToGametype("ctf", "YELLOW TEAM WINS!", Color.LightYellow);

                        List<C3Player> LostPlayers = new List<C3Player>();
                        List<C3Player> WonPlayers = new List<C3Player>();

                        foreach (C3Player player1 in C3Mod.C3Players)
                        {
                            if (player1.GameType == "oneflag")
                            {
                                if (player1.Team == 5)
                                    WonPlayers.Add(player1);
                                if (player1.Team == 6)
                                    LostPlayers.Add(player1);
                            }
                        }

                        C3Events.GameEnd(WonPlayers, LostPlayers, "oneflag", Team1Score, Team2Score);

                        SendToSpawn(false);
                        C3Tools.ResetGameType("oneflag");
                        OneFlagCTF.FlagPoint = new Vector2();
                        OneFlagCTF.SpawnPoint = new Vector2[2];
                        return;
                    }
                }
            }
        }

        public static int TpToOneFlagSpawns()
        {
            int team1players = 0;
            int team2players = 0;

            for (int i = 0; i < C3Mod.C3Players.Count; i++)
            {
                if ((C3Mod.C3Players[i].Team == 5 && Main.player[C3Mod.C3Players[i].Index].team != C3Mod.C3Config.TeamColor1))
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(C3Mod.C3Config.TeamColor1);
                else if (C3Mod.C3Players[i].Team == 6 && Main.player[C3Mod.C3Players[i].Index].team != C3Mod.C3Config.TeamColor2)
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(C3Mod.C3Config.TeamColor2);

                if (C3Mod.C3Players[i].Team == 5)
                {
                    team1players++;
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    if (C3Mod.C3Players[i].tileX != (int)(SpawnPoint[0].X) || C3Mod.C3Players[i].tileY != (int)(SpawnPoint[0].Y - 3))
                        TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)SpawnPoint[0].X, (int)SpawnPoint[0].Y);
                    if (C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                }
                else if (C3Mod.C3Players[i].Team == 6)
                {
                    team2players++;
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    if (C3Mod.C3Players[i].tileX != (int)(SpawnPoint[1].X) || C3Mod.C3Players[i].tileY != (int)(SpawnPoint[1].Y - 3))
                        TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)SpawnPoint[1].X, (int)SpawnPoint[1].Y);
                    if (C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                }
            }

            if (team1players == 0 || team2players == 0)
            {
                C3Tools.BroadcastMessageToGametype("oneflag", "Not enough players to start One Flag CTF", Color.DarkCyan);
                OneFlagGameRunning = false;
                OneFlagGameCountdown = false;
                OneFlagCTF.FlagPoint = new Vector2();
                OneFlagCTF.SpawnPoint = new Vector2[2];
                return 0;
            }
            return 1;
        }

        public static void SendToSpawn(bool pvpstate)
        {
            for (int i = 0; i < C3Mod.C3Players.Count; i++)
            {
                if (C3Mod.C3Players[i].Team == 5)
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                }
                else if (C3Mod.C3Players[i].Team == 6)
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                }
            }
        }
    }

    internal class OneFlagArena
    {
        public Vector2[] Spawns = new Vector2[2];
        public Vector2 Flag = new Vector2();
        public string Name = "";

        public OneFlagArena(Vector2 flag, Vector2 redspawn, Vector2 bluespawn, string name)
        {
            Flag = flag;
            Spawns[0] = redspawn;
            Spawns[1] = bluespawn;
            Name = name;
        }
    }
}
