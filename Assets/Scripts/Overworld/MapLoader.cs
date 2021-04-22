using System.Xml;

public static class MapLoader {
    public static void LoadMap(string path) {
        XmlDocument xml = new XmlDocument();
        string xmlPath = path + ".xml";
        if (FileLoader.SanitizePath(ref xmlPath, "Sprites/UI/Fonts/", false))
            return;
        xml.Load(xmlPath);
    }
}
