public interface IItem
{
    string Name { get; set; }
    int ID { get; set; }
    int NetID { get; }

    void Interact(int playerID);
    void Hide(bool hide);
    void Destroy();
}
