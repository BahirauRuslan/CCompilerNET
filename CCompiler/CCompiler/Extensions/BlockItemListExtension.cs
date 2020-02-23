using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CCompiler.ObjectDefinitions;

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
