using MoonSharp.Interpreter;

public static class MoonSharpUtil {
    public static DynValue CloneIfRequired(Script newOwner, DynValue value) {
        if (value == null)                return null;
        if (value.Type == DataType.Table) return RecursiveTableOwnership(newOwner, value);
        return value;
    }

    private static DynValue RecursiveTableOwnership(Script newOwner, DynValue tableSource) {
        DynValue t = DynValue.NewTable(newOwner);
        foreach (TablePair pair in tableSource.Table.Pairs) {
            DynValue val = pair.Value;
            if (val.Type == DataType.Table) {
                DynValue newOwnedSubtable = RecursiveTableOwnership(newOwner, val);
                t.Table.Set(pair.Key, newOwnedSubtable);
            } else if (val.Type == DataType.Function)
                t.Table.Set(pair.Key, DynValue.NewClosure(pair.Value.Function));
            else
                t.Table.Set(pair.Key, pair.Value);
        }
        return t;
    }
}
