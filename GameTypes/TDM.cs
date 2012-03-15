using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace C3Mod.GameTypes
{
    internal class TDM
    {
        public static List<TDMArena> Arenas = new List<TDMArena>();
        public static bool TDMRunning = false;
        public static bool TDMCountdown = false;
        public static Vector2[] TDMSpawns = new Vector2[2];
        public static int StartCount = 5;
        public static int VoteCount = 0;
        public static DateTime countDownTick = DateTime.UtcNow;
        public static DateTime voteCountDown = DateTime.UtcNow;
        public static DateTime scoreNotify = DateTime.UtcNow;
        public static int Team1Score = 0;
        public static int Team2Score = 0;

        public static void OnUpdate()
        {
            lock (C3Mod.C3Players)
            {
                if (C3Mod.VoteRunning && C3Mod.VoteType == "tdm")
                {
                    int VotedPlayers = 0;
                    int TotalPlayers = 0;

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "" || player.GameType == "tdm")
                            TotalPlayers++;
                        if (player.GameType == "tdm")
                            VotedPlayers++;
                    }

                    if (VotedPlayers == TotalPlayers)
                    {
                        C3Tools.BroadcastMessageToGametype("tdm", "Vote to play Team Deathmatch passed, Teleporting to start positions", Color.DarkCyan);
                        C3Mod.VoteRunning = false;
                        C3Mod.VoteType = "";
                        Team2Score = 0;
                        Team1Score = 0;
                        bool[] playersDead = new bool[Main.maxNetPlayers];
                        TpToSpawnPoint();
                        countDownTick = DateTime.UtcNow;
                        TDMCountdown = true;
                        return;
                    }

                    double tick = (DateTime.UtcNow - voteCountDown).TotalMilliseconds;
                    if (tick > (C3Mod.C3Config.VoteNotifyInterval * 1000) && VoteCount > 0)
                    {
                        if (VoteCount != 1 && VoteCount < (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval))
                        {
                            C3Tools.BroadcastMessageToGametype("tdm", "Vote still in progress, please be patient", Color.Cyan);
                            C3Tools.BroadcastMessageToGametype("", "Vote to play Team Deathmatch in progress, type /join to join the lobby", Color.Cyan);
                        }

                        VoteCount--;
                        voteCountDown = DateTime.UtcNow;
                    }

                    else if (VoteCount == 0)
                    {
                        C3Mod.VoteRunning = false;

                        int team1players = 0;
                        int team2players = 0;

                        foreach (C3Player player in C3Mod.C3Players)
                        {
                            if (player.Team == 7)
                                team1players++;
                            else if (player.Team == 8)
                                team2players++;
                        }

                        if (team1players >= C3Mod.C3Config.VoteMinimumPerTeam && team2players >= C3Mod.C3Config.VoteMinimumPerTeam)
                        {
                            C3Tools.BroadcastMessageToGametype("tdm", "Vote to play Team Deathmatch passed, Teleporting to start positions", Color.DarkCyan);
                            Team2Score = 0;
                            Team1Score = 0;
                            bool[] playersDead = new bool[Main.maxNetPlayers];
                            TpToSpawnPoint();
                            countDownTick = DateTime.UtcNow;
                            TDMCountdown = true;
                        }
                        else
                        {
                            C3Tools.BroadcastMessageToGametype("tdm", "Vote to play Team Deathmatch failed, Not enough players", Color.DarkCyan);
                            TDM.TDMSpawns = new Vector2[2];
                        }
                    }
                }

                if (TDMCountdown)
                {
                    double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
                    if (tick > 1000 && StartCount > -1)
                    {
                        if (TpToSpawnPoint() > 0)
                        {
                            if (StartCount == 0)
                            {
                                C3Tools.BroadcastMessageToGametype("tdm", "Fight!!!", Color.Cyan);
                                StartCount = 5;
                                TDMCountdown = false;
                                TDMRunning = true;
                            }
                            else
                            {
                                C3Tools.BroadcastMessageToGametype("tdm", "Game starting in " + StartCount.ToString() + "...", Color.Cyan);
                                countDownTick = DateTime.UtcNow;
                                StartCount--;
                            }
                        }
                        else
                        {
                            StartCount = 5;
                            C3Tools.ResetGameType("tdm");
                            return;
                        }
                    }
                }

                if (TDMRunning)
                {
                    int Team1Players = 0;
                    int Team2Players = 0;

                    double tick = (DateTime.UtcNow - scoreNotify).TotalMilliseconds;
                    if (tick > (C3Mod.C3Config.TDMScoreNotifyInterval * 1000))
                    {
                        switch (C3Mod.C3Config.TeamColor1)
                        {
                            case 1:
                                {
                                    if (C3Mod.C3Config.TeamColor2 == 2)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Red - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Green", Color.Cyan);
                                    else if (C3Mod.C3Config.TeamColor2 == 3)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Red - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Blue", Color.Cyan);
                                    else if (C3Mod.C3Config.TeamColor2 == 4)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Red - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Yellow", Color.Cyan);
                                    break;
                                }
                            case 2:
                                {
                                    if (C3Mod.C3Config.TeamColor2 == 1)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Green - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Red", Color.Cyan);
                                    else if (C3Mod.C3Config.TeamColor2 == 3)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Green - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Blue", Color.Cyan);
                                    else if (C3Mod.C3Config.TeamColor2 == 4)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Green - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Yellow", Color.Cyan);
                                    break;
                                }
                            case 3:
                                {
                                    if (C3Mod.C3Config.TeamColor2 == 1)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Blue - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Red", Color.Cyan);
                                    else if (C3Mod.C3Config.TeamColor2 == 2)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Blue - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Green", Color.Cyan);
                                    else if (C3Mod.C3Config.TeamColor2 == 4)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Blue - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Blue", Color.Cyan);
                                    break;
                                }
                            case 4:
                                {
                                    if (C3Mod.C3Config.TeamColor2 == 1)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Yellow - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Red", Color.Cyan);
                                    else if (C3Mod.C3Config.TeamColor2 == 2)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Yellow - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Green", Color.Cyan);
                                    else if (C3Mod.C3Config.TeamColor2 == 3)
                                        C3Tools.BroadcastMessageToGametype("tdm", "Current score: Yellow - " + Team1Score.ToString() + " --- " + Team2Score.ToString() + " - Blue", Color.Cyan);
                                    break;
                                }
                        }
                        scoreNotify = DateTime.UtcNow;
                    }

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.TSPlayer == null)
                        {
                            C3Mod.C3Players.Remove(player);
                            break;
                        }

                        if (player.GameType == "tdm")
                        {
                            if (!player.TSPlayer.TpLock)
                                if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }

                            if (player.Team == 7)
                                Team1Players++;
                            else if (player.Team == 8)
                                Team2Players++;

                            if ((player.Team == 7 && Main.player[player.Index].team != C3Mod.C3Config.TeamColor1))
                                TShock.Players[player.Index].SetTeam(C3Mod.C3Config.TeamColor1);
                            if ((player.Team == 8 && Main.player[player.Index].team != C3Mod.C3Config.TeamColor2))
                                TShock.Players[player.Index].SetTeam(C3Mod.C3Config.TeamColor2);

                            if (!Main.player[player.Index].hostile)
                            {
                                Main.player[player.Index].hostile = true;
                                NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", player.Index, 0f, 0f, 0f);
                            }

                            //Respawn on flag
                            if (Main.player[player.Index].dead && !player.Dead)
                            {
                                if (player.Team == 7)
                                {
                                    Team2Score++;
                                    player.Dead = true;
                                }
                                else if (player.Team == 8)
                                {
                                    Team1Score++;
                                    player.Dead = true;
                                }
                            }

                            if (!Main.player[player.Index].dead && player.Dead)
                            {
                                player.Dead = false;
                                player.TSPlayer.TpLock = false;
                                if (player.Team == 7)
                                    TShock.Players[player.Index].Teleport((int)TDMSpawns[0].X, (int)TDMSpawns[0].Y);
                                else if (player.Team == 8)
                                    TShock.Players[player.Index].Teleport((int)TDMSpawns[1].X, (int)TDMSpawns[1].Y);
                                NetMessage.SendData(4, -1, player.Index, player.PlayerName, player.Index, 0f, 0f, 0f, 0);
                                if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }
                            }
                        }
                    }

                    if (Team1Players == 0 || Team2Players == 0)
                    {
                        C3Tools.BroadcastMessageToGametype("tdm", "Not enough players to continue, ending game", Color.DarkCyan);
                        TDMRunning = false;
                        TpToSpawns(false);
                        C3Tools.ResetGameType("tdm");
                        TDM.TDMSpawns = new Vector2[2];
                        return;
                    }

                    if (Team2Score == C3Mod.C3Config.TeamDeathmatchScorelimit)
                    {
                        TDMRunning = false;
                        if (C3Mod.C3Config.TeamColor2 == 1)
                            C3Tools.BroadcastMessageToGametype("tdm", "RED TEAM WINS!", Color.OrangeRed);
                        else if (C3Mod.C3Config.TeamColor2 == 2)
                            C3Tools.BroadcastMessageToGametype("tdm", "GREEN TEAM WINS!", Color.LightGreen);
                        else if (C3Mod.C3Config.TeamColor2 == 3)
                            C3Tools.BroadcastMessageToGametype("tdm", "BLUE TEAM WINS!", Color.LightBlue);
                        else if (C3Mod.C3Config.TeamColor2 == 4)
                            C3Tools.BroadcastMessageToGametype("tdm", "YELLOW TEAM WINS!", Color.LightYellow);

                        List<C3Player> LostPlayers = new List<C3Player>();
                        List<C3Player> WonPlayers = new List<C3Player>();

                        foreach (C3Player player1 in C3Mod.C3Players)
                        {
                            if (player1.GameType == "tdm")
                            {
                                if (player1.Team == 8)
                                    WonPlayers.Add(player1);
                                if (player1.Team == 7)
                                    LostPlayers.Add(player1);
                            }
                        }

                        C3Events.GameEnd(WonPlayers, LostPlayers, "tdm", Team2Score, Team1Score);

                        TpToSpawns(false);
                        C3Tools.ResetGameType("tdm");
                        TDM.TDMSpawns = new Vector2[2];
                        return;
                    }

                    if (Team1Score == C3Mod.C3Config.TeamDeathmatchScorelimit)
                    {
                        TDMRunning = false;
                        if (C3Mod.C3Config.TeamColor1 == 1)
                            C3Tools.BroadcastMessageToGametype("tdm", "RED TEAM WINS!", Color.OrangeRed);
                        else if (C3Mod.C3Config.TeamColor1 == 2)
                            C3Tools.BroadcastMessageToGametype("tdm", "GREEN TEAM WINS!", Color.LightGreen);
                        else if (C3Mod.C3Config.TeamColor1 == 3)
                            C3Tools.BroadcastMessageToGametype("tdm", "BLUE TEAM WINS!", Color.LightBlue);
                        else if (C3Mod.C3Config.TeamColor1 == 4)
                            C3Tools.BroadcastMessageToGametype("tdm", "YELLOW TEAM WINS!", Color.LightYellow);

                        List<C3Player> LostPlayers = new List<C3Player>();
                        List<C3Player> WonPlayers = new List<C3Player>();

                        foreach (C3Player player1 in C3Mod.C3Players)
                        {
                            if (player1.GameType == "tdm")
                            {
                                if (player1.Team == 7)
                                    WonPlayers.Add(player1);
                                if (player1.Team == 8)
                                    LostPlayers.Add(player1);
                            }
                        }

                        C3Events.GameEnd(WonPlayers, LostPlayers, "tdm", Team1Score, Team2Score);

                        TpToSpawns(false);
                        C3Tools.ResetGameType("tdm");
                        TDM.TDMSpawns = new Vector2[2];
                        return;
                    }
                }
            }
        }        

        public static int TpToSpawnPoint()
        {
            try
            {
                bool RedTeamPlayer = false;
                bool BlueTeamPlayer = false;

                for (int i = 0; i < C3Mod.C3Players.Count; i++)
                {
                    if ((C3Mod.C3Players[i].Team == 7 && Main.player[C3Mod.C3Players[i].Index].team != C3Mod.C3Config.TeamColor1))
                        TShock.Players[C3Mod.C3Players[i].Index].SetTeam(C3Mod.C3Config.TeamColor1);
                    else if (C3Mod.C3Players[i].Team == 8 && Main.player[C3Mod.C3Players[i].Index].team != C3Mod.C3Config.TeamColor2)
                        TShock.Players[C3Mod.C3Players[i].Index].SetTeam(C3Mod.C3Config.TeamColor2);

                    if (C3Mod.C3Players[i].Team == 7)
                    {
                        RedTeamPlayer = true;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].tileX != (int)(TDMSpawns[0].X) || C3Mod.C3Players[i].tileY != (int)(TDMSpawns[0].Y - 3))
                        {
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)TDMSpawns[0].X, (int)TDMSpawns[0].Y);
                        }
                        if(C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                    else if (C3Mod.C3Players[i].Team == 8)
                    {
                        BlueTeamPlayer = true;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].tileX != (int)(TDMSpawns[1].X) || C3Mod.C3Players[i].tileY != (int)(TDMSpawns[1].Y - 3))
                        {
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)TDMSpawns[1].X, (int)TDMSpawns[1].Y);
                        }
                        if(C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                }
                if (!RedTeamPlayer || !BlueTeamPlayer)
                {
                    C3Tools.BroadcastMessageToGametype("tdm", "Not enough players to continue, ending game", Color.DarkCyan);
                    TDMRunning = false;
                    TDMCountdown = false;
                    TDM.TDMSpawns = new Vector2[2];
                    return 0;
                }
                return 1;
            }
            catch { return 0; }
        }

        public static void TpToSpawns(bool pvpstate)
        {
            for (int i = 0; i < C3Mod.C3Players.Count; i++)
            {
                if (C3Mod.C3Players[i].Team == 7)
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                }
                if (C3Mod.C3Players[i].Team == 8)
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                }
            }
        }
    }

    internal class TDMArena
    {
        public string Name = "";
        public Vector2[] Spawns = new Vector2[2];

        public TDMArena(Vector2 redspawn, Vector2 bluespawn, string name)
        {
            Spawns[0] = redspawn;
            Spawns[1] = bluespawn;
            Name = name;
        }
    }
}