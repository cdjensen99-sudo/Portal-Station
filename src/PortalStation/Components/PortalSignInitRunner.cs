using UnityEngine;

namespace PortalStation;

/// <summary>
/// Defers portal display-sign creation until the portal's network view is ready.
/// </summary>
internal sealed class PortalSignInitRunner : MonoBehaviour
{
    private void Start()
    {
        PortalDisplaySignSpawner.EnsureDisplaySign(gameObject);
        Destroy(this);
    }
}
