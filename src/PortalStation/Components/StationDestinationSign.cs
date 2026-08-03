using UnityEngine;

namespace PortalStation;

public sealed class StationDestinationSign : MonoBehaviour
{
    private int _slotIndex;

    public int SlotIndex => _slotIndex;

    internal void SetSlotIndex(int slotIndex)
    {
        _slotIndex = slotIndex;
    }
}
