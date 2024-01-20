using System.Linq;
using TMPro;
using UnityEngine;

public class BattleParticipant : MonoBehaviour
{
	Rigidbody2D rb;
	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	public float Health { get; private set; }
	public enum Team
	{
		Player = 0,
		Enemy = 1
	}
	public Team team;

	[Header("Battle Attributes")]
	public float maxHealth;
	public float damagePerHit;
	public float hitsPerSecond;
	public float range;
	public float speed;
	public float rotationSpeed;

	BattleParticipant target;

	private void Start()
	{
		BattleManager.instance.participants.Add(this);
		Health = maxHealth;
	}

	private void Update()
	{
		if (target == null)
		{
			target = BattleManager.instance.participants.Where(p => p.team != team).OrderBy(p => Vector2.Distance(transform.position, p.transform.position)).FirstOrDefault();
			Debug.Log($"{gameObject.name}'s new target: {target.name}");
		}

		//rotate towards target
		float deltaAngle = Vector2.SignedAngle(transform.right, target.transform.position - transform.position);
		if (deltaAngle > 0f) rb.rotation += Mathf.Min(rotationSpeed * Time.deltaTime, deltaAngle);
		else rb.rotation += Mathf.Max(-rotationSpeed * Time.deltaTime, deltaAngle);

		if (Vector2.Distance(transform.position, target.transform.position) > range)
		{
			//move into range
		}
	}


	public void ChangeHealth(float amount)
	{
		Health += amount;
		if (Health <= 0) Die();
		else if (Health > maxHealth) Health = maxHealth;
	}

	void Die()
	{
		Debug.Log($"{gameObject.name} has died!");
		BattleManager.instance.participants.Remove(this);
		gameObject.SetActive(false);
	}
}
