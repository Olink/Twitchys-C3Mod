using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using Terraria;
using Hooks;
using TShockAPI;
using TShockAPI.DB;
using C3Mod.GameTypes;

namespace C3Mod
{
    internal class C3Commands
    {
        //Converted v2.2
        #region CTFCommands
        public static void SetCTFTeam1Flag(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempflags[0] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);

            if (C3Mod.C3Config.TeamColor1 == 1)
                args.Player.SendMessage("Temporary CTF Red flag set at your position", Color.OrangeRed);
            else if (C3Mod.C3Config.TeamColor1 == 2)
                args.Player.SendMessage("Temporary CTF Green flag set at your position", Color.LightGreen);
            else if (C3Mod.C3Config.TeamColor1 == 3)
                args.Player.SendMessage("Temporary CTF Blue flag set at your position", Color.LightBlue);
            else if (C3Mod.C3Config.TeamColor1 == 4)
                args.Player.SendMessage("Temporary CTF Yellow flag set at your position", Color.LightYellow);
        }

        public static void SetCTFTeam2Flag(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempflags[1] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);
            if (C3Mod.C3Config.TeamColor2 == 1)
                args.Player.SendMessage("Temporary CTF Red flag set at your position", Color.OrangeRed);
            else if (C3Mod.C3Config.TeamColor2 == 2)
                args.Player.SendMessage("Temporary CTF Green flag set at your position", Color.LightGreen);
            else if (C3Mod.C3Config.TeamColor2 == 3)
                args.Player.SendMessage("Temporary CTF Blue flag set at your position", Color.LightBlue);
            else if (C3Mod.C3Config.TeamColor2 == 4)
                args.Player.SendMessage("Temporary CTF Yellow flag set at your position", Color.LightYellow);
        }

        public static void SetCTFTeam1Spawn(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempspawns[0] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);

            if (C3Mod.C3Config.TeamColor1 == 1)
                args.Player.SendMessage("Temporary CTF Red spawn set at your position", Color.OrangeRed);
            else if (C3Mod.C3Config.TeamColor1 == 2)
                args.Player.SendMessage("Temporary CTF Green spawn set at your position", Color.LightGreen);
            else if (C3Mod.C3Config.TeamColor1 == 3)
                args.Player.SendMessage("Temporary CTF Blue spawn set at your position", Color.LightBlue);
            else if (C3Mod.C3Config.TeamColor1 == 4)
                args.Player.SendMessage("Temporary CTF Yellow spawn set at your position", Color.LightYellow);
        }

        public static void SetCTFTeam2Spawn(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempspawns[1] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);

            if (C3Mod.C3Config.TeamColor2 == 1)
                args.Player.SendMessage("Temporary CTF Red spawn set at your position", Color.OrangeRed);
            else if (C3Mod.C3Config.TeamColor2 == 2)
                args.Player.SendMessage("Temporary CTF Green spawn set at your position", Color.LightGreen);
            else if (C3Mod.C3Config.TeamColor2 == 3)
                args.Player.SendMessage("Temporary CTF Blue spawn set at your position", Color.LightBlue);
            else if (C3Mod.C3Config.TeamColor2 == 4)
                args.Player.SendMessage("Temporary CTF Yellow spawn set at your position", Color.LightYellow);
        }

        public static void AddCTFArena(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);

                if (player.tempflags[0] != new Vector2() && player.tempflags[1] != new Vector2() && player.tempspawns[0] != new Vector2() && player.tempspawns[1] != new Vector2())
                {
                    string text = "";

                    foreach (string word in args.Parameters)
                        text = text + word + " ";

                    List<SqlValue> list = new List<SqlValue>();
                    list.Add(new SqlValue("Name", "'" + text + "'"));
                    list.Add(new SqlValue("RedX", player.tempflags[0].X));
                    list.Add(new SqlValue("RedY", player.tempflags[0].Y));
                    list.Add(new SqlValue("BlueX", player.tempflags[1].X));
                    list.Add(new SqlValue("BlueY", player.tempflags[1].Y));
                    list.Add(new SqlValue("RedSpawnX", player.tempspawns[0].X));
                    list.Add(new SqlValue("RedSpawnY", player.tempspawns[0].Y));
                    list.Add(new SqlValue("BlueSpawnX", player.tempspawns[1].X));
                    list.Add(new SqlValue("BlueSpawnY", player.tempspawns[1].Y));
                    C3Mod.SQLEditor.InsertValues("FlagPoints", list);

                    CTF.Arenas.Add(
                        new CTFArena(
                        new Vector2(player.tempflags[0].X, player.tempflags[0].Y),
                        new Vector2(player.tempflags[1].X, player.tempflags[1].Y),
                        new Vector2(player.tempspawns[0].X, player.tempspawns[0].Y),
                        new Vector2(player.tempspawns[1].X, player.tempspawns[1].Y),
                        text));

                    player.tempspawns = new Vector2[2];
                    player.tempflags = new Vector2[2];

                    args.Player.SendMessage("Added CTF arena: " + text + ", ID: " + CTF.Arenas.Count, Color.Yellow);
                }
                else
                    args.Player.SendMessage("Please set up both Flags and Spawns", Color.Red);
            }
            else
                args.Player.SendMessage("Please provide a arena name", Color.Red);
        }
        #endregion
        //Converted v2.2
        #region OneFlagCommands

        public static void SetOneFlag(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempflags[0] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);
            args.Player.SendMessage("Temporary One Flag set at your position", Color.Cyan);
        }

        public static void SetOneFlagTeam1Spawn(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempspawns[0] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);
            if (C3Mod.C3Config.TeamColor1 == 1)
                args.Player.SendMessage("Temporary One Flag CTF Red spawn set at your position", Color.OrangeRed);
            else if (C3Mod.C3Config.TeamColor1 == 2)
                args.Player.SendMessage("Temporary One Flag CTF Green spawn set at your position", Color.LightGreen);
            else if (C3Mod.C3Config.TeamColor1 == 3)
                args.Player.SendMessage("Temporary One Flag CTF Blue spawn set at your position", Color.LightBlue);
            else if (C3Mod.C3Config.TeamColor1 == 4)
                args.Player.SendMessage("Temporary One Flag CTF Yellow spawn set at your position", Color.LightYellow);
        }

        public static void SetOneFlagTeam2Spawn(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempspawns[1] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);
            if (C3Mod.C3Config.TeamColor2 == 1)
                args.Player.SendMessage("Temporary One Flag CTF Red spawn set at your position", Color.OrangeRed);
            else if (C3Mod.C3Config.TeamColor2 == 2)
                args.Player.SendMessage("Temporary One Flag CTF Green spawn set at your position", Color.LightGreen);
            else if (C3Mod.C3Config.TeamColor2 == 3)
                args.Player.SendMessage("Temporary One Flag CTF Blue spawn set at your position", Color.LightBlue);
            else if (C3Mod.C3Config.TeamColor2 == 4)
                args.Player.SendMessage("Temporary One Flag  CTF Yellow spawn set at your position", Color.LightYellow);
        }

        public static void AddOneFlagArena(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);

                if (player.tempflags[0] != new Vector2() && player.tempspawns[0] != new Vector2() && player.tempspawns[1] != new Vector2()) 
                {
                    string text = "";

                    foreach (string word in args.Parameters)
                        text = text + word + " ";

                    List<SqlValue> list = new List<SqlValue>();
                    list.Add(new SqlValue("Name", "'" + text + "'"));
                    list.Add(new SqlValue("FlagX", player.tempflags[0].X));
                    list.Add(new SqlValue("FlagY", player.tempflags[0].Y));
                    list.Add(new SqlValue("RedSpawnX", player.tempspawns[0].X));
                    list.Add(new SqlValue("RedSpawnY", player.tempspawns[0].Y));
                    list.Add(new SqlValue("BlueSpawnX", player.tempspawns[1].X));
                    list.Add(new SqlValue("BlueSpawnY", player.tempspawns[1].Y));
                    C3Mod.SQLEditor.InsertValues("OneFlagPoints", list);

                    OneFlagCTF.Arenas.Add(
                        new OneFlagArena(
                        new Vector2(player.tempflags[0].X, player.tempflags[0].Y),
                        new Vector2(player.tempspawns[0].X, player.tempspawns[0].Y),
                        new Vector2(player.tempspawns[1].X, player.tempspawns[1].Y),
                        text));

                    player.tempspawns = new Vector2[2];
                    player.tempflags = new Vector2[2];

                    args.Player.SendMessage("Added One Flag arena: " + text + ", ID: " + OneFlagCTF.Arenas.Count, Color.Yellow);
                }
                else
                    args.Player.SendMessage("Please set up both Flag Vector2 and Spawns", Color.Red);
            }
            else
                args.Player.SendMessage("Please provide a arena name", Color.Red);
        }

        #endregion
        //Converted v2.2
        #region DuelCommands
        public static void SetDuelTeam1Spawn(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempspawns[0] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);
            if (C3Mod.C3Config.TeamColor1 == 1)
                args.Player.SendMessage("Temporary Duel Red spawn set at your position", Color.OrangeRed);
            else if (C3Mod.C3Config.TeamColor1 == 2)
                args.Player.SendMessage("Temporary Duel Green spawn set at your position", Color.LightGreen);
            else if (C3Mod.C3Config.TeamColor1 == 3)
                args.Player.SendMessage("Temporary Duel Blue spawn set at your position", Color.LightBlue);
            else if (C3Mod.C3Config.TeamColor1 == 4)
                args.Player.SendMessage("Temporary Duel Yellow spawn set at your position", Color.LightYellow);
        }

        public static void SetDuelTeam2Spawn(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempspawns[1] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);
            if (C3Mod.C3Config.TeamColor2 == 1)
                args.Player.SendMessage("Temporary Duel Red spawn set at your position", Color.OrangeRed);
            else if (C3Mod.C3Config.TeamColor2 == 2)
                args.Player.SendMessage("Temporary Duel Green spawn set at your position", Color.LightGreen);
            else if (C3Mod.C3Config.TeamColor2 == 3)
                args.Player.SendMessage("Temporary Duel Blue spawn set at your position", Color.LightBlue);
            else if (C3Mod.C3Config.TeamColor2 == 4)
                args.Player.SendMessage("Temporary Duel Yellow spawn set at your position", Color.LightYellow);
        }

        public static void AddDuelArena(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);

                if (player.tempspawns[0] != new Vector2() && player.tempspawns[1] != new Vector2())
                {
                    string text = "";

                    foreach (string word in args.Parameters)
                        text = text + word + " ";

                    List<SqlValue> list = new List<SqlValue>();
                    list.Add(new SqlValue("Name", "'" + text + "'"));
                    list.Add(new SqlValue("RedSpawnX", player.tempspawns[0].X));
                    list.Add(new SqlValue("RedSpawnY", player.tempspawns[0].Y));
                    list.Add(new SqlValue("BlueSpawnX", player.tempspawns[1].X));
                    list.Add(new SqlValue("BlueSpawnY", player.tempspawns[1].Y));
                    C3Mod.SQLEditor.InsertValues("DuelSpawns", list);

                    Duel.Arenas.Add(
                        new DuelArena(
                        new Vector2(player.tempspawns[0].X, player.tempspawns[0].Y),
                        new Vector2(player.tempspawns[1].X, player.tempspawns[1].Y),
                        text));

                    player.tempspawns = new Vector2[2];
                    player.tempflags = new Vector2[2];

                    args.Player.SendMessage("Added Duel arena: " + text + ", ID: " + Duel.Arenas.Count, Color.Yellow);
                }
                else
                    args.Player.SendMessage("Please set up Spawns", Color.Red);
            }
            else
                args.Player.SendMessage("Please provide a arena name", Color.Red);
        }

        #endregion
        //Converted v2.2
        #region TDMCommands
        public static void SetTDMTeam1Spawn(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempspawns[0] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);
            if (C3Mod.C3Config.TeamColor1 == 1)
                args.Player.SendMessage("Temporary TDM Red spawn set at your position", Color.OrangeRed);
            else if (C3Mod.C3Config.TeamColor1 == 2)
                args.Player.SendMessage("Temporary TDM Green spawn set at your position", Color.LightGreen);
            else if (C3Mod.C3Config.TeamColor1 == 3)
                args.Player.SendMessage("Temporary TDM Blue spawn set at your position", Color.LightBlue);
            else if (C3Mod.C3Config.TeamColor1 == 4)
                args.Player.SendMessage("Temporary TDM Yellow spawn set at your position", Color.LightYellow);
        }

        public static void SetTDMTeam2Spawn(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempspawns[1] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);
            if (C3Mod.C3Config.TeamColor2 == 1)
                args.Player.SendMessage("Temporary TDM Red spawn set at your position", Color.OrangeRed);
            else if (C3Mod.C3Config.TeamColor2 == 2)
                args.Player.SendMessage("Temporary TDM Green spawn set at your position", Color.LightGreen);
            else if (C3Mod.C3Config.TeamColor2 == 3)
                args.Player.SendMessage("Temporary TDM Blue spawn set at your position", Color.LightBlue);
            else if (C3Mod.C3Config.TeamColor2 == 4)
                args.Player.SendMessage("Temporary TDM Yellow spawn set at your position", Color.LightYellow);
        }

        public static void AddTDMArena(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);

                if (player.tempspawns[0] != new Vector2() && player.tempspawns[1] != new Vector2())
                {
                    string text = "";

                    foreach (string word in args.Parameters)
                        text = text + word + " ";

                    List<SqlValue> list = new List<SqlValue>();
                    list.Add(new SqlValue("Name", "'" + text + "'"));
                    list.Add(new SqlValue("RedSpawnX", player.tempspawns[0].X));
                    list.Add(new SqlValue("RedSpawnY", player.tempspawns[0].Y));
                    list.Add(new SqlValue("BlueSpawnX", player.tempspawns[1].X));
                    list.Add(new SqlValue("BlueSpawnY", player.tempspawns[1].Y));
                    C3Mod.SQLEditor.InsertValues("TDMSpawns", list);

                    TDM.Arenas.Add(
                        new TDMArena(
                        new Vector2(player.tempspawns[0].X, player.tempspawns[0].Y),
                        new Vector2(player.tempspawns[1].X, player.tempspawns[1].Y),
                        text));

                    player.tempspawns = new Vector2[2];
                    player.tempflags = new Vector2[2];

                    args.Player.SendMessage("Added TDM arena: " + text + ", ID: " + TDM.Arenas.Count, Color.Yellow);
                }
                else
                    args.Player.SendMessage("Please set up Spawns", Color.Red);
            }
            else
                args.Player.SendMessage("Please provide a arena name", Color.Red);
        }

        #endregion
        //Converted v2.2
        #region FFACommands
        public static void SetFFASpawn(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);
            player.tempspawns[0] = new Vector2((int)args.Player.X / 16, (int)args.Player.Y / 16 + 3);
            args.Player.SendMessage("Temporary FFA spawn set at your position", Color.OrangeRed);
        }

        public static void AddFFAArena(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);

                if (player.tempspawns[0] != new Vector2())
                {
                    string text = "";

                    foreach (string word in args.Parameters)
                        text = text + word + " ";

                    List<SqlValue> list = new List<SqlValue>();
                    list.Add(new SqlValue("Name", "'" + text + "'"));
                    list.Add(new SqlValue("SpawnX", player.tempspawns[0].X));
                    list.Add(new SqlValue("SpawnY", player.tempspawns[0].Y));
                    C3Mod.SQLEditor.InsertValues("FFASpawns", list);

                    FFA.Arenas.Add(
                        new FFAArena(
                        new Vector2(player.tempspawns[0].X, player.tempspawns[0].Y),
                        text));

                    player.tempspawns = new Vector2[2];
                    player.tempflags = new Vector2[2];

                    args.Player.SendMessage("Added FFA arena: " + text + ", ID: " + FFA.Arenas.Count, Color.Yellow);
                }
                else
                    args.Player.SendMessage("Please set up Spawn", Color.Red);
            }
            else
                args.Player.SendMessage("Please provide a arena name", Color.Red);
        }

        #endregion
        //Converted v2.2
        #region Apocalypse
        public static void SetApocPlayerSpawn(CommandArgs args)
        {
            Apocalypse.SpawnSet((int)args.Player.X, (int)args.Player.Y);
            args.Player.SendMessage("Player spawn set at your position", Color.OrangeRed);
        }

        public static void SetApocMonsterSpawn(CommandArgs args)
        {
            Apocalypse.MonsterSpawnSet((int)args.Player.X, (int)args.Player.Y);
            args.Player.SendMessage("Monster spawn set at your position", Color.OrangeRed);
        }

        public static void SetApocSpectatorSpawn(CommandArgs args)
        {
            Apocalypse.SpectatorSpawnSet((int)args.Player.X, (int)args.Player.Y);
            args.Player.SendMessage("Spectator spawn set at your position", Color.OrangeRed);
        }

        #endregion
        //Converted v2.2
        #region Voting and Game Administration
        public static void Stop(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                switch (args.Parameters[0])
                {
                    case "ctf":
                        {
                            if (CTF.CTFGameRunning || CTF.CTFGameCountdown)
                            {
                                C3Tools.BroadcastMessageToGametype("ctf", "CTF stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("CTF Game Stopped", Color.DarkCyan);
                                CTF.TpToSpawns(false);
                                C3Tools.ResetGameType("ctf");
                                CTF.CTFGameRunning = false;
                                CTF.CTFGameCountdown = false;
                                CTF.flagPoints = new Vector2[2];
                                CTF.spawnPoints = new Vector2[2];
                            }
                            else
                            {
                                args.Player.SendMessage("CTF game not running", Color.DarkCyan);
                            }
                            break;
                        }
                    case "duel":
                        {
                            if (Duel.DuelRunning || Duel.DuelCountdown)
                            {
                                C3Tools.BroadcastMessageToGametype("1v1", "Duel stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("Duel Stopped", Color.DarkCyan);
                                Duel.TpToSpawns(false);
                                C3Tools.ResetGameType("1v1");
                                Duel.DuelRunning = false;
                                Duel.DuelCountdown = false;
                                Duel.DuelSpawns = new Vector2[2];
                            }
                            else
                            {
                                args.Player.SendMessage("Duel not running", Color.DarkCyan);
                            }
                            break;
                        }
                    case "oneflag":
                        {
                            if (OneFlagCTF.OneFlagGameCountdown || OneFlagCTF.OneFlagGameRunning)
                            {
                                C3Tools.BroadcastMessageToGametype("oneflag", "One flag stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("One Flag Stopped", Color.DarkCyan);
                                OneFlagCTF.SendToSpawn(false);
                                C3Tools.ResetGameType("oneflag");
                                OneFlagCTF.OneFlagGameRunning = false;
                                OneFlagCTF.OneFlagGameCountdown = false;
                                OneFlagCTF.FlagPoint = new Vector2();
                                OneFlagCTF.SpawnPoint = new Vector2[2];
                            }
                            else
                            {
                                args.Player.SendMessage("One Flag not running", Color.DarkCyan);
                            }
                            break;
                        }
                    case "apocalypse":
                        {
                            if (Apocalypse.Intermission || Apocalypse.Running)
                            {
                                C3Tools.BroadcastMessageToGametype("apoc", "Apocalypse stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("Apocalypse Stopped", Color.DarkCyan);
                                Apocalypse.TpToSpawns(false);
                                C3Tools.ResetGameType("apoc");
                                Apocalypse.Running = false;
                                Apocalypse.Intermission = false;
                            }
                            else
                            {
                                args.Player.SendMessage("Apocalypse not running", Color.DarkCyan);
                            }
                            break;
                        }
                    case "tdm":
                        {
                            if (TDM.TDMCountdown || TDM.TDMRunning)
                            {
                                C3Tools.BroadcastMessageToGametype("tdm", "Team Deathmatch stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("Team Deathmatch Stopped", Color.DarkCyan);
                                TDM.TpToSpawns(false);
                                C3Tools.ResetGameType("tdm");
                                TDM.TDMRunning = false;
                                TDM.TDMCountdown = false;
                            }
                            else
                            {
                                args.Player.SendMessage("Team Deathmatch not running", Color.DarkCyan);
                            }
                            break;
                        }
                    case "ffa":
                        {
                            if (FFA.FFACountdown || FFA.FFARunning)
                            {
                                C3Tools.BroadcastMessageToGametype("ffa", "Free For All stopped by admin", Color.LightBlue);
                                args.Player.SendMessage("Free For All Stopped", Color.DarkCyan);
                                FFA.TpToSpawns(false);
                                C3Tools.ResetGameType("ffa");
                                FFA.FFARunning = false;
                                FFA.FFACountdown = false;
                            }
                            else
                            {
                                args.Player.SendMessage("Free For All not running", Color.DarkCyan);
                            }
                            break;
                        }
                }
            }
            else
                args.Player.SendMessage("Please enter a gametype", Color.DarkCyan);
        }

        public static void StartVote(CommandArgs args)
        {
            if (!C3Mod.VoteRunning)
            {
                int TotalAvialPlayers = 0;

                foreach (C3Player player in C3Mod.C3Players)
                {
                    if (player.GameType == "")
                        TotalAvialPlayers++;
                }

                if (TotalAvialPlayers > 1)
                {
                    if (args.Parameters.Count > 0)
                    {
                        int arena = 1;
                        if (args.Parameters.Count > 1)
                            Int32.TryParse(args.Parameters[1], out arena);

                        //Gametype Divider
                        ///CTF
                        if (args.Parameters[0].ToLower() == "ctf")
                        {
                            if (C3Mod.C3Config.CTFEnabled)
                            {
                                if (CTF.Arenas.Count > 0 && CTF.Arenas.Count >= arena)
                                {
                                    if (!CTF.CTFGameRunning && !CTF.CTFGameCountdown)
                                    {
                                        CTF.flagPoints = CTF.Arenas[arena - 1].Flags;
                                        CTF.spawnPoints = CTF.Arenas[arena - 1].Spawns;
                                        C3Tools.BroadcastMessageToGametype("", "Vote to play Capture the Flag started by: " + args.Player.Name, Color.Cyan);
                                        C3Tools.BroadcastMessageToGametype("", "Type /join to join the lobby for this game! Arena: " + CTF.Arenas[arena - 1].Name, Color.Cyan);
                                        CTF.Team2Score = 0;
                                        CTF.Team1Score = 0;
                                        CTF.Team1FlagCarrier = null;
                                        CTF.Team2FlagCarrier = null;
                                        C3Mod.VoteType = "ctf";
                                        CTF.VoteCount = (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval);
                                        C3Mod.VoteRunning = true;
                                    }
                                    else
                                        args.Player.SendMessage("Capture Flag game already running!", Color.DarkCyan);
                                }
                                else
                                    args.Player.SendMessage("Arena does not exist!", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("Capture Flag disabled on this server", Color.DarkCyan);
                        }
                        //Gametype Divider
                        ///One Flag
                        else if (args.Parameters[0].ToLower() == "oneflag")
                        {
                            if (C3Mod.C3Config.OneFlagEnabled)
                            {
                                if (OneFlagCTF.Arenas.Count > 0 && OneFlagCTF.Arenas.Count >= arena)
                                {
                                    if (!OneFlagCTF.OneFlagGameRunning && !OneFlagCTF.OneFlagGameCountdown)
                                    {
                                        OneFlagCTF.FlagPoint = OneFlagCTF.Arenas[arena - 1].Flag;
                                        OneFlagCTF.SpawnPoint = OneFlagCTF.Arenas[arena - 1].Spawns;
                                        C3Tools.BroadcastMessageToGametype("", "Vote to play One Flag CTF started by: " + args.Player.Name, Color.Cyan);
                                        C3Tools.BroadcastMessageToGametype("", "Type /join to join the lobby for this game! Arena: " + OneFlagCTF.Arenas[arena - 1].Name, Color.Cyan);
                                        OneFlagCTF.Team2Score = 0;
                                        OneFlagCTF.Team1Score = 0;
                                        OneFlagCTF.FlagCarrier = null;
                                        C3Mod.VoteType = "oneflag";
                                        OneFlagCTF.VoteCount = (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval);
                                        C3Mod.VoteRunning = true;
                                    }
                                    else
                                        args.Player.SendMessage("One Flag CTF game already running!", Color.DarkCyan);
                                }
                                else
                                    args.Player.SendMessage("Arena does not exist!", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("One Flag disabled on this server", Color.DarkCyan);
                        }
                        //Gametype Divider
                        ///TDM
                        else if (args.Parameters[0].ToLower() == "tdm")
                        {
                            if (C3Mod.C3Config.TeamDeathmatchEnabled)
                            {
                                if (TDM.Arenas.Count > 0 && TDM.Arenas.Count >= arena)
                                {
                                    if (!TDM.TDMRunning && !TDM.TDMCountdown)
                                    {
                                        TDM.TDMSpawns = TDM.Arenas[arena - 1].Spawns;
                                        C3Tools.BroadcastMessageToGametype("", "Vote to play Team Deathmatch started by: " + args.Player.Name, Color.Cyan);
                                        C3Tools.BroadcastMessageToGametype("", "Type /join to join the lobby for this game! Arena: " + TDM.Arenas[arena - 1].Name, Color.Cyan);
                                        TDM.Team2Score = 0;
                                        TDM.Team1Score = 0;
                                        C3Mod.VoteType = "tdm";
                                        TDM.VoteCount = (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval);
                                        C3Mod.VoteRunning = true;
                                    }
                                    else
                                        args.Player.SendMessage("Team Deathmatch already running!", Color.DarkCyan);
                                }
                                else
                                    args.Player.SendMessage("Arena does not exist!", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("Team Deathmatch disabled on this server", Color.DarkCyan);
                        }
                        ///Gametype Divider
                        ///Apocalypse
                        else if (args.Parameters[0].ToLower() == "apocalypse")
                        {
                            if (C3Mod.C3Config.MonsterApocalypseEnabled)
                            {
                                if (!Apocalypse.Running && !Apocalypse.Intermission)
                                {
                                    if (Apocalypse.SpectatorArea != Vector2.Zero && Apocalypse.PlayerSpawn != Vector2.Zero && Apocalypse.MonsterSpawn != Vector2.Zero)
                                    {
                                        C3Tools.BroadcastMessageToGametype("", "Vote to play Apocalypse started by: " + args.Player.Name, Color.Cyan);
                                        C3Tools.BroadcastMessageToGametype("", "Type /join to join the lobby for this game!", Color.Cyan);
                                        Apocalypse.VoteCount = (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval);
                                        C3Mod.VoteType = "apoc";
                                        Apocalypse.LastMonster = 0;
                                        Apocalypse.Wave = 0;
                                        C3Mod.VoteRunning = true;
                                    }
                                    else
                                        args.Player.SendMessage("Spawns not set up yet", Color.DarkCyan);
                                }
                                else
                                    args.Player.SendMessage("Apocalypse already running!", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("Apocalypse disabled on this server", Color.DarkCyan);
                        }
                        ///Gametype Divider
                        ///Free For All
                        else if (args.Parameters[0].ToLower() == "ffa")
                        {
                            if (C3Mod.C3Config.FreeForAllEnabled)
                            {
                                if (FFA.Arenas.Count > 0 && FFA.Arenas.Count >= arena && arena > 0)
                                {
                                    if (!FFA.FFARunning && !FFA.FFACountdown)
                                    {
                                        FFA.FFASpawn = FFA.Arenas[arena - 1].Spawn;
                                        C3Tools.BroadcastMessageToGametype("", "Vote to play Free For All started by: " + args.Player.Name, Color.Cyan);
                                        C3Tools.BroadcastMessageToGametype("", "Type /join to join the lobby for this game! Arena: " + FFA.Arenas[arena - 1].Name, Color.Cyan);
                                        foreach (C3Player player in C3Mod.C3Players)
                                        {
                                            player.FFAScore = 0;
                                        }
                                        C3Mod.VoteType = "ffa";
                                        FFA.VoteCount = (C3Mod.C3Config.VoteTime / C3Mod.C3Config.VoteNotifyInterval);
                                        C3Mod.VoteRunning = true;
                                    }
                                    else
                                        args.Player.SendMessage("Free For All already running!", Color.DarkCyan);
                                }
                                else
                                    args.Player.SendMessage("Arena does not exist!", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("Free For All disabled on this server", Color.DarkCyan);
                        }
                        else
                            args.Player.SendMessage("Not an available gametype", Color.DarkCyan);
                    }
                    else
                        args.Player.SendMessage("Incorrect format: /vote <gametype>", Color.OrangeRed);
                }
                else
                    args.Player.SendMessage("Not enough available players to make a vote!", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("Vote already running!", Color.DarkCyan);
        }

        public static void JoinVote(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);

            if (C3Mod.VoteRunning)
            {
                if (player.GameType == "")
                {
                    switch (C3Mod.VoteType)
                    {
                        case "ctf":
                            {
                                args.Player.SendMessage("You have joined the lobby for Capture the Flag", Color.Cyan);

                                string team = C3Tools.AssignTeam(C3Tools.GetC3PlayerByIndex(args.Player.Index), "ctf");

                                switch (team)
                                {
                                    case "Red":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Red team!", Color.OrangeRed);
                                            break;
                                        }
                                    case "Green":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Green team!", Color.LightGreen);
                                            break;
                                        }
                                    case "Blue":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Blue team!", Color.LightBlue);
                                            break;
                                        }
                                    case "Yellow":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Yellow team!", Color.LightYellow);
                                            break;
                                        }
                                }
                                break;
                            }
                        case "oneflag":
                            {
                                args.Player.SendMessage("You have joined the lobby for One Flag CTF", Color.Cyan);

                                string team = C3Tools.AssignTeam(C3Tools.GetC3PlayerByIndex(args.Player.Index), "oneflag");

                                switch (team)
                                {
                                    case "Red":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Red team!", Color.OrangeRed);
                                            break;
                                        }
                                    case "Green":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Green team!", Color.LightGreen);
                                            break;
                                        }
                                    case "Blue":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Blue team!", Color.LightBlue);
                                            break;
                                        }
                                    case "Yellow":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Yellow team!", Color.LightYellow);
                                            break;
                                        }
                                }
                                break;
                            }
                        case "tdm":
                            {
                                args.Player.SendMessage("You have joined the lobby for Team Deathmatch", Color.Cyan);

                                string team = C3Tools.AssignTeam(C3Tools.GetC3PlayerByIndex(args.Player.Index), "tdm");

                                switch (team)
                                {
                                    case "Red":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Red team!", Color.OrangeRed);
                                            break;
                                        }
                                    case "Green":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Green team!", Color.LightGreen);
                                            break;
                                        }
                                    case "Blue":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Blue team!", Color.LightBlue);
                                            break;
                                        }
                                    case "Yellow":
                                        {
                                            args.Player.SendMessage("You have been auto assigned to the Yellow team!", Color.LightYellow);
                                            break;
                                        }
                                }
                                break;
                            }
                        case "ffa":
                            {
                                args.Player.SendMessage("You have joined the lobby for Free For All", Color.Cyan);

                                C3Tools.GetC3PlayerByIndex(args.Player.Index).GameType = "ffa";
                                C3Tools.GetC3PlayerByIndex(args.Player.Index).Team = 9;
                                break;
                            }
                        case "apoc":
                            {
                                args.Player.SendMessage("You have joined the lobby for Apocalypse...", Color.Cyan);
                                C3Tools.GetC3PlayerByIndex(args.Player.Index).GameType = "apoc";
                                break;
                            }
                    }
                }
                else
                    args.Player.SendMessage("You are already ingame!", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("No vote running!", Color.DarkCyan);
        }

        public static void CancelVote(CommandArgs args)
        {
            if (C3Mod.VoteRunning)
            {
                C3Mod.VoteRunning = false;

                C3Tools.BroadcastMessageToGametype("", args.Player.Name + " stopped the current vote!", Color.DarkCyan);

                foreach (C3Player player in C3Mod.C3Players)
                {
                    if (player.GameType == C3Mod.VoteType)
                    {
                        player.GameType = "";
                        player.Team = 0;
                    }
                }

                C3Mod.VoteType = "";
            }
            else
                args.Player.SendMessage("No vote running!", Color.DarkCyan);
        }

        public static void Quit(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);

            if (player.GameType != "")
            {
                switch (player.GameType)
                {
                    case "apoc":
                        {
                            player.SendMessage("Quit the Apocalypse - Wuss.", Color.Cyan);
                            break;
                        }
                    case "ctf":
                        {
                            player.SendMessage("Quit Capture the Flag", Color.Cyan);
                            break;
                        }
                    case "oneflag":
                        {
                            player.SendMessage("Quit One Flag", Color.Cyan);
                            break;
                        }
                    case "1v1":
                        {
                            player.SendMessage("Quit your Duel", Color.Cyan);
                            break;
                        }
                    case "tdm":
                        {
                            player.SendMessage("Quit Team Deathmatch", Color.Cyan);
                            break;
                        }
                    case "ffa":
                        {
                            player.SendMessage("Quit Free For All", Color.Cyan);
                            break;
                        }
                }
                player.GameType = "";
                player.TSPlayer.Spawn();
                player.TSPlayer.SetTeam(0);
                player.TSPlayer.TPlayer.hostile = false;
                NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, "", player.TSPlayer.Index);
                player.LivesUsed = 0;
            }
            else
                player.SendMessage("You are not in a game!", Color.Cyan);
        }

        public static void ChallengePlayer(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);

            if (args.Parameters.Count > 0)
            {
                int arena = 1;
                string name = "";

                if (!Int32.TryParse(args.Parameters[0], out arena))
                {
                    arena = 1;
                    StringBuilder sb = new StringBuilder();
                    int count = 0;

                    foreach (string arg in args.Parameters)
                    {
                        if (count != args.Parameters.Count - 1)
                            sb.Append(arg + " ");
                        else
                            sb.Append(arg);

                        count++;
                    }

                    name = sb.ToString();
                }
                else
                {
                    arena = Int32.Parse(args.Parameters[0]);
                    name = args.Parameters[1];
                }

                if (C3Mod.C3Config.DuelsEnabled)
                {
                    if (Duel.Arenas.Count > 0 && Duel.Arenas.Count >= arena)
                    {
                        if (!Duel.DuelRunning && !Duel.DuelCountdown)
                        {
                            if (player.Challenging == null)
                            {
                                C3Player challenging;
                                if ((challenging = C3Tools.GetC3PlayerByName(name.ToLower())) != null)
                                {
                                    if (challenging.GameType == "")
                                    {
                                        player.Challenging = challenging;
                                        player.ChallengeNotifyCount = 5;
                                        player.Challenging.ChallengeArena = arena;
                                        args.Player.SendMessage("Challenge has been made to: " + challenging.PlayerName + ", Arena: " + Duel.Arenas[arena - 1].Name, Color.Cyan);
                                    }
                                    else
                                        args.Player.SendMessage(challenging.PlayerName + ": Is unavailable to be challenged at this time", Color.DarkCyan);
                                }
                                else
                                    args.Player.SendMessage("Could not find player with that name", Color.DarkCyan);
                            }
                            else
                                args.Player.SendMessage("You are already challenging someone!", Color.DarkCyan);
                        }
                        else
                            args.Player.SendMessage("A duel is already running. Try again later", Color.DarkCyan);
                    }
                    else
                        args.Player.SendMessage("Arena does not exist!", Color.DarkCyan);
                }
                else
                    args.Player.SendMessage("Dueling disabled on this server", Color.DarkCyan);
            }
        }

        public static void AcceptChallenge(CommandArgs args)
        {
            var player = C3Tools.GetC3PlayerByIndex(args.Player.Index);

            foreach (C3Player opponent in C3Mod.C3Players)
            {
                if (opponent.Challenging == player)
                {
                    if (!Duel.DuelRunning && !Duel.DuelCountdown)
                    {
                        Duel.Team1PlayerScore = 0;
                        Duel.Team2PlayerScore = 0;
                        Duel.DuelSpawns = Duel.Arenas[player.ChallengeArena - 1].Spawns;

                        Random r = new Random();
                        switch (r.Next(2) + 1)
                        {
                            case 1:
                                {
                                    Duel.Team1Player = player;
                                    Duel.Team2Player = opponent;
                                    player.Team = 3;
                                    player.GameType = "1v1";
                                    opponent.Team = 4;
                                    opponent.GameType = "1v1";
                                    opponent.Challenging = null;
                                    Duel.DuelCountdown = true;
                                    break;
                                }
                            case 2:
                                {
                                    Duel.Team1Player = opponent;
                                    Duel.Team2Player = player;
                                    player.Team = 4;
                                    player.GameType = "1v1";
                                    opponent.Team = 3;
                                    opponent.GameType = "1v1";
                                    opponent.Challenging = null;
                                    Duel.DuelCountdown = true;
                                    break;
                                }
                        }
                    }
                    else
                    {
                        opponent.SendMessage("A Dual is already running, challenge cancelled", Color.DarkCyan);
                        opponent.Challenging.SendMessage("A Dual is already running, challenge cancelled", Color.DarkCyan);
                        opponent.Challenging = null;
                    }
                }
            }
        }

        public static void ListArenaID(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                string arenatype = args.Parameters[0].ToLower();
                const int pagelimit = 15;
                const int perline = 5;
                int page = 0;

                if (args.Parameters.Count > 1)
                {
                    if (!int.TryParse(args.Parameters[1], out page) || page < 1)
                    {
                        args.Player.SendMessage(string.Format("Invalid page number ({0})", page), Color.Red);
                        return;
                    }
                    page--;
                }

                switch (arenatype)
                {
                    case "ctf":
                        {
                            var arenas = CTF.Arenas;
                            if (arenas.Count == 0)
                            {
                                args.Player.SendMessage("There are currently no arenas defined.", Color.Red);
                                return;
                            }
                            int pagecount = arenas.Count / pagelimit;
                            if (page > pagecount)
                            {
                                args.Player.SendMessage(string.Format("Page number exceeds pages ({0}/{1})", page + 1, pagecount + 1), Color.Red);
                                return;
                            }
                            args.Player.SendMessage(string.Format("Current Arenas ({0}/{1}):", page + 1, pagecount + 1), Color.Green);
                            var nameslist = new List<string>();
                            for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < arenas.Count; i++)
                            {
                                nameslist.Add(arenas[i].Name + ": " + (i + 1).ToString());
                            }
                            var names = nameslist.ToArray();
                            for (int i = 0; i < names.Length; i += perline)
                            {
                                args.Player.SendMessage(string.Join(", ", names, i, Math.Min(names.Length - i, perline)), Color.Yellow);
                            }
                            if (page < pagecount)
                            {
                                args.Player.SendMessage(string.Format("Type /list ctf {0} for more regions.", (page + 2)), Color.Yellow);
                            }
                            break;
                        }
                    case "oneflag":
                        {
                            var arenas = OneFlagCTF.Arenas;
                            if (arenas.Count == 0)
                            {
                                args.Player.SendMessage("There are currently no arenas defined.", Color.Red);
                                return;
                            }
                            int pagecount = arenas.Count / pagelimit;
                            if (page > pagecount)
                            {
                                args.Player.SendMessage(string.Format("Page number exceeds pages ({0}/{1})", page + 1, pagecount + 1), Color.Red);
                                return;
                            }
                            args.Player.SendMessage(string.Format("Current Arenas ({0}/{1}):", page + 1, pagecount + 1), Color.Green);
                            var nameslist = new List<string>();
                            for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < arenas.Count; i++)
                            {
                                nameslist.Add(arenas[i].Name + ": " + (i + 1).ToString());
                            }
                            var names = nameslist.ToArray();
                            for (int i = 0; i < names.Length; i += perline)
                            {
                                args.Player.SendMessage(string.Join(", ", names, i, Math.Min(names.Length - i, perline)), Color.Yellow);
                            }
                            if (page < pagecount)
                            {
                                args.Player.SendMessage(string.Format("Type /list oneflag {0} for more regions.", (page + 2)), Color.Yellow);
                            }
                            break;
                        }
                    case "ffa":
                        {
                            var arenas = FFA.Arenas;
                            if (arenas.Count == 0)
                            {
                                args.Player.SendMessage("There are currently no arenas defined.", Color.Red);
                                return;
                            }
                            int pagecount = arenas.Count / pagelimit;
                            if (page > pagecount)
                            {
                                args.Player.SendMessage(string.Format("Page number exceeds pages ({0}/{1})", page + 1, pagecount + 1), Color.Red);
                                return;
                            }
                            args.Player.SendMessage(string.Format("Current Arenas ({0}/{1}):", page + 1, pagecount + 1), Color.Green);
                            var nameslist = new List<string>();
                            for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < arenas.Count; i++)
                            {
                                nameslist.Add(arenas[i].Name + ": " + (i + 1).ToString());
                            }
                            var names = nameslist.ToArray();
                            for (int i = 0; i < names.Length; i += perline)
                            {
                                args.Player.SendMessage(string.Join(", ", names, i, Math.Min(names.Length - i, perline)), Color.Yellow);
                            }
                            if (page < pagecount)
                            {
                                args.Player.SendMessage(string.Format("Type /list ffa {0} for more regions.", (page + 2)), Color.Yellow);
                            }
                            break;
                        }
                    case "tdm":
                        {
                            var arenas = TDM.Arenas;
                            if (arenas.Count == 0)
                            {
                                args.Player.SendMessage("There are currently no arenas defined.", Color.Red);
                                return;
                            }
                            int pagecount = arenas.Count / pagelimit;
                            if (page > pagecount)
                            {
                                args.Player.SendMessage(string.Format("Page number exceeds pages ({0}/{1})", page + 1, pagecount + 1), Color.Red);
                                return;
                            }
                            args.Player.SendMessage(string.Format("Current Arenas ({0}/{1}):", page + 1, pagecount + 1), Color.Green);
                            var nameslist = new List<string>();
                            for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < arenas.Count; i++)
                            {
                                nameslist.Add(arenas[i].Name + ": " + (i + 1).ToString());
                            }
                            var names = nameslist.ToArray();
                            for (int i = 0; i < names.Length; i += perline)
                            {
                                args.Player.SendMessage(string.Join(", ", names, i, Math.Min(names.Length - i, perline)), Color.Yellow);
                            }
                            if (page < pagecount)
                            {
                                args.Player.SendMessage(string.Format("Type /list tdm {0} for more regions.", (page + 2)), Color.Yellow);
                            }
                            break;
                        }
                    case "duel":
                        {
                            var arenas = Duel.Arenas;
                            if (arenas.Count == 0)
                            {
                                args.Player.SendMessage("There are currently no arenas defined.", Color.Red);
                                return;
                            }
                            int pagecount = arenas.Count / pagelimit;
                            if (page > pagecount)
                            {
                                args.Player.SendMessage(string.Format("Page number exceeds pages ({0}/{1})", page + 1, pagecount + 1), Color.Red);
                                return;
                            }
                            args.Player.SendMessage(string.Format("Current Arenas ({0}/{1}):", page + 1, pagecount + 1), Color.Green);
                            var nameslist = new List<string>();
                            for (int i = (page * pagelimit); (i < ((page * pagelimit) + pagelimit)) && i < arenas.Count; i++)
                            {
                                nameslist.Add(arenas[i].Name + ": " + (i + 1).ToString());
                            }
                            var names = nameslist.ToArray();
                            for (int i = 0; i < names.Length; i += perline)
                            {
                                args.Player.SendMessage(string.Join(", ", names, i, Math.Min(names.Length - i, perline)), Color.Yellow);
                            }
                            if (page < pagecount)
                            {
                                args.Player.SendMessage(string.Format("Type /list duel {0} for more regions.", (page + 2)), Color.Yellow);
                            }
                            break;
                        }
                }
            }
        }

        #endregion
        //Converted v2.2
        #region Limits
        public static void SetCTFLimit(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (Int32.TryParse(args.Parameters[0], out C3Mod.C3Config.CTFScoreLimit))
                {
                    C3Mod.C3Config.Write(C3Tools.C3ConfigPath);
                    args.Player.SendMessage("Score limit set successfully!", Color.Cyan);
                }
                else
                    args.Player.SendMessage("Invalid number, please enter numbers only", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("Incorrect format: /setctflimit <limit>", Color.OrangeRed);
        }

        public static void SetOneFlagLimit(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (Int32.TryParse(args.Parameters[0], out C3Mod.C3Config.OneFlagScorelimit))
                {
                    C3Mod.C3Config.Write(C3Tools.C3ConfigPath);
                    args.Player.SendMessage("Score limit set successfully!", Color.Cyan);
                }
                else
                    args.Player.SendMessage("Invalid number, please enter numbers only", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("Incorrect format: /setoneflaglimit <limit>", Color.OrangeRed);
        }

        public static void SetDuelLimit(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (Int32.TryParse(args.Parameters[0], out C3Mod.C3Config.DuelScoreLimit))
                {
                    C3Mod.C3Config.Write(C3Tools.C3ConfigPath);
                    args.Player.SendMessage("Score limit set successfully!", Color.Cyan);
                }
                else
                    args.Player.SendMessage("Invalid number, please enter numbers only", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("Incorrect format: /setduellimit <limit>", Color.OrangeRed);
        }

        public static void SetTDMLimit(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (Int32.TryParse(args.Parameters[0], out C3Mod.C3Config.TeamDeathmatchScorelimit))
                {
                    C3Mod.C3Config.Write(C3Tools.C3ConfigPath);
                    args.Player.SendMessage("Score limit set successfully!", Color.Cyan);
                }
                else
                    args.Player.SendMessage("Invalid number, please enter numbers only", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("Incorrect format: /settdmlimit <limit>", Color.OrangeRed);
        }

        public static void SetFFALimit(CommandArgs args)
        {
            if (args.Parameters.Count > 0)
            {
                if (Int32.TryParse(args.Parameters[0], out C3Mod.C3Config.FFAScorelimit))
                {
                    C3Mod.C3Config.Write(C3Tools.C3ConfigPath);
                    args.Player.SendMessage("Score limit set successfully!", Color.Cyan);
                }
                else
                    args.Player.SendMessage("Invalid number, please enter numbers only", Color.DarkCyan);
            }
            else
                args.Player.SendMessage("Incorrect format: /setffalimit <limit>", Color.OrangeRed);
        }

        #endregion        
    }
}
