using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace C3Mod
{
    public delegate void PvPDeathEventHandler(DeathArgs e);
    public delegate void GameEndEventHandler(GameEndArgs e);
    public delegate void FlagCaptureHandler(FlagCaptureArgs e);
    public delegate void FlagGabbedHandler(FlagGrabbedArgs e);
    public delegate void ApocalypseWaveAdvanceHandler(ApocalypseWaveAdvanceArgs e);

    public class C3Events
    {
        public static event PvPDeathEventHandler OnPvPDeath;
        public static event GameEndEventHandler OnGameEnd;
        public static event FlagCaptureHandler OnFlagCapture;
        public static event FlagGabbedHandler OnFlagGrabed;
        public static event ApocalypseWaveAdvanceHandler OnApocWaveAdvance;

        internal static void Death(C3Player killer, C3Player killed, string gametype, bool pvpkill)
        {
            DeathArgs e = new DeathArgs();
            e.Killer = killer;
            e.Killed = killed;
            e.GameType = gametype;
            e.PvPKill = pvpkill;
            if (OnPvPDeath != null)
                OnPvPDeath(e);
        }

        internal static void GameEnd(List<C3Player> winningteamplayers, List<C3Player> losingteamplayers, string gametype, int winningteamscore, int losingteamscore)
        {
            GameEndArgs e = new GameEndArgs();
            e.WinningTeamPlayers = winningteamplayers;
            e.LosingTeamPlayers = losingteamplayers;
            e.GameType = gametype;
            e.WinningTeamScore = winningteamscore;
            e.LosingTeamScore = losingteamscore;
            if (OnGameEnd != null)
                OnGameEnd(e);
        }

        internal static void FlagCapture(C3Player who, string gametype, string whoscored, int capturedteamscore, int otherteamscore)
        {
            FlagCaptureArgs e = new FlagCaptureArgs();
            e.Who = who;
            e.GameType = gametype;
            e.WhoScored = whoscored;
            e.CapturedTeamScore = capturedteamscore;
            e.OtherTeamScore = otherteamscore;
            if (OnFlagCapture != null)
                OnFlagCapture(e);
        }

        internal static void FlagGrabbed(C3Player who, string gametype)
        {
            FlagGrabbedArgs e = new FlagGrabbedArgs();
            e.Who = who;
            e.GameType = gametype;
            if (OnFlagGrabed != null)
                OnFlagGrabed(e);
        }

        internal static void WaveAdvance(List<C3Player> aliveplayers, List<C3Player> spectatingplayers, int nextwave)
        {
            ApocalypseWaveAdvanceArgs e = new ApocalypseWaveAdvanceArgs();
            e.AlivePlayers = aliveplayers;
            e.SpectatingPlayers = spectatingplayers;
            e.NextWave = nextwave;
            if (OnApocWaveAdvance != null)
                OnApocWaveAdvance(e);
        }

        internal static void VoteEvent(C3Player player, bool vote, bool join, string gametype)
        {
            VoteArgs e = new VoteArgs();
            e.GameType = gametype;
            e.IsCallingVote = vote;
            e.IsJoiningVote = join;
            e.Player = player;
        }
    }

    public class DeathArgs : EventArgs
    {
        public C3Player Killer;
        public C3Player Killed;
        public bool PvPKill;
        public string GameType;
    }

    public class GameEndArgs : EventArgs
    {
        public List<C3Player> WinningTeamPlayers;
        public List<C3Player> LosingTeamPlayers;
        public string GameType;
        public int WinningTeamScore;
        public int LosingTeamScore;
    }

    public class FlagCaptureArgs : EventArgs
    {
        public C3Player Who;
        public string GameType;
        public string WhoScored;
        public int CapturedTeamScore;
        public int OtherTeamScore;
    }

    public class FlagGrabbedArgs : EventArgs
    {
        public C3Player Who;
        public string GameType;
    }

    public class ApocalypseWaveAdvanceArgs : EventArgs
    {
        public List<C3Player> AlivePlayers;
        public List<C3Player> SpectatingPlayers;
        public int NextWave;
    }

    public class VoteArgs : EventArgs
    {
        public C3Player Player;
        public bool IsCallingVote;
        public bool IsJoiningVote;
        public string GameType;
    }
}
