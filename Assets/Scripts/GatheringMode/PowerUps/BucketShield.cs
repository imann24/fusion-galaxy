public class BucketShield : PowerUp {
	private int bucketShieldHitPoints;
	private int bucketShieldHitPointsMultiplier = 2;

	//constructor that takes the number of hit points for the bucket shield
	public BucketShield (int bucketShieldHitPoints): base ("BucketShield", null) {
		this.bucketShieldHitPoints = bucketShieldHitPoints;
	}
	
	#region implemented abstract members of PowerUp
	public override void usePowerUp (int lane) {
		
		if (level == 1) { //collects all elements in one lane
			allZones[lane].bucketShieldActive(bucketShieldHitPoints);
		} else if (level == 2) { //collects all elements in two lanes
			allZones[lane].bucketShieldActive(bucketShieldHitPoints * bucketShieldHitPointsMultiplier);
		} else if (level == 3) { //collects all elements
			foreach (ZoneCollisionDetection zone in allZones) {
				zone.bucketShieldActive(bucketShieldHitPoints * bucketShieldHitPointsMultiplier);
			}
		}
		
	}
	#endregion
}