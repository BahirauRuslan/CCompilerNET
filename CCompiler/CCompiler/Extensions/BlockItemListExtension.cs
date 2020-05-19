using System.Collections.Generic;

namespace CCompiler.Extensions
{
    public static class BlockItemListExtension
    {
        public static Stack<CParser.BlockItemContext> RBABlockItemStack(
            this CParser.BlockItemListContext blockItemList)
        {
            var localBlockItemList = blockItemList;
            var blockItemStack
                = new Stack<CParser.BlockItemContext>();

            while (localBlockItemList.blockItemList() != null)
            {
                blockItemStack
                    .Push(localBlockItemList.blockItem());

                localBlockItemList = localBlockItemList.blockItemList();
            }

            blockItemStack
                .Push(localBlockItemList.blockItem());

            return blockItemStack;
        }
    }
}
