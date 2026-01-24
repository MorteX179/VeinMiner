using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Common.Entities;

public class VeinMinerPickaxeBehavior : CollectibleBehavior
{
    private const int MaxBlocks = 128;

    public VeinMinerPickaxeBehavior(CollectibleObject collObj) : base(collObj) { }

    public override bool OnBlockBrokenWith(
    IWorldAccessor world,
    Entity byEntity,
    ItemSlot itemslot,
    BlockSelection blockSel,
    float dropQuantityMultiplier,
    ref EnumHandling bhHandling)
    {
        bool result = base.OnBlockBrokenWith(
            world, byEntity, itemslot, blockSel,
            dropQuantityMultiplier, ref bhHandling
        );

        if (world.Side != EnumAppSide.Server) return result;
        if (blockSel == null) return result;

        IServerPlayer player = (byEntity as EntityPlayer)?.Player as IServerPlayer;
        if (player == null) return result;

        Block startBlock = world.BlockAccessor.GetBlock(blockSel.Position);
        if (startBlock == null) return result;

        if (!IsOre(startBlock)) return result;

        HashSet<BlockPos> visited = new HashSet<BlockPos>();
        MineOre(world, blockSel.Position, startBlock, player, visited);

        return result;
    }

    private bool IsOre(Block block)
    {
        return block.Code?.Path.StartsWith("ore-") == true;
    }

    private void MineOre
        (
        IWorldAccessor world,
        BlockPos startPos,
        Block startBlock,
        IServerPlayer player,
        HashSet<BlockPos> visited
        )
    {
        Queue<BlockPos> queue = new Queue<BlockPos>();
        queue.Enqueue(startPos);

        while (queue.Count > 0 && visited.Count < MaxBlocks)
        {
            BlockPos pos = queue.Dequeue();

            if (!visited.Add(pos)) continue;

            Block block = world.BlockAccessor.GetBlock(pos);
            if (block == null || block.Code != startBlock.Code) continue;

            world.BlockAccessor.BreakBlock(pos, player);

            foreach (BlockPos n in GetNeighbors(pos))
            {
                if (!visited.Contains(n))
                {
                    queue.Enqueue(n);
                }
            }
        }
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
