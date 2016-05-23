//model for the combination of two elements to form a third
public class Combination {
	private string parent1;
	private string parent2;
	private Element result;

	//constructor takes the strings for both the elements and the new element they create
	public Combination (string p1, string p2, Element r) {
		parent1 = p1;
		parent2 = p2;
		result = r;
	}
	
	//returns the element
	public Element getResult () {
		return result;
	}

	//returns the parents as an array
	public string [] getParents () {
		return new string[]{parent1, parent2};
	}

	//print combination for debugging 
	public override string ToString () {
		return parent1 + " + " + parent2 + "= " + result.getName();
	}
	// get the first parent
	public string getFirstParent(){
		return parent1;
	}
	// get the second parent
	public string getSecondParent(){
		return parent2;
	}

}