namespace MemoLingo.Domain.Enums
{
    /// <summary>
    /// Status de progresso de uma trilha, lição ou sessão de estudo.
    /// </summary>
    public enum ProgressStatus
    {
        Locked = 1,
        Available = 2,
        InProgress = 3,
        Completed = 4,
        Abandoned = 5
    }
}
