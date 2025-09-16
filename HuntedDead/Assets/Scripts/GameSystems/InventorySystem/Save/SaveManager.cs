using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public PlayerInventory inv;
    public PlayerStateRelay stateRelay;

    public void Save()
    {
        var list = new List<StackDto>(128);
        DumpContainer(inv.Pockets, "Pockets", list);
        DumpContainer(inv.Backpack, "Backpack", list);
        DumpContainer(inv.Vest, "Vest", list);

        var dto = new SaveDto { stacks = list.ToArray(), playerState = stateRelay.Current.ToString() };
        SaveSystem.Save(dto);
    }

    void DumpContainer(ContainerInstance c, string name, List<StackDto> outList)
    {
        if (c.def == null) return;
        for (int i = 0; i < c.count; i++)
        {
            var gi = c.items[i];
            var p = c.positions[i];
            outList.Add(new StackDto
            {
                itemGuid = gi.stack.key.itemGuid,
                tier = (int)gi.stack.key.tier,
                qty = gi.stack.qty,
                rotated = gi.rotated,
                x = p.x,
                y = p.y,
                z = p.z,
                cont = name
            });
        }
    }

    public void Load(DbRegistry db)
    {
        var dto = SaveSystem.Load();
        if (dto == null) return;
        inv.Pockets.Init(inv.pocketsDef);
        inv.Backpack.Init(inv.backpackDef);
        if (inv.vestDef) inv.Vest.Init(inv.vestDef);

        for (int i = 0; i < dto.stacks.Length; i++)
        {
            var s = dto.stacks[i];
            var def = db.ItemByGuid(s.itemGuid);
            var gi = new GridItem
            {
                def = def,
                size = def.is3D ? def.size3D : def.size2D,
                rotated = s.rotated,
                stack = new ItemStack
                {
                    key = new VariantKey { itemGuid = s.itemGuid, tier = (QualityTier)s.tier },
                    qty = s.qty
                }
            };
            ContainerInstance cont = s.cont == "Pockets" ? inv.Pockets : s.cont == "Backpack" ? inv.Backpack : inv.Vest;
            cont.TryPlaceAt(ref gi, s.x, s.y, s.z, out _);
        }
    }
}
