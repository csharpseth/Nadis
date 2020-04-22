public interface IItem
{
    string Name { get; set; }
    int ID { get; set; }
    ulong NetworkID { get; set; }

    void Interact(ulong interactorID);
    void Hide(bool hide);
    void Destroy(ulong netID);
}
