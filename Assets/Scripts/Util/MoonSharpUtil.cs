using MoonSharp.Interpreter;

public static class MoonSharpUtil {
    public static DynValue CloneIfRequired(Script newOwner, DynValue value) {
        return value == null ? null : value.Type == DataType.Table ? RecursiveTableOwnership(newOwner, value) : value;
    }

    private static DynValue RecursiveTableOwnership(Script newOwner, DynValue tableSource) {
        DynValue t = DynValue.NewTable(newOwner);
        foreach (TablePair pair in tableSource.Table.Pairs) {
            DynValue val = pair.Value;
            switch (val.Type) {
                case DataType.Table: {
                    DynValue newOwnedSubtable = RecursiveTableOwnership(newOwner, val);
                    t.Table.Set(pair.Key, newOwnedSubtable);
                    break;
                }
                case DataType.ClrFunction: t.Table.Set(pair.Key, DynValue.NewCallback(pair.Value.Callback)); break;
                case DataType.Function:    t.Table.Set(pair.Key, DynValue.NewClosure(pair.Value.Function));  break;
                default:                   t.Table.Set(pair.Key, pair.Value);                                break;
            }
        }
        return t;
    }
}
