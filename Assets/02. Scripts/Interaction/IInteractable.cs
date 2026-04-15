using MS.Field;

namespace MS.Interaction
{
    public interface IInteractable
    {
        string InteractIconKey { get; }
        void Interact(PlayerCharacter _player);
    }
}
