namespace poker.Server
{
    public enum GameState
    {
        PreGame,
        NewRound,
        Bets,
        Waiting,
    }

    public enum PlayerState
    {
        Playing,
        Folded,
    }
}