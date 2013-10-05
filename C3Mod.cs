using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using C3Mod.GameTypes;
using System.IO;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;

namespace C3Mod
{
    [ApiVersion(1, 14)]
    public class C3Mod : TerrariaPlugin
    {
        public static C3ConfigFile C3Config { get; set; }
        public static bool VoteRunning { get; set; }
        public static string VoteType { get; set; }
        public static List<C3Player> C3Players = new List<C3Player>();
        internal static SqlTableEditor SQLEditor;
        internal static SqlTableCreator SQLWriter;
        internal static readonly Version VersionNum = Assembly.GetExecutingAssembly().GetName().Version;

        public override string Name
        {
            get { return "C3Mod"; }
        }
        public override string Author
        {
            get { return "Now Open Source... Donating helps all contributors!"; }
        }
        public override string Description
        {
            get { return "Now Open Source... Donating helps all contributors!"; }
        }
        public override Version Version
        {
            get { return VersionNum; }
        }

        public override void Initialize()
        {
            C3Tools.SetupConfig();

            if (C3Config.CTFEnabled)
                ServerApi.Hooks.GameUpdate.Register(this, CTF.OnUpdate);
            if (C3Config.DuelsEnabled)
				ServerApi.Hooks.GameUpdate.Register(this, Duel.OnUpdate);
            if (C3Config.OneFlagEnabled)
                ServerApi.Hooks.GameUpdate.Register(this, OneFlagCTF.OnUpdate);
            if (C3Config.TeamDeathmatchEnabled)
                ServerApi.Hooks.GameUpdate.Register(this, TDM.OnUpdate);
            if (C3Config.MonsterApocalypseEnabled)
                ServerApi.Hooks.GameUpdate.Register(this, Apocalypse.OnUpdate);
            if (C3Config.FreeForAllEnabled)
                ServerApi.Hooks.GameUpdate.Register(this, FFA.OnUpdate);

            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            ServerApi.Hooks.GameInitialize.Register(this, OnInitialize);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer);
			ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            ServerApi.Hooks.NetGetData.Register(this, GetData);

            GetDataHandlers.InitGetDataHandler();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
				if (C3Config.CTFEnabled)
					ServerApi.Hooks.GameUpdate.Deregister(this, CTF.OnUpdate);
				if (C3Config.DuelsEnabled)
					ServerApi.Hooks.GameUpdate.Deregister(this, Duel.OnUpdate);
				if (C3Config.OneFlagEnabled)
					ServerApi.Hooks.GameUpdate.Deregister(this, OneFlagCTF.OnUpdate);
				if (C3Config.TeamDeathmatchEnabled)
					ServerApi.Hooks.GameUpdate.Deregister(this, TDM.OnUpdate);
				if (C3Config.MonsterApocalypseEnabled)
					ServerApi.Hooks.GameUpdate.Deregister(this, Apocalypse.OnUpdate);
				if (C3Config.FreeForAllEnabled)
					ServerApi.Hooks.GameUpdate.Deregister(this, FFA.OnUpdate);

				ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
				ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
				ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
				ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
				ServerApi.Hooks.NetGetData.Deregister(this, GetData);
            }
            base.Dispose(disposing);
        }

        public C3Mod(Main game)
            : base(game)
        {
            C3Config = new C3ConfigFile();
            Order = -1;
        }

        internal void OnInitialize(EventArgs args)
        {
            if (C3Config.TeamColor1 < 1 || C3Config.TeamColor1 > 4 || C3Config.TeamColor1 == C3Config.TeamColor2 || C3Config.TeamColor2 > 4 || C3Config.TeamColor2 < 1)
                throw new Exception("Team Colours are inccorectly set up. Check c3config.json");

            SQLEditor = new SqlTableEditor(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());

            ApocalypseMonsters.AddNPCs();
            int ArenaCount;

            #region TestPerms
            //Checks to see if permissions have been moved around
            bool vote = false;
            bool joinvote = false;
            bool setflags = false;
            bool setspawns = false;
            bool managec3settings = false;
            bool cvote = false;
            bool duel = false;
            foreach (Group group in TShock.Groups.groups)
            {
                if (group.Name != "superadmin")
                {
                    if (group.HasPermission("vote"))
                        vote = true;
                    if (group.HasPermission("joinvote"))
                        joinvote = true;
                    if (group.HasPermission("setflags"))
                        setflags = true;
                    if (group.HasPermission("managec3settings"))
                        managec3settings = true;
                    if (group.HasPermission("cvote"))
                        cvote = true;
                    if (group.HasPermission("duel"))
                        duel = true;
                    if (group.HasPermission("setspawns"))
                        setspawns = true;
                }
            }
            List<string> perm = new List<string>();
            if (!vote)
                perm.Add("vote");
            if (!joinvote)
                perm.Add("joinvote");
            if (!duel)
                perm.Add("duel");
            TShock.Groups.AddPermissions("default", perm);

            perm.Clear();
            if (!setflags)
                perm.Add("setflags");
            if (!setflags)
                perm.Add("setspawns");
            if (!managec3settings)
                perm.Add("managec3settings");
            if (!cvote)
                perm.Add("cvote");
            if (!setspawns)
                perm.Add("setspawns");
            TShock.Groups.AddPermissions("trustedadmin", perm);
            #endregion
            //Converted v2.2
            #region AddCommands
            if (C3Mod.C3Config.TeamColor1 == 1)
            {
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam1Flag, "setctfredflag"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam1Spawn, "setctfredspawn"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlagTeam1Spawn, "setoneflagredspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetDuelTeam1Spawn, "setduelredspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetTDMTeam1Spawn, "settdmredspawn"));
            }
            else if (C3Mod.C3Config.TeamColor1 == 2)
            {
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam1Flag, "setctfgreenflag"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam1Spawn, "setctfgreenspawn"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlagTeam1Spawn, "setoneflaggreenspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetDuelTeam1Spawn, "setduelgreenspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetTDMTeam1Spawn, "settdmgreenspawn"));
            }
            else if (C3Mod.C3Config.TeamColor1 == 3)
            {
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam1Flag, "setctfblueflag"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam1Spawn, "setctfbluespawn"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlagTeam1Spawn, "setoneflagbluespawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetDuelTeam1Spawn, "setduelbluespawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetTDMTeam1Spawn, "settdmbluespawn"));
            }
            else if (C3Mod.C3Config.TeamColor1 == 4)
            {
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam1Flag, "setctfyellowflag"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam1Spawn, "setctfyellowspawn"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlagTeam1Spawn, "setoneflagyellowspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetDuelTeam1Spawn, "setduelyellowspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetTDMTeam1Spawn, "settdmyellowspawn"));
            }
            if (C3Mod.C3Config.TeamColor2 == 1)
            {
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam2Flag, "setctfredflag"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam2Spawn, "setctfredspawn"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlagTeam2Spawn, "setoneflagredspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetDuelTeam2Spawn, "setduelredspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetTDMTeam2Spawn, "settdmredspawn"));
            }
            else if (C3Mod.C3Config.TeamColor2 == 2)
            {
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam2Flag, "setctfgreenflag"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam2Spawn, "setctfgreenspawn"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlagTeam2Spawn, "setoneflaggreenspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetDuelTeam2Spawn, "setduelgreenspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetTDMTeam2Spawn, "settdmgreenspawn"));
            }
            else if (C3Mod.C3Config.TeamColor2 == 3)
            {
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam2Flag, "setctfblueflag"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam2Spawn, "setctfbluespawn"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlagTeam2Spawn, "setoneflagbluespawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetDuelTeam2Spawn, "setduelbluespawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetTDMTeam2Spawn, "settdmbluespawn"));
            }
            else if (C3Mod.C3Config.TeamColor2 == 4)
            {
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam2Flag, "setctfyellowflag"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetCTFTeam2Spawn, "setctfyellowspawn"));
                Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlagTeam2Spawn, "setoneflagyellowspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetDuelTeam2Spawn, "setduelyellowspawn"));
                Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetTDMTeam2Spawn, "settdmyellowspawn"));
            }


            Commands.ChatCommands.Add(new Command("setflags", C3Commands.AddCTFArena, "addctfarena"));
            Commands.ChatCommands.Add(new Command("setflags", C3Commands.SetOneFlag, "setoneflag"));
            Commands.ChatCommands.Add(new Command("setflags", C3Commands.AddOneFlagArena, "addoneflagarena"));
            Commands.ChatCommands.Add(new Command("setflags", C3Commands.AddDuelArena, "addduelarena"));
            Commands.ChatCommands.Add(new Command("setflags", C3Commands.AddTDMArena, "addtdmarena"));

            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetFFASpawn, "setffaspawn"));
            Commands.ChatCommands.Add(new Command("setflags", C3Commands.AddFFAArena, "addffaarena"));

            Commands.ChatCommands.Add(new Command("vote", C3Commands.StartVote, "vote"));
            Commands.ChatCommands.Add(new Command("joinvote", C3Commands.JoinVote, "join"));
            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.Stop, "stop"));
            Commands.ChatCommands.Add(new Command(C3Commands.Quit, "quit"));
            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.SetCTFLimit, "setctflimit"));
            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.SetOneFlagLimit, "setoneflaglimit"));
            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.SetDuelLimit, "setduellimit"));
            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.SetTDMLimit, "settdmlimit"));

            Commands.ChatCommands.Add(new Command("managec3settings", C3Commands.SetFFALimit, "setffalimit"));

            Commands.ChatCommands.Add(new Command("cvote", C3Commands.CancelVote, "cvote"));
            Commands.ChatCommands.Add(new Command("duel", C3Commands.ChallengePlayer, "duel"));
            Commands.ChatCommands.Add(new Command(C3Commands.AcceptChallenge, "accept"));

            Commands.ChatCommands.Add(new Command(C3Commands.ListArenaID, "list"));
            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetApocPlayerSpawn, "setapocplayerspawn"));
            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetApocMonsterSpawn, "setapocmonsterspawn"));
            Commands.ChatCommands.Add(new Command("setspawns", C3Commands.SetApocSpectatorSpawn, "setapocspectatorspawn"));
            #endregion

            #region FlagPoints

            var table = new SqlTable("FlagPoints",
                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("RedX", MySqlDbType.Int32),
                new SqlColumn("RedY", MySqlDbType.Int32),
                new SqlColumn("BlueX", MySqlDbType.Int32),
                new SqlColumn("BlueY", MySqlDbType.Int32),
                new SqlColumn("RedSpawnX", MySqlDbType.Int32),
                new SqlColumn("RedSpawnY", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnX", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnY", MySqlDbType.Int32)
            );
            SQLWriter.EnsureExists(table);

            if ((ArenaCount = SQLEditor.ReadColumn("FlagPoints", "Name", new List<SqlValue>()).Count) != 0)
            {
                for (int i = 0; i < ArenaCount; i++)
                {
                    string name = SQLEditor.ReadColumn("FlagPoints", "Name", new List<SqlValue>())[i].ToString();
                    int redx = Int32.Parse(SQLEditor.ReadColumn("FlagPoints", "RedX", new List<SqlValue>())[i].ToString());
                    int redy = Int32.Parse(SQLEditor.ReadColumn("FlagPoints", "RedY", new List<SqlValue>())[i].ToString());
                    int bluex = Int32.Parse(SQLEditor.ReadColumn("FlagPoints", "BlueX", new List<SqlValue>())[i].ToString());
                    int bluey = Int32.Parse(SQLEditor.ReadColumn("FlagPoints", "BlueY", new List<SqlValue>())[i].ToString());
                    int redspawnx = Int32.Parse(SQLEditor.ReadColumn("FlagPoints", "RedSpawnX", new List<SqlValue>())[i].ToString());
                    int redspawny = Int32.Parse(SQLEditor.ReadColumn("FlagPoints", "RedSpawnY", new List<SqlValue>())[i].ToString());
                    int bluespawnx = Int32.Parse(SQLEditor.ReadColumn("FlagPoints", "BlueSpawnX", new List<SqlValue>())[i].ToString());
                    int bluespawny = Int32.Parse(SQLEditor.ReadColumn("FlagPoints", "BlueSpawnY", new List<SqlValue>())[i].ToString());

                    CTF.Arenas.Add(new CTFArena(new Vector2(redx, redy), new Vector2(bluex, bluey), new Vector2(redspawnx, redspawny), new Vector2(bluespawnx, bluespawny), name));
                }
            }

            #endregion

            #region DuelSpawns

            table = new SqlTable("DuelSpawns",
                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("RedSpawnX", MySqlDbType.Int32),
                new SqlColumn("RedSpawnY", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnX", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnY", MySqlDbType.Int32)
            );
            SQLWriter.EnsureExists(table);

            if ((ArenaCount = SQLEditor.ReadColumn("DuelSpawns", "Name", new List<SqlValue>()).Count) != 0)
            {
                for (int i = 0; i < ArenaCount; i++)
                {
                    string name = SQLEditor.ReadColumn("DuelSpawns", "Name", new List<SqlValue>())[0].ToString();
                    int redx = Int32.Parse(SQLEditor.ReadColumn("DuelSpawns", "RedSpawnX", new List<SqlValue>())[0].ToString());
                    int redy = Int32.Parse(SQLEditor.ReadColumn("DuelSpawns", "RedSpawnY", new List<SqlValue>())[0].ToString());
                    int bluex = Int32.Parse(SQLEditor.ReadColumn("DuelSpawns", "BlueSpawnX", new List<SqlValue>())[0].ToString());
                    int bluey = Int32.Parse(SQLEditor.ReadColumn("DuelSpawns", "BlueSpawnY", new List<SqlValue>())[0].ToString());

                    Duel.Arenas.Add(new DuelArena(new Vector2(redx, redy), new Vector2(bluex, bluey), name));
                }
            }

            #endregion

            #region OneFlagPoints

            table = new SqlTable("OneFlagPoints",
                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("FlagX", MySqlDbType.Int32),
                new SqlColumn("FlagY", MySqlDbType.Int32),
                new SqlColumn("RedSpawnX", MySqlDbType.Int32),
                new SqlColumn("RedSpawnY", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnX", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnY", MySqlDbType.Int32)
            );
            SQLWriter.EnsureExists(table);

            if ((ArenaCount = SQLEditor.ReadColumn("OneFlagPoints", "Name", new List<SqlValue>()).Count) != 0)
            {
                for (int i = 0; i < ArenaCount; i++)
                {
                    string name = SQLEditor.ReadColumn("OneFlagPoints", "Name", new List<SqlValue>())[i].ToString();
                    int flagx = Int32.Parse(SQLEditor.ReadColumn("OneFlagPoints", "FlagX", new List<SqlValue>())[i].ToString());
                    int flagy = Int32.Parse(SQLEditor.ReadColumn("OneFlagPoints", "FlagY", new List<SqlValue>())[i].ToString());
                    int redspawnx = Int32.Parse(SQLEditor.ReadColumn("OneFlagPoints", "RedSpawnX", new List<SqlValue>())[i].ToString());
                    int redspawny = Int32.Parse(SQLEditor.ReadColumn("OneFlagPoints", "RedSpawnY", new List<SqlValue>())[i].ToString());
                    int bluespawnx = Int32.Parse(SQLEditor.ReadColumn("OneFlagPoints", "BlueSpawnX", new List<SqlValue>())[i].ToString());
                    int bluespawny = Int32.Parse(SQLEditor.ReadColumn("OneFlagPoints", "BlueSpawnY", new List<SqlValue>())[i].ToString());

                    OneFlagCTF.Arenas.Add(new OneFlagArena(new Vector2(flagx, flagy), new Vector2(redspawnx, redspawny), new Vector2(bluespawnx, bluespawny), name));
                }
            }

            #endregion

            #region TDMSpawns

            table = new SqlTable("TDMSpawns",
                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("RedSpawnX", MySqlDbType.Int32),
                new SqlColumn("RedSpawnY", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnX", MySqlDbType.Int32),
                new SqlColumn("BlueSpawnY", MySqlDbType.Int32)
            );
            SQLWriter = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter.EnsureExists(table);

            if ((ArenaCount = SQLEditor.ReadColumn("TDMSpawns", "Name", new List<SqlValue>()).Count) != 0)
            {
                for (int i = 0; i < ArenaCount; i++)
                {
                    string name = SQLEditor.ReadColumn("TDMSpawns", "Name", new List<SqlValue>())[i].ToString();
                    int redx = Int32.Parse(SQLEditor.ReadColumn("TDMSpawns", "RedSpawnX", new List<SqlValue>())[i].ToString());
                    int redy = Int32.Parse(SQLEditor.ReadColumn("TDMSpawns", "RedSpawnY", new List<SqlValue>())[i].ToString());
                    int bluex = Int32.Parse(SQLEditor.ReadColumn("TDMSpawns", "BlueSpawnX", new List<SqlValue>())[i].ToString());
                    int bluey = Int32.Parse(SQLEditor.ReadColumn("TDMSpawns", "BlueSpawnY", new List<SqlValue>())[i].ToString());

                    TDM.Arenas.Add(new TDMArena(new Vector2(redx, redy), new Vector2(bluex, bluey), name));
                }
            }

            #endregion

            #region FFASpawns

            table = new SqlTable("FFASpawns",
                new SqlColumn("Name", MySqlDbType.Text),
                new SqlColumn("SpawnX", MySqlDbType.Int32),
                new SqlColumn("SpawnY", MySqlDbType.Int32)
            );
            SQLWriter = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter.EnsureExists(table);

            if ((ArenaCount = SQLEditor.ReadColumn("FFASpawns", "Name", new List<SqlValue>()).Count) != 0)
            {
                for (int i = 0; i < ArenaCount; i++)
                {
                    string name = SQLEditor.ReadColumn("FFASpawns", "Name", new List<SqlValue>())[i].ToString();
                    int x = Int32.Parse(SQLEditor.ReadColumn("FFASpawns", "SpawnX", new List<SqlValue>())[i].ToString());
                    int y = Int32.Parse(SQLEditor.ReadColumn("FFASpawns", "SpawnY", new List<SqlValue>())[i].ToString());

                    FFA.Arenas.Add(new FFAArena(new Vector2(x, y), name));
                }
            }

            #endregion

            #region Apocalypse

            table = new SqlTable("Apocalypse",
                new SqlColumn("SpawnX", MySqlDbType.Int32),
                new SqlColumn("SpawnY", MySqlDbType.Int32),
                new SqlColumn("MonsterSpawnX", MySqlDbType.Int32),
                new SqlColumn("MonsterSpawnY", MySqlDbType.Int32),
                new SqlColumn("SpectatorSpawnX", MySqlDbType.Int32),
                new SqlColumn("SpectatorSpawnY", MySqlDbType.Int32)
            );
            SQLWriter = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            SQLWriter.EnsureExists(table);

            if (SQLEditor.ReadColumn("Apocalypse", "SpawnX", new List<SqlValue>()).Count == 0)
            {
                List<SqlValue> list = new List<SqlValue>();
                list.Add(new SqlValue("SpawnX", 0));
                list.Add(new SqlValue("SpawnY", 0));
                list.Add(new SqlValue("MonsterSpawnX", 0));
                list.Add(new SqlValue("MonsterSpawnY", 0));
                list.Add(new SqlValue("SpectatorSpawnX", 0));
                list.Add(new SqlValue("SpectatorSpawnY", 0));
                SQLEditor.InsertValues("Apocalypse", list);
            }
            else
            {
                Apocalypse.PlayerSpawn.X = Int32.Parse(SQLEditor.ReadColumn("Apocalypse", "SpawnX", new List<SqlValue>())[0].ToString());
                Apocalypse.PlayerSpawn.Y = Int32.Parse(SQLEditor.ReadColumn("Apocalypse", "SpawnY", new List<SqlValue>())[0].ToString());
                Apocalypse.MonsterSpawn.X = Int32.Parse(SQLEditor.ReadColumn("Apocalypse", "MonsterSpawnX", new List<SqlValue>())[0].ToString());
                Apocalypse.MonsterSpawn.Y = Int32.Parse(SQLEditor.ReadColumn("Apocalypse", "MonsterSpawnY", new List<SqlValue>())[0].ToString());
                Apocalypse.SpectatorArea.X = Int32.Parse(SQLEditor.ReadColumn("Apocalypse", "SpectatorSpawnX", new List<SqlValue>())[0].ToString());
                Apocalypse.SpectatorArea.Y = Int32.Parse(SQLEditor.ReadColumn("Apocalypse", "SpectatorSpawnY", new List<SqlValue>())[0].ToString());
            }

            #endregion
        }

        internal void OnGreetPlayer(GreetPlayerEventArgs args)
        {
            lock (C3Players)
                C3Players.Add(new C3Player(args.Who));

            if(C3Config.ShowWelcomeMessage)
				TShock.Players[args.Who].SendMessage("This server is running C3Mod, created by Twitchy. C3Mod is now open source.", Color.Cyan);
        }
        //Converted v2.2
		internal void OnUpdate(EventArgs args)
        {
            if (C3Config.C3TeamsLocked)
            {
                lock (C3Players)
                {
                    foreach (C3Player player in C3Mod.C3Players)
                    {
                        if (player.GameType == "")
                        {
                            if (Main.player[player.Index].team == C3Config.TeamColor1 ||
                                Main.player[player.Index].team == C3Config.TeamColor2)
                            {
                                player.SendMessage("This team is reserved for C3Mod, switching you to neutral",
                                                   Color.DarkCyan);
                                TShock.Players[player.Index].SetTeam(0);
                            }
                        }
                    }
                }
            }
        }

        internal void OnLeave(LeaveEventArgs args)
        {
            lock (C3Mod.C3Players)
            {
                for (int i = 0; i < C3Mod.C3Players.Count; i++)
                {
					if (C3Mod.C3Players[i].Index == args.Who)
                    {
                        C3Mod.C3Players.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        internal void GetData(GetDataEventArgs e)
        {
            PacketTypes type = e.MsgID;
            var player = TShock.Players[e.Msg.whoAmI];

            if (player == null)
            {
                e.Handled = true;
                return;
            }

            if (!player.ConnectionAlive)
            {
                e.Handled = true;
                return;
            }

            using (var data = new MemoryStream(e.Msg.readBuffer, e.Index, e.Length))
            {
                try
                {
                    if (GetDataHandlers.HandlerGetData(type, player, data))
                        e.Handled = true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }
    }
}