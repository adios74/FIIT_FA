using System.Diagnostics.CodeAnalysis;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.Treap;

public class Treap<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, TreapNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    /// <summary>
    /// Разрезает дерево с корнем <paramref name="root"/> на два поддерева:
    /// Left: все ключи <= <paramref name="key"/>
    /// Right: все ключи > <paramref name="key"/>
    /// </summary>
    protected virtual (TreapNode<TKey, TValue>? Left, TreapNode<TKey, TValue>? Right) Split(TreapNode<TKey, TValue>? root, TKey key)
    {
        TreapNode<TKey, TValue>? left;
        TreapNode<TKey, TValue>? right;

        if (root == null)
        {
            return (null, null);
        }
        
        int cmp = key.CompareTo(root.Key);
        
        if (cmp >= 0)
        {
            (left, right) = Split(root.Right, key);
            root.Right = left;
            return (root, right);
        }
        else
        {
            (left, right) = Split(root.Left, key);
            root.Left = right;
            return (left, root);
        }
        
    }

    /// <summary>
    /// Сливает два дерева в одно.
    /// Важное условие: все ключи в <paramref name="left"/> должны быть меньше ключей в <paramref name="right"/>.
    /// Слияние происходит на основе Priority (куча).
    /// </summary>
    protected virtual TreapNode<TKey, TValue>? Merge(TreapNode<TKey, TValue>? left, TreapNode<TKey, TValue>? right)
    {
        if (left == null)
        {
            return right;
            
        } else if (right == null)
        {
            return left;
            
        } else if (left.Priority > right.Priority)
        {
            left.Right = Merge(left.Right, right);
            return left;
            
        } else
        {
            right.Left = Merge(left, right.Left);
            return right;
        }
    }
    

    public override void Add(TKey key, TValue value)
    {
        var node = FindNode(key);
        if (node != null)
        {
            node.Value = value;
            return;
        }
        var newNode = CreateNode(key, value);
        var (left, right) = Split(Root, key);
        var newRoot = Merge(left, newNode);
        Root = Merge(newRoot, right);
        Count++;
    }

    public override bool Remove(TKey key)
    {
        var node = FindNode(key);
        if (node == null)
        {
            return false;
        }

        var newNode = Merge(node.Left, node.Right);
        if (node.Parent == null)
        {
            Root = newNode;
        }
        else if (node.IsLeftChild)
        {
            node.Parent.Left = newNode;
        }
        else if (node.IsRightChild)
        {
            node.Parent.Right = newNode;
        }

        newNode?.Parent = node.Parent;
        Count--;
        return true;
    }

    protected override TreapNode<TKey, TValue> CreateNode(TKey key, TValue value) 
        => new TreapNode<TKey, TValue>(key, value);
    
    
    protected override void OnNodeAdded(TreapNode<TKey, TValue> newNode) { }
    
    protected override void OnNodeRemoved(TreapNode<TKey, TValue>? parent, TreapNode<TKey, TValue>? child) { }
    
}