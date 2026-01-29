using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;

public class VeinMinerPickaxeBehavior : CollectibleBehavior
{
    private const int MaxBlocks = 4;

    public VeinMinerPickaxeBehavior(CollectibleObject collObj) : base(collObj) { }

    public override bool OnBlockBrokenWith
        (
            IWorldAccessor world,
            Entity byEntity,
            ItemSlot itemslot,
            BlockSelection blockSel,
            float dropQuantityMultiplier,
            ref EnumHandling bhHandling
        )
    {
        if (world.Side != EnumAppSide.Server) return false;
        if (blockSel == null) return false;

        if (byEntity is not EntityPlayer ep) return false;
        IServerPlayer player = ep.Player as IServerPlayer;
        if (player == null) return false;

        if (ep.Controls.Sneak)
        {
            bool enabled = IsVeinMinerEnabled(itemslot);
            SetVeinMinerEnabled(itemslot, !enabled);

            player.SendMessage(
                GlobalConstants.GeneralChatGroup,
                Lang.Get(enabled ? "veinminer:off" : "veinminer:on"),
                EnumChatType.Notification
            );

            bhHandling = EnumHandling.PreventDefault;
            return true;
        }

        if (!IsVeinMinerEnabled(itemslot)) return false;

        Block startBlock = world.BlockAccessor.GetBlock(blockSel.Position);
        if (startBlock == null || !IsOre(startBlock)) return false;

        bhHandling = EnumHandling.PreventDefault;

        int mined = MineOre(world, blockSel.Position, startBlock, player, skipFirstBlock: false);

        if (mined == 0)
        {
            return true;
        }

        bhHandling = EnumHandling.PreventDefault;

        itemslot.Itemstack.Collectible.DamageItem(world, byEntity, itemslot, mined);

        return false;
    }


    private int MineOre
        (
        IWorldAccessor world,
        BlockPos startPos,
        Block startBlock,
        IServerPlayer player,
        bool skipFirstBlock = false
        )
    {
        HashSet<BlockPos> visited = new();
        Queue<BlockPos> queue = new();
        List<BlockPos> toBreak = new();

        queue.Enqueue(startPos);
        bool first = true;

        while (queue.Count > 0 && toBreak.Count < VeinMinerModSystem.Config.MaxBlocks)
        {
            BlockPos pos = queue.Dequeue();
            if (!visited.Add(pos)) continue;

            Block block = world.BlockAccessor.GetBlock(pos);
            if (block == null || block.Code != startBlock.Code) continue;

            if (!(first && skipFirstBlock))
            {
                toBreak.Add(pos);
            }

            first = false;

            foreach (BlockPos n in GetNeighbors(pos))
            {
                if (!visited.Contains(n))
                {
                    queue.Enqueue(n);
                }
            }
        }

        foreach (BlockPos pos in toBreak)
        {
            world.BlockAccessor.BreakBlock(pos, player);
        }

        return toBreak.Count;
    }

    private bool IsOre(Block block)
    {
        return block.Code?.Path.StartsWith("ore-") == true;
    }

    private bool IsVeinMinerEnabled(ItemSlot slot)
    {
        return slot.Itemstack.Attributes.GetBool("veinminerEnabled", true);
    }

    private void SetVeinMinerEnabled(ItemSlot slot, bool value)
    {
        slot.Itemstack.Attributes.SetBool("veinminerEnabled", value);
    }

    private IEnumerable<BlockPos> GetNeighbors(BlockPos pos)
    {
        yield return pos.AddCopy(1, 0, 0);
        yield return pos.AddCopy(-1, 0, 0);
        yield return pos.AddCopy(0, 1, 0);
        yield return pos.AddCopy(0, -1, 0);
        yield return pos.AddCopy(0, 0, 1);
        yield return pos.AddCopy(0, 0, -1);
    }
}