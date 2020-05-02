public interface IComponent
{
    EntityComponent Container { get; set; }
    bool Active { get; set; }

    void OnAdd();
    void OnDestroy();
    void OnUpdate(float deltaTime);
}
