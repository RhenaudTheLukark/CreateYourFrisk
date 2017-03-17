public class MonsterMessage : TextMessage {
    public MonsterMessage(string text) : base("[effect:rotate]" + text, false, false) { }
}