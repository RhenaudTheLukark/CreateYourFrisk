using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ModPage {
    public DirectoryInfo path;
    public bool isMod;
    public bool isOpen = true;
    public bool isNestedOpen { get { return (parent == null ? true : parent.isNestedOpen) && isOpen; } }
    public bool isHidden { get { return parent != null ? !parent.isNestedOpen : false; } }

    public ModPage parent;
    public List<ModPage> children = new List<ModPage>();
    public int linkedFolders = 0;
    public int deepLinkedFolders { get { return children.Select(p => p.deepLinkedFolders).Sum() + linkedFolders; } }
    public int linkedMods = 0;
    public int deepLinkedMods { get { return children.Select(p => p.deepLinkedMods).Sum() + linkedMods; } }
    public int linkedChildren { get { return linkedFolders + linkedMods; } }
    public int deepLinkedChildren { get { return deepLinkedFolders + deepLinkedMods; } }
    public int shownChildrenAndSelf { get { return isHidden ? 0 : 1 + children.Select(p => p.shownChildrenAndSelf).Sum(); } }
    public int nestLevel { get { return parent == null ? 0 : parent.nestLevel + 1; } }

    public ModPage(DirectoryInfo _path, bool _isMod) {
        path = _path;
        isMod = _isMod;
    }
}
