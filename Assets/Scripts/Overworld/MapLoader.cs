using System.Xml;

public static class MapLoader {
    public static void LoadMap(string path) {
        XmlDocument xml = new XmlDocument();
        string xmlPath = FileLoader.requireFile("Sprites/UI/Fonts/" + path + ".xml", false);
        if (xmlPath == null)
            return;
        xml.Load(xmlPath);
    }
}
