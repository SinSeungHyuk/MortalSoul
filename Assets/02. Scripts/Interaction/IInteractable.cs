using Cysharp.Threading.Tasks;
using MS.Field;

namespace MS.Interaction
{
    public interface IInteractable
    {
        string InteractIconKey { get; }
        UniTask InteractAsync(PlayerCharacter _player);
    }
}
