using System.Collections;
using System.Linq;
using UnityEngine;

public class BattleParticipant : MonoBehaviour
{
	Rigidbody2D rb;
	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	public float Health { get; private set; }

	public BattleManager.Team team;
	[SerializeField] bool enableHealthBar = true;
	HealthBar healthBar;

	[Header("Battle Attributes")]
	public float maxHealth;
	public float damagePerHit;
	public float hitsPerSecond;
	bool canHit = true;
	public float range;
	public float speed;
	public float rotationSpeed;

	BattleParticipant target;

	[SerializeField][Tooltip("Whether the participant should move back to their desired range when their target gets too close")] bool shouldMoveBack = false;
	bool moveBack = false;

	public void Setup(CharacterData data)
	{
		maxHealth = data.maxHealth;
		damagePerHit = data.damagePerHit;
		hitsPerSecond = data.hitsPerSecond;
		range = data.range;
		speed = data.speed;
		rotationSpeed = data.rotationSpeed;
		shouldMoveBack = data.shouldMoveBack;
	}


	private void Start()
	{
		BattleManager.instance.participants.Add(this);
		Health = maxHealth;

		if (enableHealthBar)
		{
			GameObject obj = Instantiate(BattleManager.instance.healthBarPrefab, BattleManager.instance.worldSpaceCanvas.transform);
			obj.name = $"HealthBar ({name})";
			healthBar = obj.GetComponent<HealthBar>();
			healthBar.min = 0f;
			healthBar.max = maxHealth;
			healthBar.disappearOnZero = true;
		}
	}

	private void Update()
	{
		healthBar.SetValue(Health);
		healthBar.transform.position = (Vector2)transform.position + new Vector2(0, transform.localScale.y + .25f);

		if (target == null || target.Health <= 0f)
		{
			target = null;
			target = BattleManager.instance.participants.Where(p => p.team != team).OrderBy(p => Vector2.Distance(transform.position, p.transform.position)).FirstOrDefault();
			if (target == null)
			{
				Debug.Log(team + " won!!! yippie!");
				Debug.Break();
				return;
			}
			Debug.Log($"{gameObject.name}'s new target: {target.name}");
		}

		//rotate towards target
		float deltaAngle = Vector2.SignedAngle(transform.right, target.transform.position - transform.position);
		if (deltaAngle > 0f) rb.rotation += Mathf.Min(rotationSpeed * Time.deltaTime, deltaAngle);
		else rb.rotation += Mathf.Max(-rotationSpeed * Time.deltaTime, deltaAngle);

		//move into range
		float distance = Vector2.Distance(transform.position, target.transform.position);
		if (shouldMoveBack && moveBack && Mathf.Abs(range - distance) <= .2f) moveBack = false;
		if (Mathf.Abs(deltaAngle) <= 5f) //is looking at target?
		{
			if (distance > range) rb.MovePosition((Vector2)transform.position + speed * Time.deltaTime * (Vector2)transform.right);
			else
			{
				//handle moving back
				if (shouldMoveBack)
				{
					if (distance < range * .6f) moveBack = true;
					if (moveBack) rb.MovePosition((Vector2)transform.position - speed * Time.deltaTime * (Vector2)transform.right);
				}
				//handle hits
				if (canHit)
				{
					target.ChangeHealth(-damagePerHit);
					Debug.Log($"{gameObject.name} hit {target.name} for {damagePerHit} damage ({target.Health})");
					StartCoroutine(HitCooldown());
				}
			}
		}
	}

	private void OnEnable()
	{
		if (healthBar != null) healthBar.gameObject.SetActive(true);
	}
	private void OnDisable()
	{
		if (healthBar != null) healthBar.gameObject.SetActive(false);
	}
	private void OnDestroy()
	{
		if (healthBar != null) Destroy(healthBar.gameObject);
	}

	IEnumerator HitCooldown()
	{
		canHit = false;
		yield return new WaitForSeconds(1f / hitsPerSecond);
		canHit = true;
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
