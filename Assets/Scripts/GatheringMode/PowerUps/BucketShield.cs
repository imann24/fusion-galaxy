public class BucketShield : PowerUp {
	public const string NAME = "BucketShield";
	public static readonly string [] DESCRIPTIONS = new string[]{
		"Tap or drag this ability into a specific lane to give that lane no miss penalty for the next 2 incorrect elements that fall into it.",//power 5, level 1
		"Tap or drag this ability into a specific lane to give that lane no miss penalty for the next 4 incorrect elements that fall into it.",//power 5, level 2
		"Tap or drag this ability to give all lanes no miss penalty for the next 4 incorrect elements that fall into them, for each lane.",//power 5, level 3
	};

	private int bucketShieldHitPoints;
	private int bucketShieldHitPointsMultiplier = 2;

	//constructor that takes the number of hit points for the bucket shield
	public BucketShield (int bucketShieldHitPoints): base (NAME, DESCRIPTIONS, null) {
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