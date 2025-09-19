using UnityEngine;
using System.Collections.Generic;

public class VicinityScanner : MonoBehaviour
{
    [SerializeField] float radius = 3f;
    [SerializeField] LayerMask itemMask;
    readonly Collider[] _buf = new Collider[64];
    readonly Dictionary<VariantKey, int> _agg = new(64);

    public IReadOnlyDictionary<VariantKey, int> ScanAggregated()
    {
        _agg.Clear();
        int n = Physics.OverlapSphereNonAlloc(transform.position, radius, _buf, itemMask);
        for (int i = 0; i < n; i++)
        {
            if (_buf[i] && _buf[i].TryGetComponent(out WorldItem wi))
            {
                var key = wi.Key; int qty = wi.Qty;
                if (_agg.TryGetValue(key, out var cur)) _agg[key] = cur + qty;
                else _agg[key] = qty;
            }
        }
        return _agg;
    }

    public int PickupInto(ContainerInstance dst, VariantKey key, int wantQty)
    {
        int picked = 0;
        int n = Physics.OverlapSphereNonAlloc(transform.position, radius, _buf, itemMask);
        for (int i = 0; i < n && picked < wantQty; i++)
        {
            if (_buf[i] && _buf[i].TryGetComponent(out WorldItem wi))
            {
                if (!wi.Key.Equals(key)) continue;
                int take = Mathf.Min(wantQty - picked, wi.Qty);

                var def = FindObjectOfType<DbRegistry>().ItemByGuid(key.itemGuid);
                var gi = new GridItem
                {
                    def = def,
                    size = def.is3D ? def.size3D : def.size2D,
                    rotated = false,
                    stack = new ItemStack { key = key, qty = take }
                };
                if (Placement.TryPlace(dst, ref gi, out _, out _))
                {
                    picked += take;
                    wi.Qty -= take;
                    if (wi.Qty <= 0) GameObject.Destroy(wi.gameObject);
                }
            }
        }
        return picked;
    }
}
