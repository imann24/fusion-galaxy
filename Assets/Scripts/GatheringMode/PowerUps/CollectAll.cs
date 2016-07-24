public class CollectAll : PowerUp {
	public const string NAME = "CollectAll";
	public readonly static string [] DESCRIPTIONS = new string[]{
		"Automatically collect elements, when fully upgraded it collects all elements on screen.",
		"Automatically collect elements, when fully upgraded it collects all elements on screen.",
		"Automatically collect elements, when fully upgraded it collects all elements on screen.",
	};

	public CollectAll (): base (NAME, DESCRIPTIONS, null) {}

    #region implemented abstract members of PowerUp
	public override void usePowerUp (int lane) {
	
		if (level == 1) { //collects all elements in one lane
			controller.collectAllElementsInLane(new int[1]{lane});
		} else if (level == 2) { //collects all elements in two lanes
			controller.collectAllElementsInLane(new int[2]{lane, getSecondLane(lane)}); 
		} else if (level == 3) { //collects all elements
			controller.collectAllOnscreenElements();
		}

    }
    #endregion
}