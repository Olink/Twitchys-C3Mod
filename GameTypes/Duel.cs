using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace C3Mod.GameTypes
{
    //Converted v2.2
    internal class Duel
    {
        public static List<DuelArena> Arenas = new List<DuelArena>();
        public static bool DuelRunning = false;
        public static bool DuelCountdown = false;
        public static Vector2[] DuelSpawns = new Vector2[2];
        public static int StartCount = 5;
        public static DateTime countDownTick = DateTime.UtcNow;
        public static int Team1PlayerScore = 0;
        public static int Team2PlayerScore = 0;

        public static C3Player Team1Player;
        public static C3Player Team2Player;

        public static void OnUpdate()
        {
            lock (C3Mod.C3Players)
            {
                foreach (C3Player player in C3Mod.C3Players)
                {
                    if (player.Challenging != null)
                    {
                        double tick = (DateTime.UtcNow - player.ChallengeTick).TotalMilliseconds;
                        if (tick > (C3Mod.C3Config.DuelNotifyInterval * 1000))
                        {
                            if (player.ChallengeNotifyCount != 1)
                            {
                                player.Challenging.SendMessage("You have been challenged to a duel by: " + player.PlayerName + ", Arena: " + Arenas[player.Challenging.ChallengeArena - 1].Name, Color.Cyan);
                                player.Challenging.SendMessage("Type /accept to accept this challenge!", Color.Cyan);
                            }
                            player.ChallengeNotifyCount--;
                            player.ChallengeTick = DateTime.UtcNow;
                        }
                        else if (player.ChallengeNotifyCount == 0)
                        {
                            player.Challenging.SendMessage("Challenge by: " + player.PlayerName + " timed out", Color.Cyan);
                            player.SendMessage(player.Challenging.PlayerName + ": Did not answer your challenge in the required amount of time", Color.DarkCyan);
                            player.Challenging = null;
                            player.ChallengeNotifyCount = 5;
                        }
                    }
                }
            }

            if (DuelCountdown)
            {
                double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
                if (tick > 1000 && StartCount > -1)
                {
                    if (TpToSpawnPoint() > 0)
                    {
                        if (StartCount == 0)
                        {
                            C3Tools.BroadcastMessageToGametype("1v1", "Fight!!!", Color.Cyan);
                            StartCount = 5;
                            DuelCountdown = false;
                            DuelRunning = true;
                        }
                        else
                        {
                            C3Tools.BroadcastMessageToGametype("1v1", "Game starting in " + StartCount.ToString() + "...", Color.Cyan);
                            countDownTick = DateTime.UtcNow;
                            StartCount--;
                        }
                    }
                    else
                    {
                        StartCount = 5;
                        C3Tools.ResetGameType("1v1");
                        return;
                    }
                }
            }

            if (DuelRunning)
            {
                int Team1PlayerCount = 0;
                int Team2PlayerCount = 0;

                lock (C3Mod.C3Players)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.TSPlayer == null)
                        {
                            C3Mod.C3Players.Remove(player);
                            break;
                        }

                        if (player.GameType == "1v1")
                        {
                            if (!player.TSPlayer.TpLock)
                                if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }

                            if (player.Team == 3)
                                Team1PlayerCount++;
                            else if (player.Team == 4)
                                Team2PlayerCount++;
                            if ((player.Team == 3 && Main.player[player.Index].team != C3Mod.C3Config.TeamColor1))
                                TShock.Players[player.Index].SetTeam(C3Mod.C3Config.TeamColor1);
                            else if (player.Team == 4 && Main.player[player.Index].team != C3Mod.C3Config.TeamColor2)
                                TShock.Players[player.Index].SetTeam(C3Mod.C3Config.TeamColor2);

                            if (!Main.player[player.Index].hostile)
                            {
                                Main.player[player.Index].hostile = true;
                                NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", player.Index, 0f, 0f, 0f);
                            }

                            //Respawn on flag
                            if (Main.player[player.Index].dead)
                            {
                                if (player.Team == 3)
                                {
                                    Team2PlayerScore++;

                                    if (Team2PlayerScore != C3Mod.C3Config.DuelScoreLimit)
                                    {
                                        C3Tools.BroadcastMessageToGametype("1v1", Team2Player.PlayerName + ": Scores!", Color.DarkCyan);
                                        TpToSpawnPoint();
                                        DuelRunning = false;
                                        DuelCountdown = true;

                                        Item heart = TShock.Utils.GetItemById(58);
                                        Item star = TShock.Utils.GetItemById(184);
                                        foreach (C3Player players in C3Mod.C3Players)
                                        {
                                            if (players.GameType == "1v1")
                                            {
                                                players.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                                                players.GiveItem(star.type, star.name, star.width, star.height, 20);
                                            }
                                        }
                                    }
                                }
                                else if (player.Team == 4)
                                {
                                    Team1PlayerScore++;

                                    if (Team1PlayerScore != C3Mod.C3Config.DuelScoreLimit)
                                    {
                                        C3Tools.BroadcastMessageToGametype("1v1", Team1Player.PlayerName + ": Scores!", Color.DarkCyan);
                                        TpToSpawnPoint();
                                        DuelRunning = false;
                                        DuelCountdown = true;

                                        Item heart = TShock.Utils.GetItemById(58);
                                        Item star = TShock.Utils.GetItemById(184);

                                        foreach (C3Player players in C3Mod.C3Players)
                                        {
                                            if (players.GameType == "1v1")
                                            {
                                                players.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                                                players.GiveItem(star.type, star.name, star.width, star.height, 20);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (Team1PlayerCount == 0 || Team2PlayerCount == 0)
                {
                    C3Tools.BroadcastMessageToGametype("1v1", "Opponent left, ending game", Color.DarkCyan);
                    DuelRunning = false;
                    TpToSpawns(false);
                    C3Tools.ResetGameType("1v1");
                    Team1Player = null;
                    Team2Player = null;
                    DuelSpawns = new Vector2[2];
                    return;
                }

                if (Team2PlayerScore == C3Mod.C3Config.DuelScoreLimit)
                {
                    DuelRunning = false;
                    C3Tools.BroadcastMessageToGametype("1v1", Team2Player.PlayerName + ": WINS!", Color.DarkCyan);
                    C3Tools.BroadcastMessageToGametype("", Team2Player.PlayerName + ": Beat " + Team1Player.PlayerName + " in a duel!", Color.DarkCyan);

                    List<C3Player> LostPlayers = new List<C3Player>();
                    List<C3Player> WonPlayers = new List<C3Player>();
                    WonPlayers.Add(Team2Player);
                    LostPlayers.Add(Team1Player);

                    C3Events.GameEnd(WonPlayers, LostPlayers, "1v1", Team2Player.FFAScore, Team1Player.FFAScore);

                    TpToSpawns(false);
                    C3Tools.ResetGameType("1v1");
                    Team1Player = null;
                    Team2Player = null;
                    DuelSpawns = new Vector2[2];
                    return;
                }

                if (Team1PlayerScore == C3Mod.C3Config.DuelScoreLimit)
                {
                    DuelRunning = false;
                    C3Tools.BroadcastMessageToGametype("1v1", Team1Player.PlayerName + ": WINS!", Color.DarkCyan);
                    C3Tools.BroadcastMessageToGametype("", Team1Player.PlayerName + ": Beat " + Team2Player.PlayerName + " in a duel!", Color.DarkCyan);

                    List<C3Player> LostPlayers = new List<C3Player>();
                    List<C3Player> WonPlayers = new List<C3Player>();
                    WonPlayers.Add(Team1Player);
                    LostPlayers.Add(Team2Player);

                    C3Events.GameEnd(WonPlayers, LostPlayers, "1v1", Team1Player.FFAScore, Team2Player.FFAScore);

                    TpToSpawns(false);
                    C3Tools.ResetGameType("1v1");
                    Team1Player = null;
                    Team2Player = null;
                    DuelSpawns = new Vector2[2];
                    return;                    
                }
            }
        }        

        public static int TpToSpawnPoint()
        {
            try
            {
                bool Team1Player = false;
                bool Team2Player = false;

                for (int i = 0; i < C3Mod.C3Players.Count; i++)
                {
                    if ((C3Mod.C3Players[i].Team == 3 && Main.player[C3Mod.C3Players[i].Index].team != C3Mod.C3Config.TeamColor1))
                        TShock.Players[C3Mod.C3Players[i].Index].SetTeam(C3Mod.C3Config.TeamColor1);
                    else if (C3Mod.C3Players[i].Team == 4 && Main.player[C3Mod.C3Players[i].Index].team != C3Mod.C3Config.TeamColor2)
                        TShock.Players[C3Mod.C3Players[i].Index].SetTeam(C3Mod.C3Config.TeamColor2);

                    if (C3Mod.C3Players[i].Team == 3)
                    {
                        Team1Player = true;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].TSPlayer.X / 16 != DuelSpawns[0].X || C3Mod.C3Players[i].TSPlayer.Y / 16 + 3 != DuelSpawns[0].Y)
                        {
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)DuelSpawns[0].X, (int)DuelSpawns[0].Y);
                        }
                        if (C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                    else if (C3Mod.C3Players[i].Team == 4)
                    {
                        Team2Player = true;
                        C3Mod.C3Players[i].TSPlayer.TpLock = false;
                        if (C3Mod.C3Players[i].TSPlayer.X / 16 != DuelSpawns[1].X || C3Mod.C3Players[i].TSPlayer.Y / 16 + 3 != DuelSpawns[1].Y)
                        {
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)DuelSpawns[1].X, (int)DuelSpawns[1].Y);
                        }
                        if (C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                }

                if (!Team1Player || !Team2Player)
                {
                    C3Tools.BroadcastMessageToGametype("1v1", "Opponent left, ending game", Color.DarkCyan);
                    DuelRunning = false;
                    DuelCountdown = false;
                    DuelSpawns = new Vector2[2];
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
                if (C3Mod.C3Players[i].Team == 3)
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                }
                if (C3Mod.C3Players[i].Team == 4)
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

    internal class DuelArena
    {
        public string Name = "";
        public Vector2[] Spawns = new Vector2[2];

        public DuelArena(Vector2 redspawn, Vector2 bluespawn, string name)
        {
            Spawns[0] = redspawn;
            Spawns[1] = bluespawn;
            Name = name;
        }
    }
}