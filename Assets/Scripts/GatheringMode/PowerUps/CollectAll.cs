public class CollectAll : PowerUp {
	public const string NAME = "CollectAll";
	public readonly static string [] DESCRIPTIONS = new string[]{
		"Tap or drag this ability into a specific lane to automatically collect all elements in that lane.",//power 9, level 1
		"Tap or drag this ability into a specific lane to automatically collect all elements in that lane and an adjacent lane.",//power 9, level 2
		"Tap or drag this ability to automatically collect all elements in all lanes.",//power 9, level 3
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