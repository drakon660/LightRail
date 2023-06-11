namespace LightRail.Wsdl.Core;

public class TreeNode<T> where T : NodeElement
{
    public int Level { get; set; }
    public T Content { get; set; }
    public TreeNode<T> Parent { get; private set; }
    public List<TreeNode<T>> Children { get; }
    public TreeNode<T> FirstChild => Children?.FirstOrDefault();
    public bool HasMultipleChildren => Children?.Count > 1;
    public bool HasChildren => Children?.Count > 0;

    private TreeNode(T content, int level)
    {
        Content = content;
        Children = new List<TreeNode<T>>();
        Level = level;
    }

    public static TreeNode<T> Create(T data, int level) => new TreeNode<T>(data, level);

    private void SetParent(TreeNode<T> parent)
    {
        Parent = parent;
    }

    public T FindChildByName(string name)
    {
        if (name == Content.Name)
            return Content;

        foreach (var nodeElement in Children)
        {
            var t = nodeElement.FindChildByName(name);

            if (t != null)
                return t;
        }

        return default;
    }
    
    public void AddChildAtLevel(int level, TreeNode<T> child)
    {
        if (level < 0)
        {
            throw new ArgumentException("Level must be a non-negative integer.");
        }
        if (level == 0 || level == 1)
        {
            child.SetParent(this);
            Children.Add(child);
        }
        else if (Children.Count > 0)
        {
            Children[^1].AddChildAtLevel(level - 1, child);
        }
        else
        {
            throw new ArgumentException("Level exceeds depth of tree.");
        }
    }
}
