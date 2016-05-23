public class CollectAll : PowerUp {

    public CollectAll (): base ("CollectAll", null) {}

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