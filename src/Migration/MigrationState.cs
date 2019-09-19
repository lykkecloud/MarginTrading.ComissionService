namespace Migration
{
    public enum MigrationState
    {
        Started = 0,
        Finished = 1,
        RollbackStarted = 2,
        RolledBack = 3,
    }
}