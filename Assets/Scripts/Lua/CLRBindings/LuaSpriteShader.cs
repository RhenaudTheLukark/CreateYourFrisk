using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;
using System.Collections.Generic;

public class LuaSpriteShader {
    private string mode = "sprite";
    private GameObject gameObject;
    [MoonSharpHidden] public Material material;
    [MoonSharpHidden] public bool _isActive = false;
    private TextureWrapMode H = TextureWrapMode.Clamp;
    private TextureWrapMode V = TextureWrapMode.Clamp;
    private Dictionary<string, int> propertyIDs = new Dictionary<string, int>();

    public LuaSpriteShader(string mode = "sprite", GameObject go = null) {
        this.mode = mode;
        this.gameObject = go;

        if (mode == "sprite")       this.material = go.GetComponent<Image>().material;
        else if (mode == "event")   this.material = go.GetComponent<SpriteRenderer>().material;
        else if (mode == "camera")  this.material = go.GetComponent<CameraShader>().material;
    }

    public void Set(string bundleName, string shaderName) {
        if (bundleName == null)
            throw new CYFException("shader.Set: The first argument, the name of the AssetBundle to load, is nil.");
        else if (shaderName == null)
            throw new CYFException("shader.Set: The second argument, the name of the shader to load, is nil.");

        material = Material.Instantiate(ShaderRegistry.Get(bundleName, shaderName));
        if (mode == "camera") {
            CameraShader cs = gameObject.GetComponent<CameraShader>();
            cs.enabled = true;
            cs.material = material;
        } else if (mode == "event")
            gameObject.GetComponent<SpriteRenderer>().material = material;
        else
            gameObject.GetComponent<Image>().material = material;
        _isActive = true;
    }

    #if UNITY_EDITOR
        public void Test(string shaderName) {
            if (shaderName == null)
                throw new CYFException("shader.Test: The first argument, the name of the shader to load, is nil.");

            try {
                material = new Material(Shader.Find(shaderName));
            } catch { throw new CYFException("The shader \"" + shaderName + "\" could not be found."); }
            if (mode == "camera") {
                CameraShader cs = gameObject.GetComponent<CameraShader>();
                cs.enabled = true;
                cs.material = material;
            } else if (mode == "event")
                gameObject.GetComponent<SpriteRenderer>().material = material;
            else
                gameObject.GetComponent<Image>().material = material;
            _isActive = true;
        }
    #else
        public void Test(string shaderName) {
            throw new CYFException("shader.Test may only be used from within the Unity editor.");
        }
    #endif

    public void Revert() {
        material = ShaderRegistry.UI_DEFAULT_MATERIAL;
        if (mode == "camera") {
            CameraShader cs = gameObject.GetComponent<CameraShader>();
            cs.enabled = false;
            cs.material = material;
        } else if (mode == "event")
            gameObject.GetComponent<SpriteRenderer>().material = material;
        else
            gameObject.GetComponent<Image>().material = material;
        _isActive = false;
    }

    private void checkActive() {
        if (!_isActive)
            throw new CYFException("Attempted to perform action on inactive shader.");
    }

    public bool isactive { get { return _isActive; } }
    public bool isActive { get { return _isActive; } }

    [MoonSharpHidden] public void UpdateTexture(Texture t) {
        t.wrapModeU = H;
        t.wrapModeV = V;
    }

    public void SetWrapMode(string wrapMode, int sides = 0) {
        checkActive();
        TextureWrapMode newMode = TextureWrapMode.Clamp;
        if (wrapMode == "repeat")          newMode = TextureWrapMode.Repeat;
        else if (wrapMode == "mirror")     newMode = TextureWrapMode.Mirror;
        else if (wrapMode == "mirroronce") newMode = TextureWrapMode.MirrorOnce;

        if (sides == 0) {
            H = newMode;
            V = newMode;
        } else if (sides == 1) {
            H = newMode;
        } else
            V = newMode;

        if (mode != "camera")
            UpdateTexture(mode == "event" ? gameObject.GetComponent<SpriteRenderer>().sprite.texture : gameObject.GetComponent<Image>().mainTexture);
        else {
            CameraShader.H = H;
            CameraShader.V = V;
        }
    }

    private int IndexProperty(string name, bool get) {
        checkActive();
        if (!material.HasProperty(name) && get)
            throw new CYFException("Shader has no property \"" + name + "\".");

        if (!propertyIDs.ContainsKey(name))
            propertyIDs[name] = Shader.PropertyToID(name);
        return propertyIDs[name];
    }

    public bool HasProperty(string name) { return material.HasProperty(name); }



    public DynValue GetColor(string name) {
        Color color = material.GetColor(IndexProperty(name, true));
        Table output = new Table(null);
        output.Set(1, DynValue.NewNumber(color.r));
        output.Set(2, DynValue.NewNumber(color.g));
        output.Set(3, DynValue.NewNumber(color.b));
        output.Set(4, DynValue.NewNumber(color.a));
        return DynValue.NewTable(output);
    }
    public void SetColor(string name, DynValue value) {
        if (value.Type != DataType.Table || value.Table.Length < 3)
            throw new CYFException("shader.SetColor: The second argument, the color, needs to be a table with 3 or 4 numbers.");

        Vector4 v4output = new Vector4((float)value.Table.Get(1).Number,
                                       (float)value.Table.Get(2).Number,
                                       (float)value.Table.Get(3).Number,
                                       value.Table.Length > 3 ? (float)value.Table.Get(4).Number : 1f);
        material.SetColor(IndexProperty(name, false), v4output);
    }



    public DynValue GetColorArray(string name) {
        Color[] colors = material.GetColorArray(IndexProperty(name, true));
        Table output = new Table(null);
        for (var i = 0; i < colors.Length; i++) {
            Color color = colors[i];
            Table colorTable = new Table(null);
            colorTable.Set(1, DynValue.NewNumber(color.r));
            colorTable.Set(2, DynValue.NewNumber(color.g));
            colorTable.Set(3, DynValue.NewNumber(color.b));
            colorTable.Set(4, DynValue.NewNumber(color.a));
            output.Set(i + 1, DynValue.NewTable(colorTable));
        }
        return DynValue.NewTable(output);
    }
    public void SetColorArray(string name, DynValue value) {
        if (value.Type != DataType.Table)
            throw new CYFException("shader.SetColorArray: The second argument, the table of colors, needs to be a table.");

        Color[] colorarray = new Color[value.Table.Length];
        for (int i = 0; i < value.Table.Length; i++) {
            DynValue item = value.Table.Get(i + 1);
            if (item.Type != DataType.Table || item.Table.Length < 3)
                throw new CYFException("shader.SetColorArray: Item #" + (i + 1).ToString() + " needs to be a table with 3 or 4 numbers.");

            Color newColor = new Color((float)item.Table.Get(1).Number,
                                       (float)item.Table.Get(2).Number,
                                       (float)item.Table.Get(3).Number,
                                       item.Table.Length > 3 ? (float)item.Table.Get(4).Number : 1f);
            colorarray[i] = newColor;
        }
        material.SetColorArray(IndexProperty(name, false), colorarray);
    }



    public float GetFloat(string name) {
        return material.GetFloat(IndexProperty(name, true));
    }
    public void SetFloat(string name, float value) {
        material.SetFloat(IndexProperty(name, false), value);
    }



    public float[] GetFloatArray(string name) {
        return material.GetFloatArray(IndexProperty(name, true));
    }
    public void SetFloatArray(string name, float[] value) {
        material.SetFloatArray(IndexProperty(name, false), value);
    }



    public int GetInt(string name) {
        return material.GetInt(IndexProperty(name, true));
    }
    public void SetInt(string name, int value) {
        material.SetInt(IndexProperty(name, false), value);
    }



    public class MatrixFourByFour {
        [MoonSharpHidden] public Matrix4x4 self = Matrix4x4.zero;

        public MatrixFourByFour(Matrix4x4 matrix) { self = matrix; }

        public MatrixFourByFour(DynValue row1, DynValue row2, DynValue row3, DynValue row4) {
            if (row1 == null || row1.Type != DataType.Table || row1.Table.Length < 4)
                throw new CYFException("shader.Matrix: The first argument needs to be a table of 4 numbers.");
            if (row2 == null || row2.Type != DataType.Table || row2.Table.Length < 4)
                throw new CYFException("shader.Matrix: The first argument needs to be a table of 4 numbers.");
            if (row3 == null || row3.Type != DataType.Table || row3.Table.Length < 4)
                throw new CYFException("shader.Matrix: The first argument needs to be a table of 4 numbers.");
            if (row4 == null || row4.Type != DataType.Table || row4.Table.Length < 4)
                throw new CYFException("shader.Matrix: The first argument needs to be a table of 4 numbers.");

            Table t1 = row1.Table;
            Table t2 = row2.Table;
            Table t3 = row3.Table;
            Table t4 = row4.Table;
            self.SetRow(0, new Vector4((float)t1.Get(1).Number,
                                       (float)t1.Get(2).Number,
                                       (float)t1.Get(3).Number,
                                       (float)t1.Get(4).Number));
            self.SetRow(1, new Vector4((float)t2.Get(1).Number,
                                       (float)t2.Get(2).Number,
                                       (float)t2.Get(3).Number,
                                       (float)t2.Get(4).Number));
            self.SetRow(2, new Vector4((float)t3.Get(1).Number,
                                       (float)t3.Get(2).Number,
                                       (float)t3.Get(3).Number,
                                       (float)t3.Get(4).Number));
            self.SetRow(3, new Vector4((float)t4.Get(1).Number,
                                       (float)t4.Get(2).Number,
                                       (float)t4.Get(3).Number,
                                       (float)t4.Get(4).Number));
        }

        public float this[int row, int column] {
            get {
                if (row < 1 || row > 4)
                    throw new CYFException("Row must be between 1 and 4.");
                else if (column < 1 || column > 4)
                    throw new CYFException("Column must be between 1 and 4.");

                return self[row - 1, column - 1];
            }
            set {
                if (row < 1 || row > 4)
                    throw new CYFException("Row must be between 1 and 4.");
                else if (column < 1 || column > 4)
                    throw new CYFException("Column must be between 1 and 4.");

                self[row - 1, column - 1] = value;
            }
        }
    }

    public MatrixFourByFour Matrix(DynValue row1, DynValue row2, DynValue row3, DynValue row4) { return new MatrixFourByFour(row1, row2, row3, row4); }

    public MatrixFourByFour GetMatrix(string name) {
        return new MatrixFourByFour(material.GetMatrix(IndexProperty(name, true)));
    }
    public void SetMatrix(string name, MatrixFourByFour value) {
        material.SetMatrix(IndexProperty(name, false), value.self);
    }



    public MatrixFourByFour[] GetMatrixArray(string name) {
        Matrix4x4[] matrices = material.GetMatrixArray(IndexProperty(name, true));
        MatrixFourByFour[] output = new MatrixFourByFour[matrices.Length];

        for (int i = 0; i < matrices.Length; i++)
            output[i] = new MatrixFourByFour(matrices[i]);

        return output;
    }
    public void SetMatrixArray(string name, MatrixFourByFour[] value) {
        if (value.Length < 4)
            throw new CYFException("shader.SetMatrixArray: The second argument, the table of matrices, needs to be a table with shader matrix objects.");

        Matrix4x4[] matrixArray = new Matrix4x4[value.Length];

        for (int i = 0; i < value.Length; i++)
            matrixArray[i] = value[i].self;

        material.SetMatrixArray(IndexProperty(name, false), matrixArray);
    }



    public void SetTexture(string name, string sprite) {
        Sprite spr = SpriteRegistry.Get(sprite);
        material.SetTexture(IndexProperty(name, false), spr.texture);
    }

    public DynValue GetVector(string name) {
        Vector4 vector = material.GetVector(IndexProperty(name, true));
        Table output = new Table(null);
        output.Set(1, DynValue.NewNumber(vector.w));
        output.Set(2, DynValue.NewNumber(vector.x));
        output.Set(3, DynValue.NewNumber(vector.y));
        output.Set(4, DynValue.NewNumber(vector.z));
        return DynValue.NewTable(output);
    }
    public void SetVector(string name, DynValue value) {
        if (value.Type != DataType.Table || value.Table.Length < 4)
            throw new CYFException("shader.SetVector: The second argument, the vector, needs to be a table with 4 numbers.");

        Vector4 v4output = new Vector4((float)value.Table.Get(1).Number,
                                       (float)value.Table.Get(2).Number,
                                       (float)value.Table.Get(3).Number,
                                       (float)value.Table.Get(4).Number);
        material.SetVector(IndexProperty(name, false), v4output);
    }



    public DynValue GetVectorArray(string name) {
        Vector4[] vectors = material.GetVectorArray(IndexProperty(name, true));
        Table output = new Table(null);
        for (var i = 0; i < vectors.Length; i++) {
            Vector4 vector = vectors[i];
            Table vectorTable = new Table(null);
            vectorTable.Set(1, DynValue.NewNumber(vector.w));
            vectorTable.Set(2, DynValue.NewNumber(vector.x));
            vectorTable.Set(3, DynValue.NewNumber(vector.y));
            vectorTable.Set(4, DynValue.NewNumber(vector.z));
            output.Set(i + 1, DynValue.NewTable(vectorTable));
        }
        return DynValue.NewTable(output);
    }
    public void SetVectorArray(string name, DynValue value) {
        if (value.Type != DataType.Table)
            throw new CYFException("shader.SetVectorArray: The second argument, the table of vectors, needs to be a table.");

        Vector4[] v4array = new Vector4[value.Table.Length];
        for (int i = 0; i < value.Table.Length; i++) {
            DynValue item = value.Table.Get(i + 1);
            if (item.Type != DataType.Table || item.Table.Length < 4)
                throw new CYFException("shader.SetVectorArray: Item #" + (i + 1).ToString() + " needs to be a table with 4 numbers.");

            Vector4 newv4 = new Vector4((float)item.Table.Get(1).Number,
                                        (float)item.Table.Get(2).Number,
                                        (float)item.Table.Get(3).Number,
                                        (float)item.Table.Get(4).Number);
            v4array[i] = newv4;
        }
        material.SetVectorArray(IndexProperty(name, false), v4array);
    }
}
