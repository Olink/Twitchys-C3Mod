using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace C3Mod.GameTypes
{
    internal class FFA
    {
        public static List<FFAArena> Arenas = new List<FFAArena>();
        public static bool FFARunning = false;
        public static bool FFACountdown = false;
        public static Vector2 FFASpawn = new Vector2();
        public static int StartCount = 5;
        public static int VoteCount = 0;
        public static DateTime countDownTick = DateTime.UtcNow;
        public static DateTime voteCountDown = DateTime.UtcNow;
        public static DateTime scoreNotify = DateTime.UtcNow;

        public static void OnUpdate()
        {
            lock (C3Mod.C3Players)
            {
                if (C3Mod.VoteRunning && C3Mod.VoteType == "ffa")
                {
                    int VotedPlayers = 0;
                    int TotalPlayers = 0;

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "" || player.GameType == "ffa")
                            TotalPlayers++;
                        if (player.GameType == "ffa")
                            VotedPlayers++;
                    }

                    if (VotedPlayers == TotalPlayers)
                    {
                        C3Tools.BroadcastMessageToGametype("ffa", "Vote to play Free For All passed, Teleporting to start position", Color.DarkCyan);
                        C3Mod.VoteRunning = false;
                        C3Mod.VoteType = "";
                        bool[] playersDead = new bool[Main.maxNetPlayers];
                        TpToSpawnPoint();
                        countDownTick = DateTime.UtcNow;
                        FFACountdown = true;
                        return;
                    }

                    double tick = (DateTime.UtcNow - voteCountDown).TotalMilliseconds;
                    if (tick > (C3Mod.C3Config.VoteNotifyInterval * 1000) && VoteCount > 0)
                    {
                        if (VoteCount != 1 && VoteCount < (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval))
                        {
                            C3Tools.BroadcastMessageToGametype("ffa", "Vote still in progress, please be patient", Color.Cyan);
                            C3Tools.BroadcastMessageToGametype("", "Vote to play Free For All in progress, type /join to join the lobby", Color.Cyan);
                        }

                        VoteCount--;
                        voteCountDown = DateTime.UtcNow;
                    }

                    else if (VoteCount == 0)
                    {
                        C3Mod.VoteRunning = false;

                        int players = 0;

                        foreach (C3Player player in C3Mod.C3Players)
                        {
                            if (player.GameType == "ffa")
                                players++;
                        }

                        if (players >= C3Mod.C3Config.VoteMinimumPerTeam)
                        {
                            C3Tools.BroadcastMessageToGametype("ffa", "Vote to play Free For All passed, Teleporting to start position", Color.DarkCyan);
                            bool[] playersDead = new bool[Main.maxNetPlayers];
                            TpToSpawnPoint();
                            countDownTick = DateTime.UtcNow;
                            FFACountdown = true;
                        }
                        else
                        {
                            C3Tools.BroadcastMessageToGametype("ffa", "Vote to play Free For All failed, Not enough players", Color.DarkCyan);
                            FFASpawn = new Vector2();
                        }
                    }
                }

                if (FFACountdown)
                {
                    double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
                    if (tick > 1000 && StartCount > -1)
                    {
                        if (TpToSpawnPoint() > 0)
                        {
                            if (StartCount == 0)
                            {
                                C3Tools.BroadcastMessageToGametype("ffa", "Fight!!!", Color.Cyan);
                                foreach (C3Player player in C3Mod.C3Players)
                                {
                                    if (player.GameType == "ffa")
                                    {
                                        player.SpawnProtectionEnabled = true;
                                        player.LastSpawn = DateTime.UtcNow;
                                    }
                                }
                                StartCount = 5;
                                FFACountdown = false;
                                FFARunning = true;
                            }
                            else
                            {
                                C3Tools.BroadcastMessageToGametype("ffa", "Game starting in " + StartCount.ToString() + "...", Color.Cyan);
                                countDownTick = DateTime.UtcNow;
                                StartCount--;
                            }
                        }
                        else
                        {
                            StartCount = 5;
                            C3Tools.ResetGameType("ffa");
                            return;
                        }
                    }
                }

                if (FFARunning)
                {
                    int players = 0;

                    double tick = (DateTime.UtcNow - scoreNotify).TotalMilliseconds;

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.TSPlayer == null)
                        {
                            C3Mod.C3Players.Remove(player);
                            break;
                        }

                        if (player.GameType == "ffa")
                        {
                            if (!player.TSPlayer.TpLock)
                                if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }

                            players++;

                            if (Main.player[player.Index].team != 0)
                                TShock.Players[player.Index].SetTeam(0);

                            if (!Main.player[player.Index].hostile)
                            {
                                Main.player[player.Index].hostile = true;
                                NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", player.Index, 0f, 0f, 0f);
                            }

                            double tick2 = (DateTime.UtcNow - player.LastSpawn).TotalMilliseconds;

                            if (player.SpawnProtectionEnabled && tick2 >= (C3Mod.C3Config.FFASpawnProtectionTime * 1000))
                            {
                                player.SpawnProtectionEnabled = false;
                            }

                            if (!Main.player[player.Index].dead && player.Dead)
                            {
                                player.LastSpawn = DateTime.UtcNow;
                                player.SpawnProtectionEnabled = true;
                                player.Dead = false;
                                player.TSPlayer.TpLock = false;
                                TShock.Players[player.Index].Teleport((int)FFASpawn.X, (int)FFASpawn.Y);
                                NetMessage.SendData(4, -1, player.Index, player.PlayerName, player.Index, 0f, 0f, 0f, 0);
                                if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }
                            }

                            if (player.FFAScore == C3Mod.C3Config.FFAScorelimit)
                            {
                                FFARunning = false;
                                C3Tools.BroadcastMessageToGametype("ffa", player.TSPlayer.Name + " WINS!", Color.LightBlue);

                                List<C3Player> LostPlayers = new List<C3Player>();
                                List<C3Player> Winner = new List<C3Player>();
                                Winner.Add(player);

                                foreach (C3Player player1 in C3Mod.C3Players)
                                {
                                    if (player1.GameType == "ffa")
                                    {
                                        if (player1 != player)
                                            LostPlayers.Add(player);
                                    }
                                }

                                C3Events.GameEnd(Winner, LostPlayers, "ffa", player.FFAScore, -1);

                                TpToSpawns(false);
                                C3Tools.ResetGameType("ffa");
                                TDM.TDMSpawns = new Vector2[2];
                                return;
                            }
                        }
                    }

                    if (players == 1)
                    {
                        C3Tools.BroadcastMessageToGametype("ffa", "Not enough players to continue, ending game", Color.DarkCyan);
                        FFARunning = false;
                        TpToSpawns(false);
                        C3Tools.ResetGameType("ffa");
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
                bool Player = false;

                for (int i = 0; i < C3Mod.C3Players.Count; i++)
                {
                    if ((C3Mod.C3Players[i].Team == 9 && Main.player[C3Mod.C3Players[i].Index].team != 0))
                        TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);

                    if (C3Mod.C3Players[i].Team == 9)
                    {
                        Player = true;

                        C3Mod.C3Players[i].TSPlayer.TpLock = false;

                        if (C3Mod.C3Players[i].TSPlayer.X / 16 != FFASpawn.X || C3Mod.C3Players[i].TSPlayer.Y / 16 + 3 != FFASpawn.Y)
                        {
                            TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)FFASpawn.X, (int)FFASpawn.Y);
                        }

                        if (C3Mod.C3Config.TPLockEnabled) { C3Mod.C3Players[i].TSPlayer.TpLock = true; }
                    }
                }
                if (!Player)
                {
                    C3Tools.BroadcastMessageToGametype("ffa", "Not enough players to continue, ending game", Color.DarkCyan);
                    FFARunning = false;
                    FFACountdown = false;
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
                if (C3Mod.C3Players[i].GameType == "ffa")
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

    internal class FFAArena
    {
        public string Name = "";
        public Vector2 Spawn = new Vector2();

        public FFAArena(Vector2 spawn, string name)
        {
            Spawn = spawn;
            Name = name;
        }
    }
}