using System;
using System.Collections.Generic;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace C3Mod.GameTypes
{
    internal class Apocalypse
    {
        public static bool Running = false;
        public static bool Intermission = false;
        public static int Wave = 1;
        public static int VoteCount = 0;
        public static DateTime countDownTick = DateTime.UtcNow;
        public static DateTime voteCountDown = DateTime.UtcNow;
        public static DateTime scoreNotify = DateTime.UtcNow;
        public static DateTime WaveEndTime = DateTime.UtcNow;
        public static int MonstersLeft = 0;
        public static NPC CurMonster = null;
        public static Vector2 SpectatorArea = new Vector2();
        public static Vector2 PlayerSpawn = new Vector2();
        public static Vector2 MonsterSpawn = new Vector2();
        public static List<NPC> MonsterWhoAmI = new List<NPC>();
        public static int StartCount = 3;
        public static int LastMonster = 0;
        public static int playersdead = 0;

		public static void OnUpdate(EventArgs args)
        {
            if (C3Mod.VoteRunning && C3Mod.VoteType == "apoc")
            {
                int VotedPlayers = 0;
                int TotalPlayers = 0;

                lock (C3Mod.C3Players)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "" || player.GameType == "apoc")
                            TotalPlayers++;
                        if (player.GameType == "apoc")
                            VotedPlayers++;
                    }
                }

                if (VotedPlayers == TotalPlayers)
                {
                    C3Tools.BroadcastMessageToGametype("apoc", "Vote to play Monster Apocalypse passed, Teleporting to start positions", Color.DarkCyan);
                    bool[] playersDead = new bool[Main.maxNetPlayers];
                    TpToSpawnPoint();
                    countDownTick = DateTime.UtcNow;
                    Intermission = true;
                    C3Mod.VoteType = "";
                    C3Mod.VoteRunning = false;
                    ChooseNPC();
                    return;
                }

                double tick = (DateTime.UtcNow - voteCountDown).TotalMilliseconds;
                if (tick > (C3Mod.C3Config.VoteNotifyInterval * 1000) && VoteCount > 0)
                {
                    if (VoteCount != 1 && VoteCount < (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval))
                    {
                        C3Tools.BroadcastMessageToGametype("apoc", "Vote still in progress, please be patient", Color.Cyan);
                        C3Tools.BroadcastMessageToGametype("", "Vote to play Monster Apocalypse in progress, type /join to join the lobby", Color.Cyan);
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
                        if (player.GameType == "apoc")
                            players++;
                    }

                    if (players >= C3Mod.C3Config.MonsterApocalypseMinimumPlayers)
                    {
                        C3Tools.BroadcastMessageToGametype("apoc", "Vote to play Monster Apocalypse passed, Teleporting to start positions", Color.DarkCyan);
                        bool[] playersDead = new bool[Main.maxNetPlayers];
                        TpToSpawnPoint();
                        countDownTick = DateTime.UtcNow;
                        Intermission = true;
                        C3Mod.VoteType = "";
                        C3Mod.VoteRunning = false;
                        ChooseNPC();
                        return;
                    }
                    else
                        C3Tools.BroadcastMessageToGametype("apoc", "Vote to play Monster Apocalypse failed, Not enough players", Color.DarkCyan);
                }
            }

            if (Intermission)
            {
                double tick = (DateTime.UtcNow - countDownTick).TotalMilliseconds;
                if (tick > 5000 && StartCount > -1)
                {
                    if (TpToSpawnPoint() > 0)
                    {
                        if (StartCount == 0)
                        {
                            C3Tools.BroadcastMessageToGametype("apoc", "Fight!!!", Color.Cyan);
                            StartCount = 3;
                            Intermission = false;
                            Running = true;
                            SpawnMonsters();
                            playersdead = 0;
                            foreach (C3Player player in C3Mod.C3Players)
                            {
                                player.Dead = false;
                                player.LivesUsed = 0;
                            }
                        }
                        else
                        {
                            Random r = new Random();

                            switch (r.Next(20) + 1)
                            {
                                case 1:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Apocalypse in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 2:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "World's demise in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 3:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "You are dead in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 4:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Goodbye. Wave starting in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 5:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Are you ready? No. Wave starting in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 6:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "DOOOM! Wave starting in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 7:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Once upon a time.. You died. Wave starting in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 8:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "The year 2012 in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 9:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Space Ship's Leaving in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 10:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Rawr! Wave starting in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 11:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Twitchy is awesome! Wave starting in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 12:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Umm... " + (StartCount * 5).ToString() + " till...death?", Color.Cyan);
                                        break;
                                    }
                                case 13:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "It hurts ok? Dont believe otherwise! Pain in: " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 14:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Fun in: " + (StartCount * 5).ToString() + ".... Oh wait. Nvm. You'll find out.", Color.Cyan);
                                        break;
                                    }
                                case 15:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "MWAHAHAHA. Oh, hi there. Ignore that... " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 16:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "The Twitchy god spites you! " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 17:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Peanut butter. Best cure for all cuts and bruises " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 19:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Hehehe... " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                                case 20:
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "C3 Apocalypse. Leading the way since 2010! " + (StartCount * 5).ToString() + "...", Color.Cyan);
                                        break;
                                    }
                            }
                            C3Tools.BroadcastMessageToGametype("apoc", "Next Monster: " + CurMonster.name, Color.Cyan);
                            countDownTick = DateTime.UtcNow;
                            StartCount--;
                        }
                    }
                    else
                    {
                        StartCount = 3;
                        C3Tools.ResetGameType("apoc");
                        return;
                    }
                }
            }

            if (Running)
            {
                double tick = (DateTime.UtcNow - scoreNotify).TotalMilliseconds;
                if (tick > (C3Mod.C3Config.MonsterApocalypseScoreNotifyInterval * 1000))
                {
                    C3Tools.BroadcastMessageToGametype("apoc", CurMonster.name + "'s Left: " + MonstersLeft.ToString(), Color.Cyan);
                    scoreNotify = DateTime.UtcNow;
                }

                if (MonstersLeft == 0)
                {
                    Running = false;
                    Intermission = true;
                    ChooseNPC();
                    Wave++;
                    List<C3Player> AlivePlayers = new List<C3Player>();
                    List<C3Player> SpecatingPlayers = new List<C3Player>();

                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "apoc")
                        {
                            if (player.Spectator)
                                SpecatingPlayers.Add(player);
                            else
                                AlivePlayers.Add(player);
                        }
                    }

                    C3Events.WaveAdvance(AlivePlayers, SpecatingPlayers, Wave);

                    return;
                }

                foreach (NPC npc in MonsterWhoAmI)
                {
                    if (!npc.active)
                    {
                        MonsterWhoAmI.Remove(npc);
                        MonstersLeft--;

                        if (MonstersLeft == 0)
                        {
                            Running = false;
                            Intermission = true;
                            Running = false;
                            Wave++;
                            ChooseNPC();
                            return;
                        }

                        break;
                    }
                }

                int apocplayers = 0;

                lock(C3Mod.C3Players)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "apoc")
                        {
                            apocplayers++;

                            player.TSPlayer.TPlayer.hostile = false;
                            NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", player.TSPlayer.Index);

                            if (player.TerrariaTeam != 3)
                                player.TSPlayer.SetTeam(3);

                            if (!player.TSPlayer.TpLock)
                                if (C3Mod.C3Config.TPLockEnabled) { player.TSPlayer.TpLock = true; }

                            if (Main.player[player.Index].dead)
                            {
                                player.LivesUsed++;
                                player.TSPlayer.TpLock = false;

                                if (player.LivesUsed >= C3Mod.C3Config.MonsterApocalypseLivesPerWave && !player.Dead)
                                {
                                    playersdead++;
                                    if (playersdead == apocplayers)
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", "Your team failed to survive the Apocalypse!", Color.Cyan);
                                        C3Tools.BroadcastMessageToGametype("", "Players on this server failed to survive the apocalypse!", Color.Cyan);
                                        TpToSpawns(false);
                                        C3Tools.ResetGameType("apoc");
                                        Running = false;
                                        Intermission = false;

                                        List<C3Player> Players = new List<C3Player>();

                                        foreach (C3Player player1 in C3Mod.C3Players)
                                        {
                                            if (player1.GameType == "apoc")
                                            {
                                                Players.Add(player);
                                            }
                                        }

                                        C3Events.GameEnd(new List<C3Player>(), Players, "apoc", 0, Wave);
                                    }
                                    else
                                    {
                                        C3Tools.BroadcastMessageToGametype("apoc", player.PlayerName + ": Is out!", Color.Cyan);
                                        player.TSPlayer.Teleport((int)SpectatorArea.X*16, (int)SpectatorArea.Y*16);
                                        player.Dead = true;
                                    }
                                }
                                else
                                {
                                    player.SendMessage("Lives left: " + (C3Mod.C3Config.MonsterApocalypseLivesPerWave - player.LivesUsed).ToString(), Color.Cyan);
                                    player.TSPlayer.Teleport((int)PlayerSpawn.X*16, (int)PlayerSpawn.Y*16);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void SpawnMonsters()
        {
            MonstersLeft = ((Wave/3) + 1) * 10;

            if (MonstersLeft >= 30)
            {
                MonstersLeft = 30;
            }

            if (CurMonster.boss)
            {
                var npc = new NPC();
                Random r = new Random();
                int amount = r.Next(3) + 1;

                for (int i = 0; i < amount; i++)
                {
                    int npcid = NPC.NewNPC((int)(MonsterSpawn.X * 16) - r.Next(-32, 32), (int)MonsterSpawn.Y * 16, CurMonster.type);
                    Main.npc[npcid].SetDefaults(CurMonster.name);
                    Main.npc[npcid].life = Main.npc[npcid].life * (Wave / 3) + 1;
                    MonsterWhoAmI.Add(Main.npc[npcid]);
                }
                MonstersLeft = amount;
            }
            else
            {
                Random r = new Random();
                int amount = r.Next(Wave * 10) + Wave * 10;
                
                for (int i = 0; i < MonstersLeft; i++)
                {
                    int npcid = NPC.NewNPC((int)(MonsterSpawn.X * 16) - r.Next(-48, 48), (int)MonsterSpawn.Y * 16, CurMonster.type);
                    Main.npc[npcid].SetDefaults(CurMonster.name);
                    Main.npc[npcid].life = Main.npc[npcid].life * (Wave / 3) + 1;
                    MonsterWhoAmI.Add(Main.npc[npcid]);
                }
            }
        }

        public static void ChooseNPC()
        {
            var npc = new NPC();
            Random r = new Random();
            int type = r.Next(ApocalypseMonsters.Monsters.Count);
            npc.SetDefaults(ApocalypseMonsters.Monsters[type]);
            CurMonster = npc;
        }

        public static int TpToSpawnPoint()
        {
            int apocplayers = 0;
            for (int i = 0; i < C3Mod.C3Players.Count; i++)
            {
                if (C3Mod.C3Players[i].GameType == "apoc")
                {
                    apocplayers++;
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    if (C3Mod.C3Players[i].tileX != (int)(PlayerSpawn.X) || C3Mod.C3Players[i].tileY != (int)(PlayerSpawn.Y - 3))
                    {
                        TShock.Players[C3Mod.C3Players[i].Index].Teleport((int)PlayerSpawn.X*16, (int)PlayerSpawn.Y*16);
                    }
                }
            }
            if (apocplayers < C3Mod.C3Config.MonsterApocalypseMinimumPlayers)
            {
                C3Tools.BroadcastMessageToGametype("apoc", "Not enough players to continue, ending game", Color.DarkCyan);
                Running = false;
                Intermission = false;
                return 0;
            }
            return 1;
        }

        public static void TpToSpawns(bool pvpstate)
        {
            for (int i = 0; i < C3Mod.C3Players.Count; i++)
            {
                if (C3Mod.C3Players[i].GameType == "apoc")
                {
                    C3Mod.C3Players[i].TSPlayer.TpLock = false;
                    Main.player[C3Mod.C3Players[i].Index].hostile = pvpstate;
                    NetMessage.SendData(30, -1, -1, "", C3Mod.C3Players[i].Index, 0f, 0f, 0f);
                    TShock.Players[C3Mod.C3Players[i].Index].SetTeam(0);
                    TShock.Players[C3Mod.C3Players[i].Index].Spawn();
                }
            }

            Item heart = TShock.Utils.GetItemById(58);
            Item star = TShock.Utils.GetItemById(184);

            foreach (C3Player player in C3Mod.C3Players)
            {
                if (player.GameType == "apoc")
                {
                    player.GiveItem(heart.type, heart.name, heart.width, heart.height, 20);
                    player.GiveItem(star.type, star.name, star.width, star.height, 20);
                }
            }
        }

        public static void SpawnSet(int posX, int posY)
        {
            PlayerSpawn.X = (posX / 16);
            PlayerSpawn.Y = (posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("SpawnX", (int)(posX / 16)));
            values.Add(new SqlValue("SpawnY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("Apocalypse", values, new List<SqlValue>());
        }

        public static void MonsterSpawnSet(int posX, int posY)
        {
            MonsterSpawn.X = (posX / 16);
            MonsterSpawn.Y = (posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("MonsterSpawnX", (int)(posX / 16)));
            values.Add(new SqlValue("MonsterSpawnY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("Apocalypse", values, new List<SqlValue>());
        }

        public static void SpectatorSpawnSet(int posX, int posY)
        {
            SpectatorArea.X = (posX / 16);
            SpectatorArea.Y = (posY / 16) + 3;

            List<SqlValue> values = new List<SqlValue>();
            values.Add(new SqlValue("SpectatorSpawnX", (int)(posX / 16)));
            values.Add(new SqlValue("SpectatorSpawnY", (int)(posY / 16) + 3));
            C3Mod.SQLEditor.UpdateValues("Apocalypse", values, new List<SqlValue>());
        }
    }
}