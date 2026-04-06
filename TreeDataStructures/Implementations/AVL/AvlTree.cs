using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.AVL;

public class AvlTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, AvlNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override AvlNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);

    private int CalculateHeight(AvlNode<TKey, TValue> node) => node?.Height ?? 0;

    private void ChangeHeight(AvlNode<TKey, TValue> node)
    {
        if (node != null)
        {
            node.Height = 1 + Math.Max(CalculateHeight(node.Right), CalculateHeight(node.Left));
        }
    }

    private void RotateLeftAvl(AvlNode<TKey, TValue> node)
    {
        RotateLeft(node);
        ChangeHeight(node);
        ChangeHeight(node.Parent);
    }
    
    private void RotateRightAvl(AvlNode<TKey, TValue> node)
    {
        RotateRight(node);
        ChangeHeight(node);
        ChangeHeight(node.Parent);
    }
    
    private void RotateBigLeftAvl(AvlNode<TKey, TValue> node)
    {
        RotateBigLeft(node);
        ChangeHeight(node);
        ChangeHeight(node.Parent);
    }
    
    private void RotateBigRightAvl(AvlNode<TKey, TValue> node)
    {
        RotateBigRight(node);
        ChangeHeight(node);
        ChangeHeight(node.Parent);
    }

    private int CalculateBalance(AvlNode<TKey, TValue> node) => CalculateHeight(node.Right) - CalculateHeight(node.Left);
    
    protected override void OnNodeAdded(AvlNode<TKey, TValue> newNode)
    {
        var currentNode = newNode.Parent;
        while (currentNode != null)
        {
            CalculateHeight(currentNode);
            int balance = CalculateBalance(currentNode);

            if (balance > 1)
            {
                if (CalculateBalance(currentNode.Right) >= 0)
                {
                    RotateLeftAvl(currentNode);
                }
                else
                {
                    RotateBigLeftAvl(currentNode);
                }
            }
            else if (balance < -1)
            {
                if (CalculateBalance(currentNode.Left) <= 0)
                {
                    RotateRightAvl(currentNode);
                }
                else
                {
                    RotateBigRightAvl(currentNode);
                }
            }

            if (currentNode.Parent == null)
            {
                Root = currentNode;
            }
            currentNode = currentNode.Parent;
        }
    }

    
}