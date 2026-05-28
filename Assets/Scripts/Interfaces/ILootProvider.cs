public interface ILootProvider
{
    DropTableData dropTable { get; set; }
    int soulDropAmount { get; set; }
    int essenceDropAmount { get; set; }
    void SpawnSoulCore();
}
