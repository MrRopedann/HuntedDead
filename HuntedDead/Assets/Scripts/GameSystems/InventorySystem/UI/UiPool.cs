using System.Collections.Generic;
using UnityEngine;

public class UiPool : MonoBehaviour
{
    readonly Dictionary<GameObject, Stack<GameObject>> _pool = new();

    public GameObject Get(GameObject prefab, Transform parent)
    {
        if (!_pool.TryGetValue(prefab, out var st)) { st = new Stack<GameObject>(); _pool[prefab] = st; }
        if (st.Count > 0) { var go = st.Pop(); go.transform.SetParent(parent, false); go.SetActive(true); return go; }
        return Instantiate(prefab, parent);
    }
    public void Recycle(GameObject prefab, GameObject go)
    {
        go.SetActive(false); go.transform.SetParent(transform, false);
        if (!_pool.TryGetValue(prefab, out var st)) { st = new Stack<GameObject>(); _pool[prefab] = st; }
        st.Push(go);
    }
}
