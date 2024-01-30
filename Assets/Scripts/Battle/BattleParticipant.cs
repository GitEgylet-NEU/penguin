using NohaSoftware.Utilities;
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

	[SerializeField] bool enableHealthBar = true;
	HealthBar healthBar;
	HealthBar abilityBar;

	public BattleManager.Team team;
	public CharacterData Data { get; private set; }

	BattleParticipant target;

	bool canHit = true;
	bool isMovingBack = false;
	float damageSinceLastAbility;

	public void Setup(CharacterData data)
	{
		this.Data = data;
		Health = data.maxHealth;
	}


	private void Start()
	{
		if (Data == null) return;

		BattleManager.instance.participants.Add(this);

		if (enableHealthBar)
		{
			GameObject obj = Instantiate(BattleManager.instance.healthBarPrefab, BattleManager.instance.worldSpaceCanvas.transform);
			obj.name = $"HealthBar ({name})";
			healthBar = obj.GetComponent<HealthBar>();
			healthBar.min = 0f;
			healthBar.max = Data.maxHealth;
			healthBar.disappearOnZero = true;
		}
		if (Data.hasAbility && Data.ability.abilityCost >= 0f)
		{
			GameObject obj = Instantiate(BattleManager.instance.abilityBarPrefab, BattleManager.instance.worldSpaceCanvas.transform);
			obj.name = $"AbilityBar ({name})";
			abilityBar = obj.GetComponent<HealthBar>();
			abilityBar.min = 0f;
			abilityBar.max = Data.ability.abilityCost;
			abilityBar.disappearOnZero = false;
		}
	}

	private void Update()
	{
		healthBar.SetValue(Health);
		healthBar.transform.position = (Vector2)transform.position + new Vector2(0, transform.localScale.y + .25f);
		if (abilityBar != null)
		{
			abilityBar.SetValue(damageSinceLastAbility);
			abilityBar.transform.position = (Vector2)transform.position + new Vector2(0, transform.localScale.y + .15f);
		}

		if (target == null || target.Health <= 0f)
		{
			target = null;
			target = BattleManager.instance.participants.Where(p => p.team != team).OrderBy(p => Vector2.Distance(transform.position, p.transform.position)).FirstOrDefault();
			if (target == null) return;
			//Debug.Log($"{gameObject.name}'s new target: {target.name}");
		}

		//rotate towards target
		float deltaAngle = Vector2.SignedAngle(transform.right, target.transform.position - transform.position);
		if (deltaAngle > 0f) rb.rotation += Mathf.Min(Data.rotationSpeed * Time.deltaTime, deltaAngle);
		else rb.rotation += Mathf.Max(-Data.rotationSpeed * Time.deltaTime, deltaAngle);

		//move into range
		float distance = Vector2.Distance(transform.position, target.transform.position);
		if (Data.shouldMoveBack && isMovingBack && Mathf.Abs(Data.range - distance) <= .2f) isMovingBack = false;
		if (Mathf.Abs(deltaAngle) <= 5f) //is looking at target?
		{
			if (distance > Data.range) rb.MovePosition((Vector2)transform.position + Data.speed * Time.deltaTime * (Vector2)transform.right);
			else
			{
				// move back
				if (Data.shouldMoveBack)
				{
					if (distance < Data.range * .6f) isMovingBack = true;
					if (isMovingBack) rb.MovePosition((Vector2)transform.position - Data.speed * Time.deltaTime * (Vector2)transform.right);
				}
				// hit
				if (canHit)
				{
					target.ChangeHealth(-Data.damagePerHit);
					if (Data.hasAbility) damageSinceLastAbility += Data.damagePerHit;
					//Debug.Log($"{gameObject.name} hit {target.name} for {damagePerHit} damage ({target.Health})");
					StartCoroutine(HitCooldown());
				}
			}
		}

		if (Data.hasAbility && damageSinceLastAbility >= Data.ability.abilityCost)
		{
			if (CastAbility()) damageSinceLastAbility = 0;
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
		yield return new WaitForSeconds(1f / Data.hitsPerSecond);
		canHit = true;
	}

	public void ChangeHealth(float amount)
	{
		Health += amount;
		if (Health <= 0) Die();
		else if (Health > Data.maxHealth) Health = Data.maxHealth;
	}

	void Die()
	{
		Debug.Log($"{gameObject.name} has died!");
		BattleManager.instance.participants.Remove(this);
		gameObject.SetActive(false);
	}

	bool CastAbility()
	{

		switch (Data.ability.id)
		{
			case "targeted_heal":
				var query = BattleManager.instance.participants.Where(p => p != this && p.team == team && p.Health <= p.Data.maxHealth);
				if (!query.Any())
				{
					if (Health <= Data.maxHealth)
					{
						Debug.Log("cast on self");
						ChangeHealth(Data.ability.floats.GetElement("heal_amount").Value);
						return true;
					}
				}
				else
				{
					var other = query.OrderBy(p => p.Health).First();
					Debug.Log("cast on " + other.name);
					other.ChangeHealth(Data.ability.floats.GetElement("heal_amount").Value);
					return true;
				}
				return false;
			default:
				return false;
		}
	}
}
