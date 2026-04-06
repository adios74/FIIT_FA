using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Implementations.BST;

namespace TreeDataStructures.Implementations.Splay;

public class SplayTree<TKey, TValue> : BinarySearchTree<TKey, TValue>
    where TKey : IComparable<TKey>
{
    protected override BstNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new(key, value);
    
    protected override void OnNodeAdded(BstNode<TKey, TValue> newNode)
    {
        Splay(newNode);
    }
    
    protected override void OnNodeRemoved(BstNode<TKey, TValue>? parent, BstNode<TKey, TValue>? child)
    {
        if (child != null && child.Parent != null)
        {
            Splay(child.Parent);
        }
    }

    private void Splay(BstNode<TKey, TValue> node)
    {
        while (node.Parent != null)
        {
            var parent = node.Parent;
            if (parent.Parent == null)
            {
                if (node.IsLeftChild)
                {
                    RotateRight(parent);
                }
                else if (node.IsRightChild)
                {
                    RotateLeft(parent);
                }
            }
            else
            {
                var grandparent = parent.Parent;
                if (parent.IsLeftChild && node.IsLeftChild)
                {
                    RotateRight(grandparent);
                    RotateRight(parent);
                }
                else if (parent.IsRightChild && node.IsRightChild)
                {
                    RotateLeft(grandparent);
                    RotateLeft(parent);
                }
                else if (parent.IsLeftChild && node.IsRightChild)
                {
                    RotateBigRight(grandparent);
                }
                else if (parent.IsRightChild && node.IsLeftChild)
                {
                    RotateBigLeft(grandparent);
                }
            }
        }
        
        Root = node;
    }
    
    public override bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var node = FindNode(key);
        if (node == null)
        {
            value = default;
            return false;
        }
        value = node.Value;
        Splay(node);
        return true;
    }

    public override bool ContainsKey(TKey key)
    {
        var node = FindNode(key);
        if (node != null)
        {
            Splay(node);
            return true;
        }

        return false;
    }
}
