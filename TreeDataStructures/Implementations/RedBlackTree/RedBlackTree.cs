using System.Reflection;
using TreeDataStructures.Core;

namespace TreeDataStructures.Implementations.RedBlackTree;

public class RedBlackTree<TKey, TValue> : BinarySearchTreeBase<TKey, TValue, RbNode<TKey, TValue>>
    where TKey : IComparable<TKey>
{
    protected override RbNode<TKey, TValue> CreateNode(TKey key, TValue value)
        => new RbNode<TKey, TValue>(key, value);
    
    protected override void OnNodeAdded(RbNode<TKey, TValue> newNode)
    {
        while (newNode.Parent != null && newNode.Parent.Color == RbColor.Red)
        {
            if (newNode.Parent.IsLeftChild)
            {
                var uncle = newNode.Parent.Parent?.Right;
                var grandparent = newNode.Parent.Parent;
                if (uncle != null && grandparent != null && uncle.Color == RbColor.Red)
                {
                    newNode.Parent.Color = RbColor.Black;
                    uncle.Color = RbColor.Black;
                    grandparent.Color = RbColor.Red;
                    newNode = grandparent;
                }
                else
                {
                    if (newNode.IsRightChild)
                    {
                        newNode = newNode.Parent;
                        RotateLeft(newNode);
                    }

                    if (newNode.Parent != null && grandparent != null)
                    {
                        newNode.Parent.Color = RbColor.Black;
                        grandparent.Color = RbColor.Red;
                        RotateRight(grandparent);
                        break;
                    }
                }
            }
            else
            {
                var uncle = newNode.Parent.Parent?.Left;
                var grandparent = newNode.Parent.Parent;
                if (uncle != null && grandparent != null && uncle.Color == RbColor.Red)
                {
                    newNode.Parent.Color = RbColor.Black;
                    uncle.Color = RbColor.Black;
                    grandparent.Color = RbColor.Red;
                    newNode = grandparent;
                }
                else
                {
                    if (newNode.IsLeftChild)
                    {
                        newNode = newNode.Parent;
                        RotateRight(newNode);
                    }

                    if (newNode.Parent != null && grandparent != null)
                    {
                        newNode.Parent.Color = RbColor.Black;
                        grandparent.Color = RbColor.Red;
                        RotateLeft(grandparent);
                        break;
                    }
                }
            }
        }
        Root.Color = RbColor.Black;
    }
    
    
    protected override void OnNodeRemoved(RbNode<TKey, TValue>? parent, RbNode<TKey, TValue>? child)
    {
        if (child == null) { return;}
        
        if (child.Color == RbColor.Red) { return;}
        
        while (child != Root && (child?.Color ?? RbColor.Black) == RbColor.Black)
        {
            if (child == parent?.Left)
            {
                var brother = parent?.Right;
                
                if (brother == null) { break;}
                
                if (brother.Color == RbColor.Red && parent != null)
                {
                    brother.Color = RbColor.Black;
                    parent.Color = RbColor.Red;
                    RotateLeft(parent);
                    brother = parent?.Right;
                    
                    if (brother == null) { break;}
                }
                
                if ((brother.Left?.Color ?? RbColor.Black) == RbColor.Black &&
                    (brother.Right?.Color ?? RbColor.Black) == RbColor.Black)
                {
                    brother.Color = RbColor.Red;
                    child = parent;
                    parent = child?.Parent;
                }
                
                else
                {
                    if ((brother.Right?.Color ?? RbColor.Black) == RbColor.Black)
                    {
                        if (brother.Left != null)
                        {
                            brother.Left.Color = RbColor.Black;
                        }
                        
                        brother.Color = RbColor.Red;
                        RotateRight(brother);
                        brother = parent?.Right;
                        
                        if (brother == null) { break;}
                    }

                    if (parent != null)
                    {
                        brother.Color = parent.Color;
                        parent.Color = RbColor.Black;
                        
                        if (brother.Right != null)
                        {
                            brother.Right.Color = RbColor.Black;
                        }
                        
                        RotateLeft(parent);
                        child = Root;
                        break;
                    }
                }
            }
            else
            {
                var brother = parent?.Left;
                
                if (brother == null) { break;}
                
                if (brother.Color == RbColor.Red && parent != null)
                {
                    brother.Color = RbColor.Black;
                    parent.Color = RbColor.Red;
                    RotateRight(parent);
                    brother = parent?.Left;
                    
                    if (brother == null) { break;}
                }
                
                if ((brother.Left?.Color ?? RbColor.Black) == RbColor.Black &&
                    (brother.Right?.Color ?? RbColor.Black) == RbColor.Black)
                {
                    brother.Color = RbColor.Red;
                    child = parent;
                    parent = child?.Parent;
                }
                
                else
                {
                    if ((brother.Left?.Color ?? RbColor.Black) == RbColor.Black)
                    {
                        if (brother.Right != null)
                        {
                            brother.Right.Color = RbColor.Black;
                        }
                        
                        brother.Color = RbColor.Red;
                        RotateLeft(brother);
                        brother = parent?.Left;
                        
                        if (brother == null) { break;}
                    }

                    if (parent != null)
                    {
                        brother.Color = parent.Color;
                        parent.Color = RbColor.Black;
                        
                        if (brother.Left != null)
                        {
                            brother.Left.Color = RbColor.Black;
                        }
                        
                        RotateRight(parent);
                        child = Root;
                        break;
                    }
                }
            }
        }

        if (child != null)
        {
            child.Color = RbColor.Black;
        }

        if (Root != null)
        {
            Root.Color = RbColor.Black;
        }
    }
}