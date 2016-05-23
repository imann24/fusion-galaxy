public class Credit {
	public string name {get; private set;}
	public string [] names {get; private set;}
	public string role {get; private set;}
	public string college {get; private set;}
	public bool isHeader {get; private set;}
	public bool isRole {get; private set;}
	public Type type;
	public enum Type {Name, ThreeName, NameAndCollege, Header, Role};

	/// <summary>
	/// Credit type is set by an enum and stores its data in strings
	/// </summary>
	/// <param name="type">Type.</param> the type of the credit
	/// <param name="list">List.</param> the list of strings the credit stores
	public Credit (Type type, params string[]list) {
		this.type = type;
		if (type == Type.Name) {
			this.name = list[0];
		} else if (type == Type.ThreeName) {
			this.names = list;
		} else if (type == Type.NameAndCollege) {
			this.name = list[0];
			this.college = list[1];
		} else if (type == Type.Header) {
			this.name = list[0];
			isHeader = true;
		} else if (type == Type.Role) {
			this.name = list[0];
			isRole = true;
		}
	}
}
