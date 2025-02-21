using UnityEngine;

public class UM_MovePathScript : MonoBehaviour, IUpdateable 
{

	public UM_EditorPath PathFollow;
	public int currentWayPointID = 0;
	public float speed = 1f;
	private float reachDisance = 1.0f;
	public float rotationSpeed = 5.0f;
	public string pathName;
	[SerializeField] private bool disableOnEnd = false;

	Vector3 last_position;
	Vector3 current_position;

	void Start ()  => last_position = transform.position;
	
	public void UpdateMe () {

		float distance = Vector3.Distance (PathFollow.path_objs[currentWayPointID].position ,transform.position );
		transform.position = Vector3.MoveTowards (transform.position, PathFollow.path_objs [currentWayPointID].position, Time.deltaTime * speed);

		var rotation = Quaternion.LookRotation (PathFollow.path_objs [currentWayPointID].position - transform.position);
		transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * rotationSpeed);

		if (distance <= reachDisance) { currentWayPointID++; }

		if (currentWayPointID >= PathFollow.path_objs.Count) 
		{
			if (!disableOnEnd)
			{
				currentWayPointID = 0;

				return;
			}
			
			if (disableOnEnd)
			{
				this.enabled = false;

				return;
			}
        }
	}

    private void OnEnable()
	{
		MovePathUpdateManager.OnUpdate += UpdateMe;
	}
	private void OnDisable()
	{
		MovePathUpdateManager.OnUpdate -= UpdateMe;
	}
}
